using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Security;
using StackExchange.Profiling;
using Takeoff.Controllers;

namespace Takeoff
{

    /// <summary>
    /// This filter adds profile commands.
    /// </summary>
    public class ProfilerAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var profiler = MiniProfiler.Current;
            if (profiler != null)
            {
                using (profiler.Step("OnActionExecuting")) { }
            }
            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var profiler = MiniProfiler.Current;
            if (profiler != null)
            {
                profiler.Step("OnActionExecuted").Dispose();
            }
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var profiler = MiniProfiler.Current;
            if (profiler != null)
            {
                profiler.Step("OnResultExecuting").Dispose();
            }
            base.OnResultExecuting(filterContext);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            MiniProfiler.Current.IfNotNull(p => p.Step("OnResultExecuted").Dispose());

            base.OnResultExecuted(filterContext);
        }

    }



}
