using Api.Common.Util;
using Api.Repository.Interface;
using Api.Repository.Repository;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.Helper
{
    public class RedisBasketRepository :IRedisBasketRepository
    {

        private readonly ILogger<RedisBasketRepository> _logger;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisBasketRepository(ILogger<RedisBasketRepository> logger, ConnectionMultiplexer redis)
        {
            _logger = logger;
            _redis = redis;
            _database = redis.GetDatabase();
        }

        public async Task Clear()
        {
            foreach (var endPoint in _redis.GetEndPoints())
            {
                var server = GetServer();
                foreach (var key in server.Keys())
                {
                    await _database.KeyDeleteAsync(key);
                }
            }
        }

        private IServer GetServer()
        {
            var endPoint = _redis.GetEndPoints();
            return _redis.GetServer(endPoint.FirstOrDefault());
        }

        public async Task<T> Get<T>(string key) where T:class
        {
            var value = await _database.StringGetAsync(key);
            if (value.HasValue)
            {
                return UtilConvert.DeSerialize<T>(value);
            }
            else
            {
                return default(T);
            }
        }

        public async Task<string> GetValue(string key)
        {
            return await _database.StringGetAsync(key);
        }

        public async Task<bool> IsExisted(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public async Task Remove(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

        public async Task  Set(string key, object value, TimeSpan cacheTime)
        {
            if (value !=null)
            {
                await _database.StringSetAsync(key, UtilConvert.Serialize(value), cacheTime);
            }
        }

        public async Task<RedisValue[]> ListRangeAsync(string redisKey) 
        {
            return await _database.ListRangeAsync(redisKey);
        }
        public async Task<long> ListLeftPushAsync(string redisKey, string redisValue, int db = -1) 
        {
            return await _database.ListLeftPushAsync(redisKey, redisValue);
        }
        public async Task<long> ListRightPushAsync(string redisKey,string redisValue,int db = -1) 
        {
            return await _database.ListRightPushAsync(redisKey, redisValue);
        }
        public async Task<long> ListRightPushAsync(string redisKey, IEnumerable<string> redisValue, int db = -1)
        {
            var redislist = new List<RedisValue>();
            foreach (var item in redisValue)
            {
                redislist.Add(item);
            }
            return await _database.ListRightPushAsync(redisKey, redislist.ToArray());
        }
        public async Task<long> ListLeftPushAsync(string redisKey, IEnumerable<string> redisValue, int db = -1)
        {
            var redislist = new List<RedisValue>();
            foreach (var item in redisValue)
            {
                redislist.Add(item);
            }
            return await _database.ListLeftPushAsync(redisKey, redislist.ToArray());
        }
        public async Task<T> ListLeftPopAsync<T>(string redisKey, int db = -1) where T : class 
        {
            return UtilConvert.DeSerialize<T>(await _database.ListLeftPopAsync(redisKey));
        }
        public async Task<T> ListRightPopAsync<T>(string redisKey, int db = -1) where T : class
        {
            return UtilConvert.DeSerialize<T>(await _database.ListRightPopAsync(redisKey));
        }
        public async Task<string> ListLeftPopAsync(string redisKey, int db = -1)
        {
            return await _database.ListLeftPopAsync(redisKey);
        }
        public async Task<string> ListRightPopAsync(string redisKey, int db = -1)
        {
            return await _database.ListRightPopAsync(redisKey);
        }

        /// <summary>
        /// 列表长度
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public async Task<long> ListLengthAsync(string redisKey, int db = -1)
        {
            return await _database.ListLengthAsync(redisKey);
        }

        /// <summary>
        /// 返回在该列表上键所对应的元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> ListRangeAsync(string redisKey, int db = -1)
        {
            var result = await _database.ListRangeAsync(redisKey);
            return result.Select(o => o.ToString());
        }

        /// <summary>
        /// 根据索引获取指定位置数据
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> ListRangeAsync(string redisKey, int start, int stop, int db = -1)
        {
            var result = await _database.ListRangeAsync(redisKey, start, stop);
            return result.Select(o => o.ToString());
        }

        /// <summary>
        /// 删除List中的元素 并返回删除的个数
        /// </summary>
        /// <param name="redisKey">key</param>
        /// <param name="redisValue">元素</param>
        /// <param name="type">大于零 : 从表头开始向表尾搜索，小于零 : 从表尾开始向表头搜索，等于零：移除表中所有与 VALUE 相等的值</param>
        /// <param name="db"></param>
        /// <returns></returns>
        public async Task<long> ListDelRangeAsync(string redisKey, string redisValue, long type = 0, int db = -1)
        {
            return await _database.ListRemoveAsync(redisKey, redisValue, type);
        }

        /// <summary>
        /// 清空List
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="db"></param>
        public async Task ListClearAsync(string redisKey, int db = -1)
        {
            await _database.ListTrimAsync(redisKey, 1, 0);
        }

       
    }
}
