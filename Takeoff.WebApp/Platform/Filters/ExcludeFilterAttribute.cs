using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Security;

namespace Takeoff.Controllers
{

    /// <summary>
    /// This will disable any filters of the given type from being applied.  This is useful when, say, all but on action need the Authorize filter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class, AllowMultiple=true)]
    public class ExcludeFilterAttribute : ActionFilterAttribute
    {
        public ExcludeFilterAttribute(Type toExclude)
        {
            FilterToExclude = toExclude;
        }

        /// <summary>
        /// The type of filter that will be ignored.
        /// </summary>
        public Type FilterToExclude
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// A subclass of ControllerActionInvoker that implements the functionality of IgnoreFilterAttribute.  To use this, just override Controller.CreateActionInvoker() and return an instance of this.
    /// </summary>
    public class ControllerActionInvokerWithExcludeFilter : ControllerActionInvoker
    {
        protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            //base implementation does all the hard work.  we just prune off the filters to exclude
            var filterInfo = base.GetFilters(controllerContext, actionDescriptor);
            foreach( var toExclude in filterInfo.ActionFilters.OfType<ExcludeFilterAttribute>().Select(f=>f.FilterToExclude).ToArray() )
            {
                var exclude = toExclude;
                filterInfo.ActionFilters.RemoveAll(filter => exclude.IsAssignableFrom(filter.GetType()));
                filterInfo.AuthorizationFilters.RemoveAll(filter => exclude.IsAssignableFrom(filter.GetType()));
                filterInfo.ExceptionFilters.RemoveAll(filter => exclude.IsAssignableFrom(filter.GetType()));
                filterInfo.ResultFilters.RemoveAll(filter => exclude.IsAssignableFrom(filter.GetType()));
            }
            return filterInfo;
        }

    }


}
