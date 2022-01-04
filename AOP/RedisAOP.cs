using Api.Common.Attributes;
using Api.Repository.Interface;
using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Helper;

namespace WebApplication1.AOP
{
    public class RedisAOP : AOPBase
    {
        private IRedisBasketRepository cache;
        public RedisAOP(IRedisBasketRepository cache)
        {
            this.cache = cache;
        }
        public override void Intercept(IInvocation invocation)
        {
            var method = invocation.MethodInvocationTarget ?? invocation.Method;
            CachingAttribute cachingAttribute = null;
            if (method.IsDefined(typeof(CachingAttribute), true))
            {
                cachingAttribute = method.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(CachingAttribute)) as CachingAttribute;
            }
           

            if (cachingAttribute != null)
            {
                var cacheKey = GetCacheKey(invocation);
                var cacheValue = cache.GetValue(cacheKey).Result;
                if (cacheValue != null)
                {
                    var type = invocation.Method.ReturnType;
                    var resultTypes = type.GenericTypeArguments;
                    if (type.FullName == "System.Void")
                    {
                        return;
                    }
                    object response;
                    if (type != null && typeof(Task).IsAssignableFrom(type))
                    {
                        if (resultTypes.Count() > 0)
                        {
                            Type resultType = resultTypes.FirstOrDefault();
                            dynamic temp = Newtonsoft.Json.JsonConvert.DeserializeObject(cacheValue,resultType);
                            response = Task.FromResult(temp);
                        }
                        else
                        {
                            response = Task.Yield();
                        }
                    }
                    else
                    {
                        response = System.Convert.ChangeType(cache.Get<object>(cacheKey),type);
                    }
                    invocation.ReturnValue = response;
                    return;
                }
                //执行当前方法
                invocation.Proceed();

                if (!string.IsNullOrWhiteSpace(cacheKey))
                {
                    object response;

                    //Type type = invocation.ReturnValue?.GetType();
                    var type = invocation.Method.ReturnType;
                    if (type != null && typeof(Task).IsAssignableFrom(type))
                    {
                        var resultProperty = type.GetProperty("Result");
                        response = resultProperty.GetValue(invocation.ReturnValue);
                    }
                    else
                    {
                        response = invocation.ReturnValue;
                    }
                    if (response == null) response = string.Empty;
                    // 核心5：将获取到指定的response 和特性的缓存时间，进行set操作
                    cache.Set(cacheKey, response, TimeSpan.FromMinutes(cachingAttribute.AbsoluteExpiration));
                }
            }
            else
            {
                invocation.Proceed();
            }
        }
    }
}
