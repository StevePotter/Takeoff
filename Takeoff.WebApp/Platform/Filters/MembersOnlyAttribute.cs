using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Takeoff.Data;
using Takeoff.Models;
using Mediascend.Web;
using MvcContrib;
using System.Security;
using Takeoff.Controllers;

namespace Takeoff
{

    /// <summary>
    /// Makes sure that only people who are registered users of Takeoff are allowed.  Anonymous and semi-anonymous users are not allow.
    /// </summary>
    public class MembersOnlyAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if ( filterContext.HttpContext.Identity() as SemiAnonymousUserIdentity != null )
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = "SemiAnonymousUserForbidden",
                };
            }
        }
    }

}
