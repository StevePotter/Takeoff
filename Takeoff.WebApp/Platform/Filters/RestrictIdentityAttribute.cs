using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
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
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.ViewModels;

namespace Takeoff
{

    /// <summary>
    /// Makes it possible to pick a combination of identity types to not allow.  Anonymous users are automatically forbidden.
    /// By default, it'll allow any member but no guests or anonymous peeps.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class RestrictIdentityAttribute : Authorize2Attribute
    {
        public RestrictIdentityAttribute()
        {
            AllowDemo = true;
            AllowSemiAnonymous = false;
        }

        /// <summary>
        /// Whether semi-anonymous users should be allowed in.  Default is false.
        /// </summary>
        public bool AllowSemiAnonymous { get; set; }

        /// <summary>
        /// Whether demo users have access.  Default is true.
        /// </summary>
        public bool AllowDemo { get; set; }

        /// <summary>
        /// If false and the user has no account, nothing will happen.  Default is false.
        /// </summary>
        public bool RequireAccount { get; set; }

        /// <summary>
        /// When set, this will require a an accoun with a certain status.  
        /// </summary>
        public AccountStatus[] RequireAccountStatus
        {
            get { return _RequireAccountStatus; }
            set
            {
                _RequireAccountStatus = value;
                requireAccountStatusesLookup = new HashSet<AccountStatus>();
                if (value.HasItems())
                {
                    value.Each(v => requireAccountStatusesLookup.Add(v));
                }
            }
        }
        private AccountStatus[] _RequireAccountStatus;

        private HashSet<AccountStatus> requireAccountStatusesLookup; 

        protected override ActionResult GetUnauthorizedResult(AuthorizationContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            var identity = httpContext.Identity();
            if (identity == null)
                return CreateUnauthorizedResult(filterContext);

            var user = httpContext.UserThing2();
            IAccount account = user == null ? null : user.Account;

            if (identity is SemiAnonymousUserIdentity && !AllowSemiAnonymous)
            {
                return CreateForbiddenResult(filterContext, "SemiAnonymousUserForbidden", ErrorCodes.GuestForbidden, ErrorCodes.GuestForbiddenDescription);
            }

            if ( (RequireAccount || RequireAccountStatus.HasItems()) && account == null)
            {
                return CreateForbiddenResult(filterContext, "Account-Required", ErrorCodes.AccountRequired, ErrorCodes.AccountRequiredDescription);
            }

            if (RequireAccountStatus.HasItems() && !requireAccountStatusesLookup.Contains(account.Status))
            {
                return CreateForbiddenResult(filterContext, "LinkDoesNotApply", ErrorCodes.AccountStatusInvalid, ErrorCodes.AccountStatusInvalidDescription);
            }

            if (account != null)
            {
                if (!AllowDemo && account.Status == AccountStatus.Demo)
                {
                    return CreateForbiddenResult(filterContext, "DemoForbidden", ErrorCodes.DemoForbidden, ErrorCodes.DemoForbiddenDescription);
                }
            }
            return null;
        }

        private ActionResult CreateUnauthorizedResult(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            if (request.IsWebPageRequestOrNonAjaxFormPost())//contenttype checks ensure xml, json, etc are excluded
            {
                return new ViewResult
                {
                    ViewName = "Login",
                    ViewData = new ViewDataDictionary(new Account_Login
                    {
                        ReturnUrl =
                            filterContext.HttpContext.Request.Url.
                            MapIfNotNull(
                                m => m.OriginalString),
                    }),
                };
            }
            return new Http401NoLoginPageResult();
        }
    }


}
