using DewCore.Abstract.AspNetCore.Middlewares;
using Jose;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DewCore.AspNetCore.Middlewares
{
    /// <summary>
    /// Middleware DewMySQLClient class
    /// </summary>
    public class DewBadgeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly DewBadgeOptions _bo;
        private readonly IDewBadgeSigner _signReader;
        private readonly Action<HttpContext> _action;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">Next middleware</param>
        /// <param name="bo">Connection string</param>
        /// <param name="signReader">Table prefix</param>
        public DewBadgeMiddleware(RequestDelegate next, DewBadgeOptions bo, IDewBadgeSigner signReader)
        {
            _next = next;
            _bo = bo;
            _signReader = signReader;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">Next middleware</param>
        /// <param name="bo">options</param>
        /// <param name="signReader">Table prefix</param>
        /// <param name="action">action to execute after setted options</param>
        public DewBadgeMiddleware(RequestDelegate next, DewBadgeOptions bo, IDewBadgeSigner signReader, Action<HttpContext> action)
        {
            _next = next;
            _bo = bo;
            _signReader = signReader;
            _action = action;
        }
        /// <summary>
        /// Invoke method
        /// </summary>
        /// <param name="context"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {

            }
            if (context.Items.ContainsKey("DewBadgeOptions") && _bo.OverWriteMultipleRequest)
                context.Items["DewBadgeOptions"] = _bo;
            else
                context.Items.Add("DewBadgeOptions", _bo);

            string sign = _signReader.GetSign(context);
            if (context.Items.ContainsKey("DewBadgeSign") && _bo.OverWriteMultipleRequest)
                context.Items["DewBadgeSign"] = sign;
            else
                context.Items.Add("DewBadgeSign", sign);
            _action?.Invoke(context);
            await _next(context);
        }
    }
    /// <summary>
    /// HTTPContext Badge Extension class
    /// </summary>
    public static class DewBadgeHttpContextExtension
    {

        /// <summary>
        /// Return the sign from items
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetDewBadgeSign(this HttpContext context)
        {
            var data = context.Items.FirstOrDefault(x => x.Key as string == "DewBadgeSign");
            return data.Equals(default(KeyValuePair<object, object>)) ? null : data.Value as string;
        }

        /// <summary>
        /// Return the badge from items
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="IntegrityException">if signature validation failed</exception>
        /// <exception cref="EncryptionException">if JWT token can't be decrypted</exception>
        /// <exception cref="InvalidAlgorithmException">if JWT signature, encryption or compression algorithm is not supported</exception>
        public static T GetDewBadge<T>(this HttpContext context) where T : class, IDewBadge, new()
        {
            var data = context.Items.FirstOrDefault(x => x.Key as string == "DewBadgeSign");
            var badge = new T();
            var options = context.GetDewBadgeOptions();
            badge.CapitalizeOutput = options.CapitalizeOutput;
            return data.Equals(default(KeyValuePair<object, object>)) ? null : badge.DecodeSign<T>(data.Value as string, options.Secret);
        }

        /// <summary>
        /// Return the sign from items
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DewBadgeOptions GetDewBadgeOptions(this HttpContext context)
        {
            var data = context.Items.FirstOrDefault(x => x.Key as string == "DewBadgeOptions");
            return data.Equals(default(KeyValuePair<object, object>)) ? null : data.Value as DewBadgeOptions;
        }
        /// <summary>
        /// Sign in with Dew Badge
        /// </summary>
        /// <param name="context"></param>
        /// <param name="badge"></param>
        /// <param name="tag">Custom field</param>
        /// <returns></returns>
        public static bool DewBadgeSignIn<T>(this HttpContext context, DewBadge badge, object tag = null) where T : class, IDewBadgeSigner, new()
        {
            bool result = true;
            var options = context.GetDewBadgeOptions();
            var signer = new T();
            if (options != null)
            {
                if (!signer.SignIn(context, options, badge, tag))
                    result = false;
            }
            else
                result = false;
            return result;
        }
        /// <summary>
        /// Sign out with Dew Badge
        /// </summary>
        /// <param name="context"></param>
        /// <param name="badge"></param>
        /// <param name="tag">Custom field</param>
        /// <returns></returns>
        public static bool DewBadgeSignOut<T>(this HttpContext context, DewBadge badge, object tag = null) where T : class, IDewBadgeSigner, new()
        {
            bool result = true;
            var options = context.GetDewBadgeOptions();
            var signer = new T();
            if (options != null)
            {
                if (!signer.SignOut(context, options, badge, tag))
                    result = false;
            }
            else
                result = false;
            return result;
        }
    }
    /// <summary>
    /// Badge pipeline builder extension
    /// </summary>
    public static class DewBadgeBuilderExtension
    {
        /// <summary>
        /// Builder method
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="badgeOptions">Badge options</param>
        /// <returns></returns>
        public static IApplicationBuilder UseBadgeMiddleware<T>(
           this IApplicationBuilder builder, DewBadgeOptions badgeOptions = null) where T : class, IDewBadgeSigner, new()
        {
            T signer = new T();
            badgeOptions = badgeOptions ?? new DewBadgeOptions();
            return builder.UseMiddleware<DewBadgeMiddleware>(badgeOptions, signer);
        }
        /// <summary>
        /// Builder method
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="badgeOptions">Badge options</param>
        /// <param name="action">Custom action to do after setted middleware options</param>
        /// <returns></returns>
        public static IApplicationBuilder UseBadgeMiddleware<T>(
           this IApplicationBuilder builder, DewBadgeOptions badgeOptions = null, Action<HttpContext> action = null) where T : class, IDewBadgeSigner, new()
        {
            T signer = new T();
            badgeOptions = badgeOptions ?? new DewBadgeOptions();
            return builder.UseMiddleware<DewBadgeMiddleware>(badgeOptions, signer, action);
        }
    }
}

