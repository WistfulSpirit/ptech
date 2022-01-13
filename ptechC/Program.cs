using EasyNetQ;
using Message;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace ptechC
{
    //Fibonacci -> 0, 1, 1, 2, 3, 5, 8, 13, 21, 34
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
                {
                    result = res;
                }
                else
                    Console.WriteLine($"Failed to parse input value, default value ({result}) will be used");
            }
            return result;
        }
        static async Task Main(string[] args)
        {

            int asyncOperationsCount = ParseParams(args);
            using (var bus = RabbitHutch.CreateBus("host=localhost")) {
                await bus.PubSub.SubscribeAsync<string>("subscriber_id", (string s) => { Console.WriteLine("Number is " + s); });
                Console.ReadLine();
            }
            
            for (int i = 0; i < asyncOperationsCount; i++)
            {
                int seriesLength, firstVal, secondVal;
                Console.WriteLine($"Enter Fibonnacchi series length and two first values: ");
                var input = Console.ReadLine().Split(" ");
                seriesLength = int.Parse(input[0]);
                firstVal = int.Parse(input[1]);
                secondVal = int.Parse(input[2]);
                var message = new FibonacciMessage() { LengthOfSeries = seriesLength, FirstNum = firstVal, SecondNum = secondVal };
                ProccessFibonacci(message);
            }
            
            Console.ReadLine();
        }
        private static readonly HttpClient client = new HttpClient();
        private static async Task ProccessFibonacci(FibonacciMessage message)
        {
            client.PostAsJsonAsync(new Uri(@$"https://localhost:5001/api/fibonacci/{prevNum}"), message);
        }
        public static int prevNum = 5;
    }
}
