using EasyNetQ;
using Message;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleFibonacciAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FibonacciApiController : ControllerBase
    {
        private readonly IBus bus;
        private readonly FibonacciServiceDispatcher dispatcher;
        private readonly object locker = new object();
        public FibonacciApiController(IBus bus, FibonacciServiceDispatcher dispatcher) {
            this.bus = bus;
            this.dispatcher = dispatcher;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> PostFibonacciPart([FromBody] FibonacciMessage msg) {
            Task.Run(() => SendBackAsync(msg));
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        [HttpGet]
        public HttpResponseMessage GetConnectionReady()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private async Task SendBackAsync(FibonacciMessage msg) {
            var bckMsg = dispatcher.ProcessRequest(msg.ClientId, msg.Number);
            if (bckMsg.ClientId != -1) {
                bus.PubSub.PublishAsync<FibonacciMessage>(bckMsg);
            }
        }
    }
}
