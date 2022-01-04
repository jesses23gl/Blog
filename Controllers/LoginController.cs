using Api.Common.Attributes;
using Api.Common.JWTHelper;
using Api.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Api.Common.SwaggerHelper.CustomApiVersion;

namespace WebApplication1.Controllers
{
    [Route("api/login/")]
    public class LoginController : Controller
    {
        //获取Jwt token 登录
        [HttpGet]
        [CustomRoute(ApiVersions.V1, "GetJwtLogin")]
        public async Task<object> GetJwtLogin(string name, string pass) 
        {
            string jwtStr = string.Empty;
            bool _succcessful = false;

            // var userRole = await _sysUserInfoServices.GetUserRoleNameStr(name, pass);
            var userRole = "Admin";
            if (userRole != null)
            {
                JwtTokenModel tokenModel = new JwtTokenModel { Uid = 1, Role = userRole };

                jwtStr = JwtHelper.IssueJwt(tokenModel);
                _succcessful = true;
            }
            else
            {
                jwtStr = "login fail";
            }
            return Ok(new { success = _succcessful, token = jwtStr ,role=userRole});
        }
    }
}
