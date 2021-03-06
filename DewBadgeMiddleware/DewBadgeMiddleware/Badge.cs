﻿using DewCore.Abstract.AspNetCore.Middlewares;
using Jose;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DewCore.Extensions.Strings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using DewCore.Types.AspNetCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DewCore.AspNetCore.Middlewares
{
    /// <summary>
    /// Dew badge options
    /// </summary>
    public class DewBadgeOptions
    {
        /// <summary>
        /// Set debug mode
        /// </summary>
        public bool DEBUG_MODE = false;
        /// <summary>
        /// Secret key for hash
        /// </summary>
        public string Secret { get; set; } = "carriagenostop";
        /// <summary>
        /// Overwrite multiple requests
        /// </summary>
        public bool OverWriteMultipleRequest { get; set; } = false;
        /// <summary>
        /// Indicate if output will be capitalized ( { name: "El Carro" } or { Name: "El Carro" })
        /// </summary>
        public bool CapitalizeOutput { get; set; } = true;
    }
    /// <summary>
    /// Option class for cookies
    /// </summary>
    public class DewBadgeOptionsCookies : DewBadgeOptions
    {
        /// <summary>
        /// Enable redirect
        /// </summary>
        public bool EnableRedirect { get; set; } = true;
        /// <summary>
        /// Redirect path for not authorized requests
        /// </summary>
        public string RedirectNotAuthorized { get; set; } = "/errors/notauth";
        /// <summary>
        /// Redirect path for error on requests
        /// </summary>
        public string RedirectOnBadRequest { get; set; } = "/errors/error";
        /// <summary>
        /// Redirect path for forbidden
        /// </summary>
        public string RedirectForbidden { get; set; } = "/errors/forbidden";
        /// <summary>
        /// Cookie name
        /// </summary>
        public string CookieName { get; set; } = "authsign";
        /// <summary>
        /// Cookie expire time in minutes from creation
        /// </summary>
        public int CookieExpiring { get; set; } = 60;
        /// <summary>
        /// Cookie remember time in minutes from creation
        /// </summary>
        public int CookieRemember { get; set; } = 14400;
        /// <summary>
        /// Refresh every time user browse the site
        /// </summary>
        public bool RefreshExpireOnBrowse { get; set; } = true;
    }
    /// <summary>
    /// Option class for jwt
    /// </summary>
    public class DewBadgeOptionsJWT : DewBadgeOptions
    {
        /// <summary>
        /// Request header name
        /// </summary>
        public string HeaderName { get; set; } = "Authorization";
        /// <summary>
        /// Bearer string
        /// </summary>
        public string Bearer { get; set; } = "bearer ";
    }


    /// <summary>
    /// Badge class
    /// </summary>
    public class DewBadge : IDewBadge
    {
        /// <summary>
        /// Indicate if output will be capitalized ( { name: "El Carro" } or { Name: "El Carro" })
        /// </summary>
        [JsonIgnore]
        public bool CapitalizeOutput { get; set; } = true;
        private DewBadgeTypes _types;
        /// <summary>
        /// Badge types
        /// </summary>
        public DewBadgeTypes Types
        {
            get { return _types; }
            set { _types = value; }
        }
        /// <summary>
        /// Hash algoritm for the sign
        /// </summary>
        [JsonIgnore]
        public JwsAlgorithm HashSign = JwsAlgorithm.HS256;
        /// <summary>
        /// Debug message for errors with debug mode ON
        /// </summary>
        public string DebugMessage = null;

        private DewBadgeClaims _claims;
        /// <summary>
        /// List of the badge claims
        /// </summary>
        public DewBadgeClaims Claims
        {
            get { return _claims; }
            set { _claims = value; }
        }

        private long _idUser = 0;
        /// <summary>
        /// User ID
        /// </summary>
        public long IdUser
        {
            get { return _idUser; }
            set { _idUser = value; }
        }

        private DateTime _created = DateTime.Now;
        /// <summary>
        /// Created date
        /// </summary>
        public DateTime Created
        {
            get { return _created; }
            set { _created = value; }
        }

        private DateTime _expired;
        /// <summary>
        /// Expired date
        /// </summary>
        public DateTime Expired
        {
            get { return _expired; }
            set { _expired = value; }
        }

        private DateTime _updated = DateTime.Now;
        /// <summary>
        /// Expired date
        /// </summary>
        public DateTime Updated
        {
            get { return _updated; }
            set { _updated = value; }
        }
        /// <summary>
        /// Check if the badge has at least one of the passed claims
        /// </summary>
        /// <param name="claims">List of claims separated by commas</param>
        /// <returns></returns>
        public bool HasClaims(string claims)
        {
            bool result = false;
            if (claims != null)
            {
                if (claims.Contains(","))
                {
                    foreach (var item in claims.Split(','))
                    {
                        if (Claims.FirstOrDefault(x => x == item) != default(string))
                        {
                            result = true;
                            break;
                        }
                    }
                }
                else
                    result = Claims.FirstOrDefault(x => x == claims) != default(string);
            }
            return result;
        }
        /// <summary>
        /// Return true if object is expired
        /// </summary>
        /// <returns></returns>
        public virtual bool IsExpired()
        {
            return _expired <= DateTime.Now;
        }
        /// <summary>
        /// Return badge sign
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public virtual string GetSign(string secret)
        {
            string payload = Newtonsoft.Json.JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings()
            {
                ContractResolver = CapitalizeOutput ? new DefaultContractResolver() : new CamelCasePropertyNamesContractResolver()
            });
            string token = JWT.Encode(payload, Encoding.ASCII.GetBytes(secret), HashSign);
            return token;
        }
        /// <summary>
        /// Decode a sign and return a badge object
        /// </summary>
        /// <param name="sign"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public virtual T DecodeSign<T>(string sign, string secret) where T : class, IDewBadge, new()
        {
            if (sign != null)
            {
                var secretKey = Encoding.ASCII.GetBytes(secret);
                string myToken = JWT.Decode(sign, secretKey, HashSign);
                T jt = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(myToken);
                return jt;
            }
            return null;
        }
        /// <summary>
        /// Check if the badge type match
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public virtual bool AuthType(string types)
        {
            bool result = false;
            if (types != null)
            {
                if (types.Contains(","))
                {
                    foreach (var item in types.Split(','))
                    {
                        if (Types.FirstOrDefault(x => x == item) != default(string))
                        {
                            result = true;
                            break;
                        }
                    }
                }
                else
                    result = Types.FirstOrDefault(x => x == types) != default(string);
            }
            return result;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public DewBadge() { }
        /// <summary>
        /// Constructor with type
        /// </summary>
        /// <param name="types">Types (separated by comma), es: type or type1,type2,type3</param>
        public DewBadge(DewBadgeTypes types)
        {
            Types = types;
        }
        /// <summary>
        /// Constructor with claims
        /// </summary>
        /// <param name="claims">Types (separated by comma), es: type or claim1,claim2,claim3</param>
        public DewBadge(DewBadgeClaims claims)
        {
            Claims = claims;
        }
        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is DewBadge o))
                return false;
            string result = this.IdUser + "";
            foreach (var item in _types.OrderBy(x => x))
            {
                result += item;
            }
            foreach (var item in _claims.OrderBy(x => x))
            {
                result += item;
            }
            string result1 = o.IdUser + "";
            foreach (var item in o._types.OrderBy(x => x))
            {
                result += item;
            }
            foreach (var item in o._claims.OrderBy(x => x))
            {
                result += item;
            }
            return result == result1;
        }
        /// <summary>
        /// Get hash
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            string result = IdUser + "";
            foreach (var item in _types.OrderBy(x => x))
            {
                result = result + item;
            }
            foreach (var item in _claims.OrderBy(x => x))
            {
                result = result + item;
            }
            var bytes = result.ToBytes();
            int hash = 0;
            foreach (var item in bytes)
            {
                hash += item % 7;
            }
            return hash;
        }
        /// <summary>
        /// Return redirect on error
        /// </summary>
        /// <param name="options"></param>
        /// /// <param name="ctx">httpcontext</param>
        /// <returns></returns>
        public virtual void ResponseNoAuth(DewBadgeOptions options, ActionExecutingContext ctx)
        {
            var opt = options as DewBadgeOptionsCookies;
            if (opt.EnableRedirect)
            {
                ctx.Result = new RedirectResult(opt.RedirectNotAuthorized + "?fallbackurl=" + WebUtility.UrlEncode(ctx.HttpContext.Request.Path + ctx.HttpContext.Request.QueryString));
            }
            else
                ctx.Result = new UnauthorizedResult();

        }
        /// <summary>
        /// Return redirect no auth
        /// </summary>
        /// <param name="options"></param>
        /// <param name="ctx">httpcontext</param>
        /// <returns></returns>
        public virtual void ResponseOnError(DewBadgeOptions options, ActionExecutingContext ctx)
        {
            var opt = options as DewBadgeOptionsCookies;
            if (opt.EnableRedirect)
                ctx.Result = new RedirectResult(opt.RedirectOnBadRequest + "?fallbackurl=" + WebUtility.UrlEncode(ctx.HttpContext.Request.Path + ctx.HttpContext.Request.QueryString));
            else
                ctx.Result = new BadRequestResult();
        }
        /// <summary>
        /// Return redirect no auth
        /// </summary>
        /// <param name="options"></param>
        /// <param name="ctx">httpcontext</param>
        /// <returns></returns>
        public virtual void ResponseOnForbidden(DewBadgeOptions options, ActionExecutingContext ctx)
        {
            var opt = options as DewBadgeOptionsCookies;
            if (opt.EnableRedirect)
                ctx.Result = new RedirectResult(opt.RedirectForbidden + "?fallbackurl=" + WebUtility.UrlEncode(ctx.HttpContext.Request.Path + ctx.HttpContext.Request.QueryString));
            else
                ctx.Result = new ForbidResult();
        }
        /// <summary>
        /// Return expired no auth
        /// </summary>
        /// <param name="options"></param>
        /// <param name="ctx"></param>
        public virtual void ResponseOnExpired(DewBadgeOptions options, ActionExecutingContext ctx)
        {
            var opt = options as DewBadgeOptionsCookies;
            if (opt.EnableRedirect)
                ctx.Result = new RedirectResult(opt.RedirectForbidden + "?fallbackurl=" + WebUtility.UrlEncode(ctx.HttpContext.Request.Path + ctx.HttpContext.Request.QueryString));
            else
                ctx.Result = new UnauthorizedResult();
        }
    }
    /// <summary>
    /// Badge for api
    /// </summary>
    public class DewBadgeApi : DewBadge
    {
        /// Decode a sign and return a badge object
        /// <summary></summary>
        /// <param name="sign"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public override T DecodeSign<T>(string sign, string secret)
        {
            if (sign != null)
            {
                var secretKey = Encoding.ASCII.GetBytes(secret);
                string myToken = JWT.Decode(sign, secretKey, HashSign);
                T jt = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(myToken);
                return jt;
            }
            return null;
        }
        /// <summary>
        /// Return result on error
        /// </summary>
        /// <param name="options"></param>
        /// /// <param name="ctx">httpcontext</param>
        /// <returns></returns>
        public override void ResponseNoAuth(DewBadgeOptions options, ActionExecutingContext ctx)
        {
            var resp = new StandardResponse() { Error = new StandardResponseError("Not Authorized for the resource", 1) };
            ctx.Result = new JsonResult(resp) { StatusCode = 401 };
        }
        /// <summary>
        /// Return result no auth
        /// </summary>
        /// <param name="options"></param>
        /// <param name="ctx">httpcontext</param>
        /// <returns></returns>
        public override void ResponseOnError(DewBadgeOptions options, ActionExecutingContext ctx)
        {
            var resp = new StandardResponse() { Error = new StandardResponseError("Error on the resource", 2) };
            ctx.Result = new JsonResult(resp) { StatusCode = 400 };
        }
        /// <summary>
        /// Return result forbidden
        /// </summary>
        /// <param name="options"></param>
        /// <param name="ctx">httpcontext</param>
        /// <returns></returns>
        public override void ResponseOnForbidden(DewBadgeOptions options, ActionExecutingContext ctx)
        {
            var resp = new StandardResponse() { Error = new StandardResponseError("Forbidden resource", 3) };
            ctx.Result = new JsonResult(resp) { StatusCode = 403 };
        }
        /// <summary>
        /// Return expired no auth
        /// </summary>
        /// <param name="options"></param>
        /// <param name="ctx"></param>
        public override void ResponseOnExpired(DewBadgeOptions options, ActionExecutingContext ctx)
        {
            var resp = new StandardResponse() { Error = new StandardResponseError("Badge expired, not authorized", 4) };
            ctx.Result = new JsonResult(new { Text = "Badge expired, not authorized", Error = "00004" }) { StatusCode = 401 };
        }
    }

    /// <summary>
    /// Badge claims class
    /// </summary>
    public class DewBadgeClaims : List<string>
    {

    }
    /// <summary>
    /// Badge claims types
    /// </summary>
    public class DewBadgeTypes : List<string>
    {

    }
    /// <summary>
    /// Badge signer class with cookies
    /// </summary>
    public class DewBadgeSignerCookies : IDewBadgeSigner
    {
        /// <summary>
        /// Refresh cookie
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <param name="context"></param>
        /// <param name="badge"></param>
        /// <param name="tag"></param>
        public void Refresh<T>(HttpContext context, T options, DewBadge badge, object tag = null) where T : DewBadgeOptionsCookies
        {
            if (options is DewBadgeOptionsCookies opt)
            {
                var header = context.Request.Cookies.FirstOrDefault(x => { return x.Key == opt.CookieName; });
                if (!header.Equals(default(KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>)))
                {
                    context.Response.Cookies.Append(opt.CookieName, badge.GetSign(opt.Secret), new CookieOptions() { Expires = DateTime.Now.AddMinutes(opt.CookieExpiring) });
                }
            }
        }

        /// <summary>
        /// Get the sign from cookies
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetSign(HttpContext context)
        {
            string result = null;
            var options = context.GetDewBadgeOptions();
            if (options is DewBadgeOptionsCookies opt)
            {
                var header = context.Request.Cookies.FirstOrDefault(x => { return x.Key == opt.CookieName; });
                if (!header.Equals(default(KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>)))
                    result = header.Value;
            }
            return result;
        }
        /// <summary>
        /// Sign in with cookies
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="badge"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool SignIn<T>(HttpContext context, T options, DewBadge badge, object tag) where T : DewBadgeOptions
        {
            var opt = options as DewBadgeOptionsCookies;
            var remember = false;
            if (tag != null)
                remember = (bool)tag;
            context.Response.Cookies.Append(opt.CookieName, badge.GetSign(opt.Secret), new CookieOptions() { Expires = remember ? DateTime.Now.AddMinutes(opt.CookieRemember) : DateTime.Now.AddMinutes(opt.CookieExpiring) });
            return true;
        }
        /// <summary>
        /// Sign out wit cookies
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="badge"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool SignOut<T>(HttpContext context, T options, DewBadge badge, object tag) where T : DewBadgeOptions
        {
            var opt = options as DewBadgeOptionsCookies;
            context.Response.Cookies.Delete(opt.CookieName);
            return true;
        }
    }
    /// <summary>
    /// Dew badge signer with jwt (ex. api restuful service)
    /// </summary>
    public class DewBadgeSignerJWT : IDewBadgeSigner
    {
        /// <summary>
        /// Return the sign from header
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetSign(HttpContext context)
        {
            string result = null;
            var options = context.GetDewBadgeOptions();
            var opt = options as DewBadgeOptionsJWT;
            var header = context.Request.Headers.FirstOrDefault(x => { return x.Key == opt.HeaderName; });
            if (!header.Equals(default(KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>)))
                result = header.Value;
            if (result != null)
            {
                if (result.Split(' ').Count() > 1)
                    result = result.Split(' ')[1];
                else
                    result = result.Split(' ')[0];

            }
            return result;
        }
        /// <summary>
        /// Sign in with header
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="badge"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool SignIn<T>(HttpContext context, T options, DewBadge badge, object tag) where T : DewBadgeOptions
        {
            var opt = options as DewBadgeOptionsJWT;
            context.Response.Headers.Add(opt.HeaderName, opt.Bearer + badge.GetSign(options.Secret));
            return true;
        }
        /// <summary>
        /// Sign out with header
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="badge"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool SignOut<T>(HttpContext context, T options, DewBadge badge, object tag) where T : DewBadgeOptions
        {
            return true;
        }
    }
}