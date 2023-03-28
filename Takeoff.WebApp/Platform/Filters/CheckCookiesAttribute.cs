using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Security;
using Takeoff.Controllers;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.ViewModels;

namespace Takeoff
{

    public class CheckCookiesAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.Cookies.Count == 0)
            {
                filterContext.Controller.ViewData["CheckForCookies"] = true;
            }
        }
    }
}
