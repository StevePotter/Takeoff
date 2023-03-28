using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Takeoff.Models;
using Mediascend.Web;
using MvcContrib;
using System.Security;
using Takeoff.Controllers;
using System.Diagnostics;

namespace Takeoff
{

    /// <summary>
    /// If the user is logged on and validated, we make sure they have the necessary basic user information entered:  name, password, how they heard, etc.
    /// </summary> 
    public class EnsureNecessaryUserInfoAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var context = filterContext.HttpContext;
            var user = context.UserThing();
            if (context.Request.IsAjaxRequest() || user == null || !user.IsVerified) //wait until they've verified their account to require this info.  normally people enter a password upon signup or click a link or login and hit "Forgot Password"
                return;

            //note: this ain't a pretty test but so what...
            if (user.FirstName.HasChars() && user.LastName.HasChars() && user.Password.HasChars())
                return;

            var url = new UrlHelper(filterContext.RequestContext).Action<AccountController>(c => c.NecessaryInfo(filterContext.HttpContext.Request.Url.OriginalString));
            filterContext.Result = new RedirectResult(url);
        }

    }

}