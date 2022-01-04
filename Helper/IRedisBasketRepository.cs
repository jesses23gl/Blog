using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.Helper
{
    public interface IRedisBasketRepository
    {
        Task<string> GetValue(string key);
        Task<T> Get<T>(string key) where T:class;
        System.Threading.Tasks.Task Set(string key, object value, TimeSpan cacheTime);
        Task<bool> IsExisted(string key);
        System.Threading.Tasks.Task Remove(string key);
        System.Threading.Tasks.Task Clear();

        Task<RedisValue[]> ListRangeAsync(string redisKey);
        Task<long> ListLeftPushAsync(string redisKey, string redisValue, int db = -1);
        Task<long> ListLeftPushAsync(string redisKey, IEnumerable<string> redisValue, int db = -1);
        Task<long> ListRightPushAsync(string redisKey, string redisValue, int db = -1);
        Task<long> ListRightPushAsync(string redisKey, IEnumerable<string> redisValue, int db = -1);
        Task<T> ListLeftPopAsync<T>(string redisKey, int db = -1) where T : class;
        Task<T> ListRightPopAsync<T>(string redisKey, int db = -1) where T : class;
        Task<string> ListLeftPopAsync(string redisKey, int db = -1);
        Task<string> ListRightPopAsync(string redisKey, int db = -1);
        Task<long> ListLengthAsync(string redisKey, int db = -1);
        Task<IEnumerable<string>> ListRangeAsync(string redisKey, int db = -1);
        Task<IEnumerable<string>> ListRangeAsync(string redisKey, int start, int stop, int db = -1);
        Task<long> ListDelRangeAsync(string redisKey, string redisValue, long type = 0, int db = -1);
        System.Threading.Tasks.Task ListClearAsync(string redisKey, int db = -1);
    }
}
