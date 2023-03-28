using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Takeoff.Models;
using Newtonsoft.Json;

namespace Takeoff
{
    /// <summary>
    /// Saves the request/response data in SQL and S3.
    /// </summary>
    public class LogRequestAttribute : ActionFilterAttribute
    {
        public LogRequestAttribute()
        {
            SaveRequestData = true;
            SaveResponseData = true;
        }
        public LogRequestAttribute(bool saveRequest, bool saveResponse)
        {
            SaveRequestData = saveRequest;
            SaveResponseData = saveResponse;
        }
        
        public bool SaveRequestData { get; set; }
        public bool SaveResponseData { get; set; }
     
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.HttpContext.StartLogging(filterContext.RouteData, SaveRequestData, true, SaveResponseData);
        }

    }

}
