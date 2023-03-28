using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Takeoff
{
    public class NonHtmlSuccess : ActionResult
    {
        public object Data { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            new NonHtmlResponse
            {
                StatusCode = HttpStatusCode.OK,
                Data = this.Data,
            }.ExecuteResult(context);
        }
    }

}