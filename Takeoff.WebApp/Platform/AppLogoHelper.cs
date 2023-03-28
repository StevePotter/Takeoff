using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq.Expressions;
using System.Messaging;
using System.Web;
using System.Web.Mvc;
using MvcContrib;
using Takeoff.Data;
using Takeoff.ViewModels;

namespace Takeoff
{

    public static class AppLogoHelper
    {
        public const string AppLogoKey = "AppLogo";
        
        /// <summary>
        /// Sets the app logo in httpcontext, which will then be later translated into a viewmodel and put into ViewData.  This is better 
        /// than setting the value in ViewData because often attributes with special views (like suspended acct) will reset the ViewData, which would clear this out (like the page for guests of a production whose account is suspended).
        /// </summary>
        /// <param name="context"></param>
        /// <param name="account"></param>
        public static void SetAppLogo(this HttpContextBase context, IAccount account)
        {
            var logo = account.Logo;
            if ( logo == null)
                context.Items[AppLogoKey] = String.Empty;//we will want to use a null logo intentionally, to prevent a production whose account has no logo from showing another account's logo
            else
                context.Items[AppLogoKey] = account.Logo;
        }


    }

}