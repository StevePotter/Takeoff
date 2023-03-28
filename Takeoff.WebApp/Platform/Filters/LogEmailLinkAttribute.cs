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

    /// <summary>
    /// HTML emails get a "_messageid" query string parameter that points to the email.  This will detect that, and if found, log the click.
    /// </summary>
    public class LogEmailLinkAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.RequestContext == null || filterContext.RequestContext.HttpContext == null)
                return;
            var request = filterContext.RequestContext.HttpContext.Request;
            if (!request.IsWebPageRequest())
                return;

            var strMessageId = request[OutgoingMail.LinkClickQueryParameter];
            if ( !strMessageId.HasChars())
                return;

            Guid messageId;
            if (!Guid.TryParse(strMessageId, out messageId))
                return;


            filterContext.RequestContext.HttpContext.Defer(() => OutgoingMail.LogLinkClick(messageId, request.RawUrl, filterContext.RequestContext.HttpContext.RequestDate()));
        }
    }



}
