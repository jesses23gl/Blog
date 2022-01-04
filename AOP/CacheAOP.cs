using Api.Common.Attributes;
using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Helper;

namespace WebApplication1.AOP
{
    public class CacheAOP : AOPBase
    {
        private readonly ICaching cache;
        public CacheAOP(ICaching cache)
        {
            this.cache = cache;
        }
        /// <summary>
        /// 拦截的关键所在
        /// </summary>
        /// <param name="invocation"></param>
        public override void Intercept(IInvocation invocation)
        {
            var method = invocation.MethodInvocationTarget ?? invocation.Method;
            var cachingAttribute = (CachingAttribute)method.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(CachingAttribute));

            if (cachingAttribute != null)
            {
                var cacheKey = GetCacheKey(invocation);
                var cacheValue = cache.Get(cacheKey);
                if (cacheValue != null)
                {
                    invocation.ReturnValue = cacheValue;
                    return;
                }
                //执行当前方法
                invocation.Proceed();

                if (!string.IsNullOrWhiteSpace(cacheKey))
                {
                    cache.Set(cacheKey, invocation.ReturnValue);
                }
            }
            else
            {
                invocation.Proceed();
            }

           
           
        }
    }
}
