using DewCore.Abstract.AspNetCore.Middlewares;
using Jose;
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
            DewBadge badge = null;
            try
            {
                badge = context.HttpContext.GetDewBadge<DewBadge>();
            }
            catch (IntegrityException e)
            {
                badge = new DewBadge() { };
                if (options.DEBUG_MODE)
                {
                    badge.DebugMessage = e.Message;
                }
            }
            catch (EncryptionException e)
            {
                badge = new DewBadge() { };
                if (options.DEBUG_MODE)
                {
                    badge.DebugMessage = e.Message;
                }
            }
            catch (InvalidAlgorithmException e)
            {
                badge = new DewBadge() { };
                if (options.DEBUG_MODE)
                {
                    badge.DebugMessage = e.Message;
                }
            }
            catch (Exception)
            {
                badge = null;
            }
            if (badge == null || sign == null)
            {
                badge.ResponseNoAuth(options, context);
                return;
            }
            if (!badge.IsExpired())
            {
                if (_type != null && _claims == null)
                {
                    if (!badge.AuthType(_type))
                    {
                        badge.ResponseOnForbidden(options, context);
                    }
                }
                else
                {
                    if (_type != null && _claims != null)
                    {
                        if (!badge.AuthType(_type) || !badge.HasClaims(_claims))
                        {
                            badge.ResponseOnForbidden(options, context);
                        }
                    }
                    else
                    {
                        if (_type == null && _claims != null)
                        {
                            if (!badge.HasClaims(_claims))
                            {
                                badge.ResponseOnForbidden(options, context);
                            }
                        }
                    }
                }
            }
            else
            {
                badge.ResponseOnExpired(options, context);
            }
            var temp = options as DewBadgeOptionsCookies;
            if (temp.RefreshExpireOnBrowse)
                new DewBadgeSignerCookies().Refresh<DewBadgeOptionsCookies>(context.HttpContext, temp, badge);
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
            DewBadgeApi badge = null;
            try
            {
                badge = context.HttpContext.GetDewBadge<DewBadgeApi>();
            }
            catch (IntegrityException e)
            {
                badge = new DewBadgeApi() { };
                if (options.DEBUG_MODE)
                {
                    badge.DebugMessage = e.Message;
                }
            }
            catch (EncryptionException e)
            {
                badge = new DewBadgeApi() { };
                if (options.DEBUG_MODE)
                {
                    badge.DebugMessage = e.Message;
                }
            }
            catch (InvalidAlgorithmException e)
            {
                badge = new DewBadgeApi() { };
                if (options.DEBUG_MODE)
                {
                    badge.DebugMessage = e.Message;
                }
            }
            catch (Exception)
            {
                badge = null;
            }
            if (sign == null || badge == null)
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
                        badge.ResponseOnForbidden(options, context);
                    }
                }
                else
                {
                    if (_type != null && _claims != null)
                    {
                        if (!badge.AuthType(_type) || !badge.HasClaims(_claims))
                        {
                            badge.ResponseOnForbidden(options, context);
                        }
                    }
                    else
                    {
                        if (_type == null && _claims != null)
                        {
                            if (!badge.HasClaims(_claims))
                            {
                                badge.ResponseOnForbidden(options, context);
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
