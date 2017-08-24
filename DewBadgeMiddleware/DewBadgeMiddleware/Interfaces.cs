using DewCore.AspNetCore.Middlewares;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace DewCore.Abstract.AspNetCore.Middlewares
{
    public interface IDewBadge
    {
        bool HasClaims(string key);

        bool IsExpired();

        string GetSign(string secret);

        IDewBadge DecodeSign(string sign, string secret);
        bool AuthType(string type);
    }
    /// <summary>
    /// Tag interface for options
    /// </summary>
    public interface IDewBadgeSigner
    {
        string GetSign(HttpContext context);

        bool SignIn<T>(HttpContext context, T options, IDewBadge badge) where T : DewBadgeOptions;
        bool SignOut<T>(HttpContext context, T options, IDewBadge badge) where T : DewBadgeOptions;
    }
}
