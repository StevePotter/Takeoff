using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Takeoff.Controllers;
using System.Net.Mail;
using System.Configuration;
using System.Web.Script.Serialization;
using System.IO;
using System.Web.Routing;
using System.Reflection;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace Takeoff
{


    /// <summary>
    /// Makes it possible to add redirects from ajax calls.  This doesn't work outside of the box.  It also requires a bit of javascript but it's cool.  
    /// </summary>
    /// <remarks>http://craftycodeblog.com/2010/05/15/asp-net-mvc-ajax-redirect/</remarks>
    public class AjaxRedirectAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
                return;

            (filterContext.Result as RedirectResult).IfNotNull(result =>
                {
                    string destinationUrl = UrlHelper.GenerateContentUrl(result.Url, filterContext.HttpContext);
                    SetJsonResult(filterContext, destinationUrl);
                });
            (filterContext.Result as RedirectToRouteResult).IfNotNull(result =>
            {
                var routeCollection = (RouteCollection)RedirectToRouteResult_RouteProperty.GetValue(result, null);
                string destinationUrl = UrlHelper.GenerateUrl(result.RouteName, null, null, result.RouteValues, routeCollection, filterContext.RequestContext, false);
                SetJsonResult(filterContext, destinationUrl);
            });
        }

        static PropertyInfo RedirectToRouteResult_RouteProperty = typeof(RedirectToRouteResult).GetProperty("Routes", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        private static void SetJsonResult(ActionExecutedContext filterContext, string destinationUrl)
        {
            filterContext.Result = new JsonResult()
            {
                Data = new
                {
                    _RedirectUrl = destinationUrl
                }
            };
        }
    }

}
