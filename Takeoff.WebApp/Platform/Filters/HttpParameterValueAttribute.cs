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
    /// Allows selection of a given action method based on the value of some request parameter.  This is useful when you have multiple action method overloads and want to choose one based on value of a certain parameter, such as different types of signup.
    /// </summary>
    public class HttpParameterValueAttribute : ActionMethodSelectorAttribute
    {
        public HttpParameterValueAttribute(string parameterName, string parameterValue, bool caseSensitive)
        {
            this.parameterName = parameterName;
            this.parameterValue = parameterValue;
        }

        private readonly string parameterName, parameterValue;
        private readonly bool caseSensitive;

        public override bool IsValidForRequest(System.Web.Mvc.ControllerContext controllerContext, System.Reflection.MethodInfo methodInfo)
        {
            var toMatch = controllerContext.HttpContext.Request[parameterName];
            if ( toMatch == null)
                return false;
            return parameterValue.Equals(toMatch, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
        }

    }


}
