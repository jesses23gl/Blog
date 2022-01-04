using Api.Common.Attributes;
using Api.Common.Handler;
using Api.Model;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Api.Common.SwaggerHelper.CustomApiVersion;

namespace WebApplication1.Controllers.V1
{
    /// <summary>
    /// 获取天气
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IAdvertisementServices advertisementServices;

        private readonly ILogger<WeatherForecastController> logger;

        //一个接口两个实现
        //简单工厂模式注入
        private readonly Func<string, IMoreImplService> _serviceAccessor;

        //工厂模式注入
        private readonly IMoreImplService moreImplServiceChinese;
        private readonly IMoreImplService moreImplServiceEnglish;
        //private readonly SingletonFactory singletonFactory;
        
        
        public WeatherForecastController(ILogger<WeatherForecastController> logger,IAdvertisementServices advertisementServices, 
            //SingletonFactory singletonFactory,
            Func<string,IMoreImplService> func )
        {
            this.logger = logger;
            this.advertisementServices = advertisementServices;
            //this.singletonFactory = singletonFactory;
            //this.moreImplServiceChinese = singletonFactory.GetService<IMoreImplService>("Chinese");
            //this.moreImplServiceEnglish = singletonFactory.GetService<IMoreImplService>("English");

            this._serviceAccessor = func;
            moreImplServiceChinese = _serviceAccessor("Chinese");
            moreImplServiceEnglish = _serviceAccessor("English");

        }

        [HttpGet]
        [CustomRoute(ApiVersions.V1,"get")]
        //[Authorize(Roles ="Admin")]
        //[Authorize(Policy = "AdminRequireMent")]//可用中间件配置策略
        public JsonResult Get()
        {
            var ads = advertisementServices.Test();
            var model = new Api.Model.Advertisement { Title = "title", Url = "http://ssss/sfd/fs" };
            advertisementServices.Add(model);
            return new JsonResult(ads);
        } 

        [HttpGet]
        [CustomRoute(ApiVersions.V1, "GetWelcome")]
        public object GetWelcome()
        {
            //return moreImplServiceChinese.SayWelocome();
            return moreImplServiceEnglish.SayWelocome() + "\n" + moreImplServiceChinese.SayWelocome();
        } 


    }
}
