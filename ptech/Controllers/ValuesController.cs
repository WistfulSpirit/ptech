using EasyNetQ;
using Message;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ptech.Controllers
{
    [Route("api/fibonacci")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private int curNum;
        [HttpPost]
        public async Task PostBeginFibonacciCount([FromBody] FibonacciMessage message)
        {
            message.
        }
        [HttpPost]
        public async Task<ActionResult<int>> PostFibonacchiNumber([FromBody] FibonacciMessage message) {
            
            using (var bus = RabbitHutch.CreateBus("host=localhost")) {
                await bus.PubSub.PublishAsync<int>(curNum);
            }
            return curNum;
        }       
        [HttpGet("{num}")]
        public async Task<ActionResult<int>> GetFibonacchiNumber(int num) {
            curNum += num;
            using (var bus = RabbitHutch.CreateBus("host=localhost")) {
                bus.PubSub.PublishAsync<int>(curNum);
            }
            return curNum;
        }
    }
}
