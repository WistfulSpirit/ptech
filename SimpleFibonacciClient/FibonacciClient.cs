using EasyNetQ;
using Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFibonacciClient
{
    public class FibonacciClient : IDisposable
    {
        public static readonly int DefaultSeriesLength = 50;
        public static readonly int MaxSeriesLength = 90;
        public static string Port { get; set; }
        private int id;
        private int seriesLength;
        private int currentLength = 0;
        private long curNum = 1;
        private ISubscriptionResult result;
        private IBus localBus;
        private readonly HttpClient httpClient;
        private CancellationTokenSource cancellationTokenSource;
        public CancellationToken CancellationToken => cancellationTokenSource.Token;
        public FibonacciClient(int id, int length, HttpClient client, IBus bus) {
            cancellationTokenSource = new CancellationTokenSource();
            localBus = bus;
            httpClient = client;
            this.id = id;
            seriesLength = length;
            StartBusListening();
        }
        private async Task SendRequestCore(long value) {
            currentLength++;
            try {
                var response = await httpClient.PostAsJsonAsync(new Uri(@$"https://localhost:{Port}/api/FibonacciApi/"),new FibonacciMessage() { ClientId = id, Number = value}, new CancellationTokenSource(10000).Token);
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Error during sending data to server. Client id = {id}. Reason {ex.Message}");
                UnsubscribeFromQueue();
                EndJob(false);
            }
        }
        public void SendRequest() {
            SendRequestCore(curNum);
        }
        private async Task StartBusListening() {
            try {
                result = await localBus.PubSub.SubscribeAsync<FibonacciMessage>($"client_{id}", msg => Task.Run(() => HandleRecievedMessage(msg)), x=>x.WithAutoDelete(), new CancellationTokenSource(10000).Token);
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Error on SubscribeAsync. Client id = {id}. Reason {ex.Message}");
                EndJob(false);
            }
        }
        public void BeginWork() {
            Console.WriteLine($"Job begins for ClientId = {id}.");
            SendRequest();
        }
        private void HandleRecievedMessage(FibonacciMessage msg)
        {
            if (msg.ClientId != id)
                return;
            if (GetNextNum(msg.Number) != -1) {
                SendRequest();
            }
            else {
                EndJob(true);
            }
        }
        private void EndJob(bool successful) {
            if (successful) {
                Console.WriteLine($"Job ended successfully for ClientId = {id}. {seriesLength}th numer of Fibonacci series is {curNum}");
                SendRequestCore(-200);
            } else {
                Console.WriteLine($"Job ended with error for ClientId = {id}.");
            }
            cancellationTokenSource.Cancel();
        }
        public long GetNextNum(long recievedNum) {
            currentLength++;
            if (currentLength < seriesLength) {
                try {
                    curNum = curNum + recievedNum;
                }
                catch (OverflowException){
                    Console.WriteLine($"Overflow for {currentLength}th series number. ClientId = {id}.");
                    return -1;
                }
                return curNum;
            }
            else
                return -1;
        }
        private void UnsubscribeFromQueue() {
            if (result != null)
                result.Dispose();
        }
        public void Dispose() {
            UnsubscribeFromQueue();
        }
    }
}
