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
using Mediascend.Web;
namespace Takeoff
{


    /// <summary>
    /// If the user has IE6, this will present them with a page that tells them they have a crappy old browser.  It gives them options to upgrade but they can also 
    /// choose to proceed anyway.  This check is done only when they first visit the site.  A cookie also prevents unnecessary future checks, so this won't slow the app down.
    /// </summary>
    public class WarnAboutIE6Attribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            //this will be true when it's their first visit to the site (will happen again if they clear cookies)
            if (request.IsGet() && !request.IsAjaxRequest() && request.UrlReferrer == null && request.Cookies["browserChecked"] == null)
            {
                //give old IE users a warning the first time
                if (request.Browser.Browser.Trim().ToUpperInvariant().Equals("IE") && request.Browser.MajorVersion <= 6)
                {
                    filterContext.Controller.ViewData["RequestedUrl"] = request.Url.ToString();
                    filterContext.Result = new ViewResult
                    {
                        ViewData = SetViewDataAttribute.FillViewData(new ViewDataDictionary(), filterContext.HttpContext, filterContext.ActionDescriptor),
                        ViewName = "InternetExplorerOldWarning",
                    };
                }

                filterContext.HttpContext.Response.AppendCookie(new HttpCookie("browserChecked", "true"));
            }

        }
    }
}
