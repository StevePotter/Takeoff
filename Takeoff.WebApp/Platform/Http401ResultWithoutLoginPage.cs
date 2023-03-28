using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Takeoff;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Http401NoLoginPageModule), "Register")]

namespace Takeoff
{
    /// <summary>
    /// Return this when you want to return a 401 without having forms authentication's login page shown.  This bypasses a bug where new HttpStatusCodeResult(401) would always result in a login page.
    /// </summary>
    /// <remarks>
    /// Inspired by http://haacked.com/archive/2011/10/04/prevent-forms-authentication-login-page-redirect-when-you-donrsquot-want.aspx
    /// </remarks>
    public class Http401NoLoginPageResult : ActionResult
    {
        public Http401NoLoginPageResult()
        {
        }

        public Http401NoLoginPageResult(string description)
        {
            Description = description;
        }

        public string Description { get; set; }
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Items["_404NoLogin"] = true;
            if (Description.HasChars())
            {
                context.HttpContext.Items["_404NoLoginDescription"] = Description;                
            }
        }
    }



    public class Http401NoLoginPageModule : IHttpModule
    {
        private static readonly object SuppressAuthenticationKey = new Object();

        public void Init(HttpApplication context)
        {
            context.EndRequest += OnEndRequest;
        }

        private void OnEndRequest(object source, EventArgs args)
        {
            var context = (HttpApplication)source;
            var response = context.Response;

            if (context.Context.Items.Contains("_404NoLogin"))
            {
                response.TrySkipIisCustomErrors = true;
                response.ClearContent();
                response.StatusCode = 401;
                var description = (string) context.Context.Items["_404NoLoginDescription"];
                if ( description.HasChars())
                    response.StatusDescription = description;
                response.RedirectLocation = null;
            }
        }

        public void Dispose()
        {
        }

        public static void Register()
        {
            try
            {
                DynamicModuleUtility.RegisterModule(typeof(Http401NoLoginPageModule));
            }
            catch (Exception)
            {
                //happened during rip thing.  better add this to web.config to fix it
            }
        }
    }
}