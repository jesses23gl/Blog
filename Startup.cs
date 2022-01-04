using Api.Common.Extensions;
using Api.Common.Files;
using Api.Common.Handler;
using Api.Common.Middlewares;
using Api.Common.Middlewares.MiddlewareHelpers;
using Api.Repository.Interface;
using Api.Repository.Repository;
using Api.Services;
using Api.Services.Service;
using Api.Task;
using Api.Tasks;
using AspNetCoreRateLimit;
using Autofac;
using Autofac.Extras.DynamicProxy;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz.Spi;
using Serilog;
using Services;
using StackExchange.Profiling.Storage;
using StackExchange.Redis;
using System;
using System.IO;
using System.Reflection;
using Api.Task.Quartz;
using WebApplication1.AOP;
using WebApplication1.Filter;
using WebApplication1.Helper;
using WebApplication1.Logs;

namespace WebApplication1
{
    public class Startup
    {
        //Log4Net 
        public static ILoggerRepository repository { get; set; }
       
        public IConfiguration Configuration { get; }
        
        public string ApiName { get; set; } = "WebApplication1";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
     
            #region 配置log4net
            //Startup 构造函数中实例化 ILoggerRepository类型的repository，并读取配置文件
            //然后实现log4 方法===》LogHelper
            //以后使用ILogHelper
            repository = LogManager.CreateRepository("");
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            #endregion
        }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            //log4 自定义日志注入
            services.AddSingleton<ILoggerHelper, LogHelper>();
            #region miniProfiler 性能监控
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(10);
            });
            #endregion

            #region 自定义Filter
            services.AddMvc(o =>
            {
                o.Filters.Add(typeof(GlobalExceptionsFilter));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            #endregion

            #region IConfiguration 配置
            //注入绑定 application中的数据，通过将application选择粘贴Json为类 生成appData
            AppData appData = new AppData();
            Configuration.Bind(appData);
            services.AddSingleton<AppData>();
            //另一种方式是直接注入自定义AppSettings，进行读取信息，能够实现动态
            services.AddSingleton(new AppSettings(Configuration));

            #endregion

            #region Cache 注入
            services.AddSingleton<IMemoryCache>(factory =>
            {
                var cache = new MemoryCache(new MemoryCacheOptions());
                return cache;
            });
            services.AddScoped<ICaching, Caching>();
            #endregion

            #region redis
            services.AddTransient<IRedisBasketRepository, RedisBasketRepository>();
            services.AddRedisCacheSetup();
            //队列
            services.AddRedisInitMqSetup();
            #endregion

            #region AutoMapper
            services.AddAutoMapper(typeof(Startup));
            #endregion
          
            #region swagger
            services.AddSwaggerSetup();
            #endregion

            #region 自定义jwt授权策略
            //授权策略 controller中使用
            //services.AddAuthorization(options =>
            //{
            //    //策略授权：基于角色Role
            //    options.AddPolicy("Client", policy => policy.RequireRole("Client").Build());
            //    options.AddPolicy("Admin", policy => policy.RequireRole("Admin").Build());
            //    options.AddPolicy("SystemOrAdmin", policy => policy.RequireRole(new string[] { "System", "Admin" }).Build());

            //    //基于声明：
            //    options.AddPolicy("AdminClaim", options => options.RequireClaim("Role", "Admin", "User"));

            //    //基于需要：常用
            //    options.AddPolicy("AdminRequireMent", options => options.Requirements.Add(new AdminRequirement() { Name = "zh" }));

            //});
            //声明完策略后 要注入策略控制方法
            //services.AddSingleton<IAuthorizationHandler, MustRoleAdminHandler>();

            #endregion

            #region JWT 认证
            //Jwt 官方认证服务
            services.AddAuthentication_JWTSetup();
            #endregion

            #region 从http中获取用户数据
            services.AddHttpContextSetup();
            #endregion

            #region 原生DOI实现一个接口对应多个实现类
            //SingletonFactory singletonFactory = new SingletonFactory();
            //WelcomeEnglishService welcomeEnglishService = new WelcomeEnglishService();
            //WelcomeChineseService welcomeChineseService = new WelcomeChineseService();
            //singletonFactory.AddService<IMoreImplService>(welcomeChineseService, "Chinese");
            //singletonFactory.AddService<IMoreImplService>(welcomeEnglishService, "English");
            //services.AddSingleton(singletonFactory);

            //简单工厂模式实现一对多依赖注入，可以控制周期，上面不能控制周期
            services.AddSingleton<WelcomeChineseService>();
            services.AddSingleton<WelcomeEnglishService>();
            services.AddScoped(factory =>
            {
                // 所有Func<string, IMoreImplService>委托实例都是这样子 
                Func<string, IMoreImplService> accesor = key =>
                 {
                     if (key.Equals("Chinese"))
                     {
                         return factory.GetService<WelcomeChineseService>();
                     }
                     else if (key.Equals("English"))
                     {
                         return factory.GetService<WelcomeEnglishService>();
                     }
                     else
                     {
                         throw new ArgumentException($"Not support key:{key}");
                     }
                 };
                return accesor;
            });
            #endregion

            #region IP限流 IpPolicyRateLimit
            services.AddIpPolicyRateLimitSetup(Configuration);
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
            #endregion

            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddTransient<Job_Blogs_Quartz>();
            services.AddSingleton<ISchedulerCenter, SchedulerCenterServer>();

        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UserIpLimitMidd();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }

            app.UseStaticFiles();//1
            app.UseCookiePolicy();
            app.UseStatusCodePages();
            app.UseSwaggerMidd(() => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("WebApplication1.index.html"));
            app.UseRouting();//2
            app.UseSerilogRequestLogging();
            app.UseCors();//3
            //自定义Jwt认证授权中间件
            //app.UseJwtTokenAuthentication();

            //先开启认证
            app.UseAuthentication();//4
            //在开启授权中间件
            app.UseAuthorization();
            //开启性能中间件
            app.UseMiniProfiler();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        #region AutoFac
        //Autofac 三步：1、nuget安装 2、容器方法 3、使用服务工厂 将Autofac容器添加到Host
        public void ConfigureContainer(ContainerBuilder builder)
        {
            var basePath = ApplicationEnvironment.ApplicationBasePath;
            //注册某一个接口 和类
            builder.RegisterGeneric(typeof(BaseServices<>)).As(typeof(IBaseServices<>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(BaseRepository<>)).As(typeof(IBaseRepository<>)).InstancePerDependency();//注册仓储
            //注册AOP拦截器
            builder.RegisterType<LogAOP>();
            builder.RegisterType<CacheAOP>();
            builder.RegisterType<RedisAOP>();

            #region 可配置的AOP拦截器
           // var aopType = new List<Type>();
            //if (AppSettings.appSettings(new string[] {"AppSettings","RedisCachingAOP","Enable" }).ObjToBool())
            //{
            //    builder.RegisterType<RedisCacheAOP>();
            //    aopType.Add(typeof(RedisCacheAOP));
            //}
            //if (AppSettings.appSettings(new string[] { "AppSettings","CachingAOP","Enable"}).ObjToBool())
            //{
            //    builder.RegisterType<CacheAOP>();
            //    aopType.Add(typeof(CacheAOP));
            //}
            #endregion



            //注册程序集所有接口和实现类
            var servicesDllFile = Path.Combine(basePath, "Api.Services.dll");  
            var servicesDllFile1 = Path.Combine(basePath, "Api.Repository.dll");

            var assemblyServices = Assembly.LoadFrom(servicesDllFile);//LoadFrom反射文件
            builder.RegisterAssemblyTypes(assemblyServices)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(LogAOP),typeof(CacheAOP)
                //,typeof(RedisAOP)
                ); 

            var assemblyServices1 = Assembly.LoadFrom(servicesDllFile1);
            builder.RegisterAssemblyTypes(assemblyServices1)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(LogAOP),typeof(CacheAOP)
                //,typeof(RedisAOP)
                );


            #region 没有接口层的服务层注入

            //因为没有接口层，所以不能实现解耦，只能用 Load 方法。
            //注意如果使用没有接口的服务，并想对其使用 AOP 拦截，就必须设置为虚方法
            //var assemblysServicesNoInterfaces = Assembly.Load("Blog.Core.Services");
            //builder.RegisterAssemblyTypes(assemblysServicesNoInterfaces);

            #endregion

            #region 没有接口的单独类，启用class代理拦截
            // var cacheType = new List<Type>();

            //只能注入该类中的虚方法，且必须是public
            //这里仅仅是一个单独类无接口测试，不用过多追问
            //builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(Love)))
            //    .EnableClassInterceptors()
            //    .InterceptedBy(cacheType.ToArray());
            #endregion


        }
        #endregion
    }
}
