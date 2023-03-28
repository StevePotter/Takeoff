using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Takeoff.Controllers;
using System.Net.Mail;
using System.Configuration;
using System.Web.Script.Serialization;
using System.IO;
using System.Web.Routing;
using System.Reflection;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Security;
using Mediascend.Web;
using Takeoff.Models;

namespace Takeoff
{

    [Flags]
    public enum SpecialRestriction
    {
        None = 0,
        Local = 1,
        DeferredRequest = 2,
        Staff = 4,
    }

    
    /// <summary>
    /// Allows for certain restrictions to be placed on things.  If the given request satisfies any of the restrictions, it's good.  Otherwise it throws an exception.
    /// </summary>
    public class SpecialRestrictionAttribute : Authorize2Attribute
    {
        public SpecialRestrictionAttribute(SpecialRestriction restriction)
        {
            Restriction = restriction;
        }

        public SpecialRestriction Restriction { get; set; }

        private bool HasRestriction(SpecialRestriction restriction)
        {
            return (Restriction & restriction) != 0;
        }

        protected override ActionResult GetUnauthorizedResult(AuthorizationContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            
            if (HasRestriction(SpecialRestriction.Local) && httpContext.Request.IsLocal)
                return null;

            if (HasRestriction(SpecialRestriction.DeferredRequest) && DeferredWebRequestHelpers.IsDeferredRequest(httpContext.Request))
                return null;

            if (HasRestriction(SpecialRestriction.Staff) && StaffController.IsCurrUserStaff(httpContext))
                return null;

            return CreateForbiddenResult(filterContext, "NoPermission", ErrorCodes.NoPermission, ErrorCodes.NoPermissionDescription);
        }
    }


}
