using Api.Common.Attributes;
using Api.Common.Extensions;
using Api.Common.Files;
using Api.Model;
using Api.Repository.Interface;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Helper;
using WebApplication1.Logs;
using static Api.Common.SwaggerHelper.CustomApiVersion;

namespace WebApplication1.V1
{
    [Route("api/[controller]")]
    public class BlogsController : ControllerBase
    {
        private readonly ILogger<BlogsController> logger;

        private ILoggerHelper logs;
        //private IRedisBasketRepository redisBasketRepository;
        private IBlogArticleServices blogArticleServices;
        private AppData appData;
        public BlogsController(
            //IRedisBasketRepository _redisBasketRepository, 
            IBlogArticleServices _blogArticleServices,
            ILoggerHelper _logs,
            ILogger<BlogsController> _logger,
            AppData _appData)
        {
            //redisBasketRepository = _redisBasketRepository;
            blogArticleServices = _blogArticleServices;
            logs = _logs;
            appData = _appData;
            logger = _logger;
        }
        [HttpGet]
        [CustomRoute(ApiVersions.V1, "GetBlogs")]
        public async Task<List<BlogArticle>> GetBlogsAsync() 
        {
            //List<BlogArtical> blogArticleList = new List<BlogArtical>();
            Task<List<BlogArticle>> blogArticleList = null;
            //if (redisBasketRepository.Get<object>("blog") != null)
            //{
            //     blogArticleList = redisBasketRepository.Get<List<BlogArticle>>("blog");
            //}
            //else
            //{
            //    blogArticleList = new Task<List<BlogArticle>>(() => new List<BlogArticle> { new BlogArticle { bID = 1 },new BlogArticle { bID = 2} });
            //}
            return await blogArticleList;
        }

        [HttpGet]
        [CustomRoute(ApiVersions.V1, "Get")]
        public async Task<object> Get(int id) 
        {
           
            logger.LogError("serilog");

            //var jwt = appData.Logging.LogLevel.Default;
            //var jwt2 = Api.Common.Files.AppSettings.appSettings(new string[] { });

            var model = await blogArticleServices.GetBlogDetails(id);
            var data = new { success = true, data = model };
            logs.Info("data", "dataMessage");
            return data;
        }
    }
}
