using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Takeoff
{
    /// <summary>
    /// Used for a bad request, such as hitting a page or api endpoint that is not applicable for the user (like signing up when they've already signed up.).
    /// Returns the specified view if the request was for an html page; otherwise (ajax, api) it returns an object, currently JSON.
    /// </summary>
    public class BadRequest : ActionResult
    {
        public string ErrorCode { get; set; }

        public string ErrorDescription { get; set; }

        public string ResolveUrl { get; set; }

        public string ViewName { get; set; }

        public object ViewModel { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if ( context.HttpContext.Request.IsWebPageRequestOrNonAjaxFormPost())
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                new ViewResult
                    {
                        ViewData = context.Controller.ViewData,
                        ViewName = ViewName.CharsOr("BadRequest"),                        
                    }.ExecuteResult(context);
            }
            else
            {
                new NonHtmlResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Data = new
                    {
                        Status = "Error",
                        ErrorCode = ErrorCode.CharsOrEmpty(),
                        ResolveUrl = ResolveUrl.CharsOrEmpty(),
                        Description = ErrorDescription.CharsOrEmpty(),
                    }
                }.ExecuteResult(context);                
            }
        }
    }


}