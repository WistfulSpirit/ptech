using Message;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFibonacciAPI
{
    public class FibonacciServiceDispatcher
    {
        private ConcurrentDictionary<int, long> fibonacciClients;
        private object _olock;
        public FibonacciServiceDispatcher()
        {
            fibonacciClients = new ConcurrentDictionary<int, long>();
            _olock = new object();
        }
        public FibonacciMessage ProcessRequest(int id, long num)
        {
            if (fibonacciClients.ContainsKey(id) && num == -200){
                fibonacciClients.Remove(id, out long _);
                return new FibonacciMessage() { ClientId = -1, Number = -200 };
            }
            
            if (!fibonacciClients.ContainsKey(id)) {
                fibonacciClients[id] = 0;
            }
            try {
                fibonacciClients[id]= checked(fibonacciClients[id] + num);
            } catch (OverflowException) {
                return new FibonacciMessage() { ClientId = -1, Number = -200 };
            }
            return new FibonacciMessage() { ClientId = id, Number = fibonacciClients[id] };
        }
    }
}
