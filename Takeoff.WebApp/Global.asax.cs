using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.SessionState;
using Mediascend.Web;
using StackExchange.Profiling;
using Takeoff.Data;
using Takeoff.Models;
using System.Configuration;
using System.Web.Routing;
using System.Web.Mvc;
using System.Security;
using System.IO;
using Takeoff.Platform;
using Takeoff.Resources;
using Takeoff.Controllers;
using Ninject;
using Takeoff.ThingRepositories;

namespace Takeoff
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            this.Context.Items["beginDate"] = DateTime.UtcNow;
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            if (ShouldCreateProfiler(new HttpContextWrapper(Context)))
            {
                MiniProfiler.Start();
            } 

        }

        public static bool ShouldCreateProfiler(HttpContextBase context)
        {
            return (CreateProfilerForLocalRequests && context.Request.IsLocal) || (CreateProfilerForStaff && StaffController.IsCurrUserStaff(context));
        }

        protected void Application_EndRequest(Object sender, EventArgs e)
        {
            MiniProfiler.Stop();
        }

        //protected void Application_Error()
        //{
        //    //http://stackoverflow.com/questions/619895/how-can-i-properly-handle-404-in-asp-net-mvc
        //    //nice solution cuz it avoids redirects.  just not sure how it affects logging.  if you get that straightened out, you're good.
        //    var exception = Server.GetLastError();
        //    exception.IfType<HttpException>(httpException =>
        //                                        {
        //                                            Response.Clear();
        //                                            Server.ClearError();
        //                                            var routeData = new RouteData();
        //                                            routeData.Values["controller"] = "Error";
        //                                            routeData.Values["action"] = "General";
        //                                            Response.StatusCode = httpException.GetHttpCode();
        //                                            switch (Response.StatusCode)
        //                                            {
        //                                                case 403:
        //                                                    routeData.Values["action"] = "Http403";
        //                                                    break;
        //                                                case 404:
        //                                                    routeData.Values["action"] = "Http404";
        //                                                    break;
        //                                                default:
        //                                                    routeData.Values["exception"] = exception;
        //                                            }

        //                                            IController errorsController = new ErrorController();
        //                                            var rc = new RequestContext(new HttpContextWrapper(Context),
        //                                                                        routeData);                                                   
        //                                            errorsController.Execute(rc);
        //                                        });
        //}

        const string MiniProfilerRunConditionsSetting = "MiniProfilerRunConditions";
        private static bool CreateProfilerForLocalRequests = ConfigurationManager.AppSettings[MiniProfilerRunConditionsSetting].CharsOrEmpty().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Contains("Local", StringComparer.OrdinalIgnoreCase);
        private static bool CreateProfilerForStaff = ConfigurationManager.AppSettings[MiniProfilerRunConditionsSetting].CharsOrEmpty().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Contains("Staff", StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Sssshhhhhh!
        /// </summary>
        public static bool StealthUI = ConfigurationManager.AppSettings["StealthUI"].CharsOrEmpty().Trim().EqualsCaseInsensitive("true");
    }


}