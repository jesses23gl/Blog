using Api.Common.Util;
using Api.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Serilog;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Logs;

namespace WebApplication1.Filter
{
    /// <summary>
    /// 全局异常错误日志
    /// </summary>
    public class GlobalExceptionsFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment env;
        private readonly ILoggerHelper loggerHelper;
        private readonly ILogger<GlobalExceptionsFilter> logger;
        public GlobalExceptionsFilter(IWebHostEnvironment _env, ILoggerHelper _loggerHelper, ILogger<GlobalExceptionsFilter> _logger)
        {
            env = _env;
            loggerHelper = _loggerHelper;
            logger = _logger;
        }
        public void OnException(ExceptionContext context)
        {
            var json = new MessageModel<string>();
            json.msg = context.Exception.Message;
            json.status = 500;
            var errorAudit = "Unable to resolve service for";
            if (!string.IsNullOrEmpty(json.msg) && json.msg.Contains(errorAudit))
            {
                json.msg = json.msg.Replace(errorAudit, $"(若新添加服务，需要重新编译){errorAudit}");

            }
            if (env.EnvironmentName.ObjToString().Equals("Development"))
            {
                json.msgDev = context.Exception.StackTrace;
            }
            var res = new ContentResult();
            res.Content = JsonHelper.GetJSON<MessageModel<string>>(json);
            context.Result = res;

            MiniProfiler.Current.CustomTiming("Errors：", json.msg);

            logger.LogError(json.msg, json.msg + WriteLog(json.msg, context.Exception));
            loggerHelper.Error(json.msg,json.msg + WriteLog(json.msg, context.Exception));
        }

        public string WriteLog(string throwMsg, Exception ex)
        {
            return string.Format("\r\n【自定义错误】:{0} \r\n 【异常类型】:{1} \r\n【异常信息】:{2} \r\n【调用堆栈】:{3}", new object[] { throwMsg, ex.GetType().Name, ex.Message, ex.StackTrace });
        }
    }
}
