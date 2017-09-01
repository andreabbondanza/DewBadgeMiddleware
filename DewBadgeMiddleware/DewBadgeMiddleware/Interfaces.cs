using DewCore.AspNetCore.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace DewCore.Abstract.AspNetCore.Middlewares
{
    /// <summary>
    /// IDewBadge interface
    /// </summary>
    public interface IDewBadge
    {
        /// <summary>
        /// Return true if the badge contains a claim
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool HasClaims(string key);
        /// <summary>
        /// Return true if badge is expired
        /// </summary>
        /// <returns></returns>
        bool IsExpired();
        /// <summary>
        /// Return the badge sign
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        string GetSign(string secret);
        /// <summary>
        /// Generate a badge from a sign
        /// </summary>
        /// <param name="sign"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        DewBadge DecodeSign(string sign, string secret);
        /// <summary>
        /// Check if the badge type match
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool AuthType(string type);
        /// <summary>
        /// Response for error 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        void ResponseOnError(DewBadgeOptions options, ActionExecutingContext ctx);
        /// <summary>
        /// Response for No authorization
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        void ResponseNoAuth(DewBadgeOptions options, ActionExecutingContext ctx);
        /// <summary>
        /// Response for forbidden
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        void ResponseOnForbidden(DewBadgeOptions options, ActionExecutingContext ctx);
        /// <summary>
        /// Expired response
        /// </summary>
        /// <param name="options"></param>
        /// <param name="ctx"></param>
        void ResponseOnExpired(DewBadgeOptions options, ActionExecutingContext ctx);
    }
    /// <summary>
    /// Badge signer interface
    /// </summary>
    public interface IDewBadgeSigner
    {
        /// <summary>
        /// Return the sign from context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        string GetSign(HttpContext context);
        /// <summary>
        /// Sign in
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="badge"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        bool SignIn<T>(HttpContext context, T options, DewBadge badge, object tag) where T : DewBadgeOptions;
        /// <summary>
        /// Sign out
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="badge"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        bool SignOut<T>(HttpContext context, T options, DewBadge badge, object tag) where T : DewBadgeOptions;
    }
}
