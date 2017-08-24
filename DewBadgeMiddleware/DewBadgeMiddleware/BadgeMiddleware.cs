using DewCore.Abstract.AspNetCore.Middlewares;
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
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">Next middleware</param>
        /// <param name="cs">Connection string</param>
        /// <param name="tp">Table prefix</param>
        public DewBadgeMiddleware(RequestDelegate next, DewBadgeOptions bo, IDewBadgeSigner signReader)
        {
            _next = next;
            _bo = bo;
            _signReader = signReader;
        }
        /// <summary>
        /// Invoke method
        /// </summary>
        /// <param name="context"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public Task Invoke(HttpContext context, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {

            }
            context.Items.Add("DewBadgeOptions", _bo);
            string sign = _signReader.GetSign(context);
            context.Items.Add("DewBadgeSign", sign); 
            return _next(context);
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
        public static IDewBadge GetDewBadge<T>(this HttpContext context) where T : class, IDewBadge, new()
        {
            var data = context.Items.FirstOrDefault(x => x.Key as string == "DewBadgeSign");
            return data.Equals(default(KeyValuePair<object, object>)) ? null : Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data.Value as string);
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
        /// <returns></returns>
        public static bool DewBadgeSignIn<T>(this HttpContext context, DewBadge badge) where T : class, IDewBadgeSigner, new()
        {
            bool result = true;
            var options = context.GetDewBadgeOptions();
            var signer = new T();
            if (options != null)
            {
                if (!signer.SignIn(context, options, badge))
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
        /// <returns></returns>
        public static bool DewBadgeSignOut<T>(this HttpContext context, DewBadge badge) where T : class, IDewBadgeSigner, new()
        {
            bool result = true;
            var options = context.GetDewBadgeOptions();
            var signer = new T();
            if (options != null)
            {
                if (!signer.SignOut(context, options, badge))
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
        /// <param name="bo"></param>
        /// <param name="tp"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseBadgeMiddleware<T>(
           this IApplicationBuilder builder, DewBadgeOptions bo) where T : class, IDewBadgeSigner, new()
        {
            T signer = new T();
            return builder.UseMiddleware<DewBadgeMiddleware>(bo, signer);
        }
    }
}

