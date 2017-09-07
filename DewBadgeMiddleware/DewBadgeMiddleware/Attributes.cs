using DewCore.Abstract.AspNetCore.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace DewCore.AspNetCore.Middlewares
{
    /// <summary>
    /// Dew Badge attribute class
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class DewBadgeAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Attribute on executing override
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            var sign = context.HttpContext.GetDewBadgeSign();
            var options = context.HttpContext.GetDewBadgeOptions();
            var badge = context.HttpContext.GetDewBadge<DewBadge>();
            if (badge == null)
            {
                badge = new DewBadge() { };
                badge.ResponseNoAuth(options, context);
                return;
            }
            if (!badge.IsExpired())
            {
                if (_type != null && _claims == null)
                {
                    if (!badge.AuthType(_type))
                    {
                        badge.ResponseNoAuth(options, context);
                    }
                }
                else
                {
                    if (_type != null && _claims != null)
                    {
                        if (!badge.AuthType(_type) || !badge.HasClaims(_claims))
                        {
                            badge.ResponseNoAuth(options, context);
                        }
                    }
                    else
                    {
                        if (_type == null && _claims != null)
                        {
                            if (!badge.HasClaims(_claims))
                            {
                                badge.ResponseNoAuth(options, context);
                            }
                        }
                    }
                }
            }
            else
            {
                badge.ResponseOnExpired(options, context);
            }
        }
        readonly string _type = null;
        readonly string _claims = null;
        /// <summary>
        /// Attribute for Badge
        /// </summary>
        /// <param name="claims">List of requested claims (separated by comma)</param>
        public DewBadgeAttribute(string claims)
        {
            this._claims = claims;
        }

        /// <summary>
        /// Attribute for Badge
        /// </summary>
        /// <param name="claims">List of requested claims (separated by comma)</param>
        /// <param name="types">Badge types (separated by comma)</param>
        public DewBadgeAttribute(string claims, string types)
        {
            _claims = claims;
            _type = types;
        }
        /// <summary>
        /// Attribute for Badge
        /// </summary>
        /// <param name="types">Badge types (separated by comma)</param>
        /// <param name="tag">Tag argoument, ignore it</param>
        public DewBadgeAttribute(string types, bool tag = true)
        {
            _type = types;
        }
        /// <summary>
        /// List of claims
        /// </summary>
        public string Claims
        {
            get { return _claims; }
        }
    }
    /// <summary>
    /// Dew Badge attribute class
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class DewBadgeApiAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Attribute on executing override
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            var sign = context.HttpContext.GetDewBadgeSign();
            var options = context.HttpContext.GetDewBadgeOptions();
            var badge = context.HttpContext.GetDewBadge<DewBadgeApi>();
            if (sign == null)
            {
                badge = new DewBadgeApi() { };
                badge.ResponseNoAuth(options, context);
                return;
            }
            if (!badge.IsExpired())
            {
                if (_type != null && _claims == null)
                {
                    if (!badge.AuthType(_type))
                    {
                        badge.ResponseNoAuth(options, context);
                    }
                }
                else
                {
                    if (_type != null && _claims != null)
                    {
                        if (!badge.AuthType(_type) || !badge.HasClaims(_claims))
                        {
                            badge.ResponseNoAuth(options, context);
                        }
                    }
                    else
                    {
                        if (_type == null && _claims != null)
                        {
                            if (!badge.HasClaims(_claims))
                            {
                                badge.ResponseNoAuth(options, context);
                            }
                        }
                    }
                }
            }
            else
            {
                badge.ResponseOnExpired(options, context);
            }
        }
        readonly string _type = null;
        readonly string _claims = null;
        /// <summary>
        /// Attribute for Badge
        /// </summary>
        /// <param name="claims">List of requested claims (separated by comma)</param>
        public DewBadgeApiAttribute(string claims)
        {
            this._claims = claims;
        }

        /// <summary>
        /// Attribute for Badge
        /// </summary>
        /// <param name="claims">List of requested claims (separated by comma)</param>
        /// <param name="types">Badge types (separated by comma)</param>
        public DewBadgeApiAttribute(string claims, string types)
        {
            _claims = claims;
            _type = types;
        }
        /// <summary>
        /// Attribute for Badge
        /// </summary>
        /// <param name="types">Badge types (separated by comma)</param>
        /// <param name="tag">Tag argoument, ignore it</param>
        public DewBadgeApiAttribute(string types, bool tag = true)
        {
            _type = types;
        }
        /// <summary>
        /// List of claims
        /// </summary>
        public string Claims
        {
            get { return _claims; }
        }
    }
}
