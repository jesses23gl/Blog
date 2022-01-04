using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {


            #region 配置Serilog
            //配置serilog输出方式，因为是非托管，要释放
            //using (var log = new LoggerConfiguration().WriteTo.Console().CreateLogger())
            //{
            //    log.Information("serilog");
            //}

            //正确配置
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Debug)//覆盖系统的一些调试信息
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("Serilogs", @"logxxx.log"),rollingInterval:RollingInterval.Infinite)


                .CreateLogger();

            
            Log.Information("I am {0}{1}", "a", "boy");

            var model = new { Name = "lz", Age = 19 };
            Log.Information("data:{model}", model);
            Log.Information("data:{@model}", model);//输出JSON

            Log.CloseAndFlush();//记得要释放资源
            #endregion
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())//使用服务工厂 将Autofac容器添加到Host
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                     .UseSerilog()//在宿主机启动的时候配置serilog,与微软ILogger进行整合
                     .UseStartup<Startup>();

                });
    }
}
