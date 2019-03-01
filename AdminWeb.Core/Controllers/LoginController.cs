﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AdminWeb.Core.AuthHelper;
using AdminWeb.Core.BasicData;
using AdminWeb.Core.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdminWeb.Core.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Produces("application/json")]
    [Route("api/Login")]
    public class LoginController : Controller
    {

        IsysUserInfoServices sysUserInfoServices;
        IUserRoleServices userRoleServices;
        IRoleServices roleServices;
        PermissionRequirement _requirement;


        /// <summary>
        /// 构造函数注入
        /// </summary>
        /// <param name="sysUserInfoServices"></param>
        /// <param name="userRoleServices"></param>
        /// <param name="roleServices"></param>
        /// <param name="requirement"></param>
        public LoginController(IsysUserInfoServices sysUserInfoServices, IUserRoleServices userRoleServices, IRoleServices roleServices, PermissionRequirement requirement)
        {
            this.sysUserInfoServices = sysUserInfoServices;
            this.userRoleServices = userRoleServices;
            this.roleServices = roleServices;
            _requirement = requirement;
        }


        #region 获取token的第1种方法
        /// <summary>
        /// 获取JWT的方法
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="sub">角色</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Token")]
        public async Task<object> GetJWTStr(string name, string pass)
        {
            string jwtStr = string.Empty;
            bool suc = false;
            //这里就是用户登陆以后，通过数据库去调取数据，分配权限的操作
            //这里直接写死了

            var user = await sysUserInfoServices.GetUserRoleNameStr(name, pass);
            if (user != null)
            {

                TokenModelJWT tokenModel = new TokenModelJWT();
                tokenModel.Uid = 1;
                tokenModel.Role = user;

                jwtStr = JwtHelper.IssueJWT(tokenModel);
                suc = true;
            }
            else
            {
                jwtStr = "login fail!!!";
            }

            return Ok(new
            {
                success = suc,
                token = jwtStr
            });
        }



        [HttpGet]
        [Route("GetTokenNuxt")]
        public async Task<object> GetJWTStrForNuxt(string name, string pass)
        {
            string jwtStr = string.Empty;
            bool suc = false;
            //这里就是用户登陆以后，通过数据库去调取数据，分配权限的操作
            //这里直接写死了
            if (name == "admins" && pass == "admins")
            {
                TokenModelJWT tokenModel = new TokenModelJWT();
                tokenModel.Uid = 1;
                tokenModel.Role = "Admin";

                jwtStr = JwtHelper.IssueJWT(tokenModel);
                suc = true;
            }
            else
            {
                jwtStr = "login fail!!!";
            }
            var result = new
            {
                data = new { success = suc, token = jwtStr }
            };

            return Ok(new
            {
                success = suc,
                data = new { success = suc, token = jwtStr }
            });
        }
        #endregion


        /// <summary>
        /// 获取JWT的方法 3.0
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetJWTToken3")]
        public async Task<object> GetJWTToken3([FromBody] User user)
        {
            string jwtStr = string.Empty;
            var userInfo = await sysUserInfoServices.GetUserRoleNameStr(user.name, user.pass);
            var userData = await sysUserInfoServices.CheckUserInfo(user.name, user.pass);

            if (!string.IsNullOrEmpty(userInfo))
            {
                //如果是基于用户的授权策略，这里要添加用户;如果是基于角色的授权策略，这里要添加角色
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, user.name),
                    new Claim(ClaimTypes.Expiration, DateTime.Now.AddSeconds(_requirement.Expiration.TotalSeconds).ToString()) };
                claims.AddRange(userInfo.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));

                //用户标识
                var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                identity.AddClaims(claims);

                var token = JwtToken.BuildJwtToken(claims.ToArray(), _requirement);

                BasicDataUser.UserName = userData.uLoginName ;
                BasicDataUser.UserId = userData.uID;

                return new JsonResult(new { status = true, data =token,uid= userData.uID });
            }
            else
            {
                return new JsonResult(new
                {
                    status = false,
                    message = "认证失败"
                });
            }
        }
        public class User
        {
            public string name{ get; set; }
            public string pass { get; set; }
        }


        /// <summary>
        /// 发送jsonp格式
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sub"></param>
        /// <param name="expiresSliding"></param>
        /// <param name="expiresAbsoulute"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("jsonp")]
        public void Getjsonp(string callBack, long id = 1, string sub = "Admin", int expiresSliding = 30, int expiresAbsoulute = 30)
        {
            TokenModelJWT tokenModel = new TokenModelJWT();
            tokenModel.Uid = id;
            tokenModel.Role = sub;

            string jwtStr = JwtHelper.IssueJWT(tokenModel);

            string response = string.Format("\"value\":\"{0}\"", jwtStr);
            string call = callBack + "({" + response + "})";
            Response.WriteAsync(call);
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("LoginOut")]
        public IActionResult LoginOut(int uid)
        {
            return Ok(new { state = true });
        }
    }
}