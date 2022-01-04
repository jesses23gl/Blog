using Api.Common.Attributes;
using Api.Common.Files;
using Api.Common.Middlewares;
using Api.Common.Util;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Api.Common.SwaggerHelper.CustomApiVersion;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]/[action]")]

    public class UserController : Controller
    {



        private readonly IUser _user;
        private readonly IAdvertisementServices advertisementServices;
        private readonly ILogger<UserController> logger;

        public UserController(IUser user, IAdvertisementServices advertisementServices,ILogger<UserController>_logger)
        {
            _user = user;
            this.advertisementServices = advertisementServices;
            logger = _logger;
        }
        [HttpGet]
        [Authorize]
        public object GetUserInfoWithAuthorize()
        {

            return _user.Name;
        }

        [HttpGet]
        [AllowAnonymous]
        [CustomRoute(ApiVersions.V1, "GetUserInfoWithoutAuthorize")]
        public object GetUserInfoWithoutAuthorize()
        {
            return _user.Name;
        }

        [HttpGet]
        [AllowAnonymous]
        [CustomRoute(ApiVersions.V1, "TestAOP")]
        [CachingAttribute(AbsoluteExpiration =10)]
        public object TestAOP()
        {
            using (MiniProfiler.Current.Step("开始加载数据："))
            {
                if (true)
                {
                    MiniProfiler.Current.Step("读取服务");
                    var aa = advertisementServices.TestAOP();
                    return aa;
                }
            }
           
        }



    }
}
