using Api.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Helper;

namespace WebApplication1.V1
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisController : ControllerBase
    {
        private IRedisBasketRepository redisBasketRepository;
        public RedisController(IRedisBasketRepository _redisBasketRepository)
        {
            redisBasketRepository = _redisBasketRepository;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task RedisMq() 
        {
            //var msg = "一条日志";
            List<string> msg = new List<string>() { "一条日志", "两条日志", "三条日志" };
            await redisBasketRepository.ListLeftPushAsync(RedisMqKey.Loging,msg);
        }
    }
  
}
