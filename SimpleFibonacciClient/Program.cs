using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFibonacciClient
{
    class Program
    {
        static int ParseParams(string[] args)
        {
            int result = 5;
            if (args.Length == 0)
                Console.WriteLine($"Where is no initial parameters. Default async operations count {result} will be used");
            else if (args.Length >= 1)
            {
                if (args.Length > 1)
                    Console.WriteLine("Excessive amount of parameters. Only first param won't be ignored");
                if (int.TryParse(args[0], out int res))
                    result = res;
                else
                    Console.WriteLine($"Failed to parse input value, default value ({result}) will be used");
            }
            return result;
        }
        private static readonly HttpClient client = new HttpClient();
        private static readonly IBus bus = RabbitHutch.CreateBus("host=localhost");
        private static async Task<bool> CheckServerActivity(HttpClient httpClient, string port) {
            HttpResponseMessage responce;
            try
            {
                responce = await httpClient.GetAsync(new Uri(@$"https://localhost:{port}/api/FibonacciApi/"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot connect to service. Program will be closed. Reason is {ex.Message}. Inner exception is {ex.InnerException.Message}");
                responce = null;
            }
            return responce != null ? responce.StatusCode == System.Net.HttpStatusCode.OK : false;            
        }

        static async Task Main(string[] args)
        {
            int parallelNum = ParseParams(args);
            Console.WriteLine("Enter localhost port to connect to the service");
            string port = Console.ReadLine();
            if (!await CheckServerActivity(client, port))
                return;
            FibonacciClient.Port = port;    
            Dictionary<int, FibonacciClient> dict = new Dictionary<int, FibonacciClient>();
            int seriesLen;
            Console.Write($"Enter -1 to skip setup for each fibonacci series ({parallelNum}) and use default length param ({FibonacciClient.DefaultSeriesLength}): ");
            bool useDefaultLength = (-1 == int.Parse(Console.ReadLine()));
            for (int i = 0; i < parallelNum; i++) {
                if (!useDefaultLength)
                {
                    Console.Write("Enter Fibonacci series length ");
                    seriesLen = int.Parse(Console.ReadLine());
                    if (seriesLen > FibonacciClient.MaxSeriesLength) {
                        Console.WriteLine($"Max series length limit exceeded, will be used max allowed length {FibonacciClient.MaxSeriesLength}");
                        seriesLen = FibonacciClient.MaxSeriesLength;
                    }

                }
                else
                    seriesLen = FibonacciClient.DefaultSeriesLength;
                dict.Add(i, new FibonacciClient(i, seriesLen, client, bus));
            }
            var tasks = dict.Values.Select(async m =>
            {
                m.BeginWork();
                try
                {
                    await Task.Delay(Timeout.Infinite, m.CancellationToken);
                }
                catch { }
            });
            Task.WaitAll(tasks.ToArray());
            foreach (var item in dict) {
                item.Value.Dispose();
            }
            bus.Dispose();
            Console.WriteLine("AllDone");
        }
    }
}
