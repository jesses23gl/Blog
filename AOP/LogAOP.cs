using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.AOP
{
    public class LogAOP : IInterceptor
    {
        private readonly ILogger<LogAOP> logger;

        private readonly IHttpContextAccessor accessor;

        public LogAOP(ILogger<LogAOP> logger, IHttpContextAccessor _accessor)
        {
            this.logger = logger;
            accessor = _accessor;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="invocation">包含被AOP拦截的信息</param>
        public void Intercept(IInvocation invocation)
        {
            string userName = accessor.HttpContext?.User?.Identity?.Name;
            var dataIntercept = "" +
                $"【当前操作的用户】:{userName}" +
                $"【当前执行的方法】:{invocation.Method.Name}\r\n" +
                $"【携带的参数有】:{string.Join(",", invocation.Arguments.Select(a => a ?? "".ToString()).ToArray())}";

            try
            {
                MiniProfiler.Current.Step($"当前执行的方法：{invocation.Method.Name}() ->");
                invocation.Proceed();
                dataIntercept += ($"【执行完成结果】:{invocation.ReturnValue}");

                Parallel.For(0, 1, e =>
                {
                    Console.WriteLine(dataIntercept);
                });
            }
            catch (Exception e)
            {
                MiniProfiler.Current.CustomTiming("Errors：", e.Message);
                throw;
            }

        }
    }
}
