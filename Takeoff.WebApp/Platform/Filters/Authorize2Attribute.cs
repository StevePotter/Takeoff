using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Takeoff.Controllers;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.ViewModels;

namespace Takeoff
{

    /// <summary>
    /// Use this instead of [Authorize] in our controllers.
    /// </summary>
    /// <remarks>
    /// The advantage of this is that:
    /// 1.  It doesn't redirect to the login page when the request is made.  This is just a detail, but the original url is the same during first login post which is nice.  
    /// 2.  If a login page isn't supposed to be shown, like during ajax or an API request, a proper 401 is returned.
    /// </remarks>
    public abstract class Authorize2Attribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var result = GetUnauthorizedResult(filterContext);
            if ( result != null)
                filterContext.Result = result;
        }

        protected abstract ActionResult GetUnauthorizedResult(AuthorizationContext filterContext);

        protected ActionResult CreateForbiddenResult(AuthorizationContext filterContext, string htmlViewName, string errorCode, string errorDescription = null, string resolveUrl = null)
        {
            var request = filterContext.HttpContext.Request;
            if (request.IsWebPageRequestOrNonAjaxFormPost())//contenttype checks ensure xml, json, etc are excluded
            {
                return new ViewResult
                {
                    ViewName = htmlViewName,
                };
            }
            return new NonHtmlErrorResponse
            {
                ErrorCode = errorCode,
                ErrorDescription = errorDescription,
                StatusCode = HttpStatusCode.Forbidden
            };
        }
    }

}