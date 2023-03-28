using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Takeoff
{
    //inspired from http://paydrotalks.com/posts/45-standard-json-response-for-rails-and-jquery

    public class NonHtmlErrorResponse: ActionResult
    {
        public HttpStatusCode? StatusCode { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorDescription { get; set; }

        /// <summary>
        /// Url to a page that can resolve the problem.
        /// </summary>
        public string ResolveUrl { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            new NonHtmlResponse
                {
                    StatusCode = StatusCode,
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


    public class NonHtmlResponse : ActionResult
    {
        public HttpStatusCode? StatusCode { get; set; }

        public object Data { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (StatusCode.HasValue)
            {
                context.HttpContext.Response.StatusCode = (int)this.StatusCode.Value;
            }

            new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = Data
            }.ExecuteResult(context);
        }
    }

}