﻿using DewCore.Abstract.AspNetCore.Middlewares;
using Jose;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DewCore.Extensions.Strings;

namespace DewCore.AspNetCore.Middlewares
{
    /// <summary>
    /// Dew badge options
    /// </summary>
    public class DewBadgeOptions
    {
        public string RedirectNotAuthorized { get; set; } = "/errors/notauth";
        public string RedirectOnError { get; set; } = "/errors/error";
        public string Secret { get; set; } = "carriagenostop";

    }

    public class DewBadgeOptionsCookies : DewBadgeOptions
    {
        public string CookieName { get; set; } = "authsign";
        public DateTime CookieExpiring { get; set; } = DateTime.Now.AddMinutes(60);
    }
    public class DewBadgeOptionsJWT : DewBadgeOptions
    {
        public string HeaderName { get; set; } = "Authorization";
        public string Bearer { get; set; } = "bearer ";
    }


    /// <summary>
    /// Badge class
    /// </summary>
    public class DewBadge : IDewBadge
    {
        private readonly string _type = null;
        /// <summary>
        /// Hash algoritm for the sign
        /// </summary>
        public JwsAlgorithm HashSign = JwsAlgorithm.HS512;

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
        /// Check if the badge has one of the passed claims
        /// </summary>
        /// <param name="claims">List of claims separated by commas</param>
        /// <returns></returns>
        public bool HasClaims(string claims)
        {
            bool result = false;
            List<string> toTest = new List<string>();
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
            return result;
        }
        /// <summary>
        /// Return true if object is expired
        /// </summary>
        /// <returns></returns>
        public bool IsExpired()
        {
            return _expired >= DateTime.Now;
        }
        /// <summary>
        /// Return badge sign
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public string GetSign(string secret)
        {
            string payload = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            string token = JWT.Encode(payload, Encoding.ASCII.GetBytes(secret), HashSign);
            return token;
        }
        /// <summary>
        /// Decode a sign and return a badge object
        /// </summary>
        /// <param name="sign"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public DewBadge DecodeSign(string sign, string secret)
        {
            if (sign != null)
            {
                var secretKey = Encoding.ASCII.GetBytes(secret);
                string myToken = JWT.Decode(sign, secretKey, HashSign);
                DewBadge jt = Newtonsoft.Json.JsonConvert.DeserializeObject<DewBadge>(myToken);
                return jt;
            }
            return null;
        }
        /// <summary>
        /// Check if the badge type match
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool AuthType(string type)
        {
            return _type == type;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public DewBadge() { }
        /// <summary>
        /// Constructor with type
        /// </summary>
        /// <param name="type"></param>
        public DewBadge(string type) { }
        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var o = obj as DewBadge;
            if (o == null)
                return false;
            string result = _type;
            foreach (var item in _claims.OrderBy(x => x))
            {
                result = result + item;
            }
            string result1 = o._type;
            foreach (var item in o._claims.OrderBy(x => x))
            {
                result1 = result1 + item;
            }
            return result == result1;
        }
        /// <summary>
        /// Get hash
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            string result = _type;
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
    }
    /// <summary>
    /// Badge claims class
    /// </summary>
    public class DewBadgeClaims : List<string>
    {

    }
    /// <summary>
    /// Badge signer class with cookies
    /// </summary>
    public class DewBadgeSignerCookies : IDewBadgeSigner
    {
        /// <summary>
        /// Get the sign from cookies
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetSign(HttpContext context)
        {
            string result = null;
            var options = context.GetDewBadgeOptions();
            var opt = options as DewBadgeOptionsCookies;
            if (opt != null)
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
        /// <returns></returns>
        public bool SignIn<T>(HttpContext context, T options, DewBadge badge) where T : DewBadgeOptions
        {
            var opt = options as DewBadgeOptionsCookies;
            context.Response.Cookies.Append(opt.CookieName, badge.GetSign(opt.Secret), new CookieOptions() { Expires = opt.CookieExpiring });
            return true;
        }
        /// <summary>
        /// Sign out wit cookies
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="badge"></param>
        /// <returns></returns>
        public bool SignOut<T>(HttpContext context, T options, DewBadge badge) where T : DewBadgeOptions
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
                result = result.Split(' ')[1];
            }
            return result;
        }
        /// <summary>
        /// Sign in with header
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="badge"></param>
        /// <returns></returns>
        public bool SignIn<T>(HttpContext context, T options, DewBadge badge) where T : DewBadgeOptions
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
        /// <returns></returns>
        public bool SignOut<T>(HttpContext context, T options, DewBadge badge) where T : DewBadgeOptions
        {
            return true;
        }
    }
}