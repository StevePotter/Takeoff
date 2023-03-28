using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.Web.UI.WebControls;
using System.Runtime.Serialization.Json;
using System.IO;
using Takeoff.Data;
using Takeoff.Models;
using System.Linq.Expressions;
using MvcContrib;
using System.Web.WebPages;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Web.Mvc.Html;
using Takeoff.Resources;
using Mediascend.Web;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Web.Routing;
using System.Globalization;

namespace Takeoff
{
    /// <summary>
    /// Various extension methods for Mvc objects.
    /// </summary>
    public static class UserExtensions
    {
        private static readonly IIdentityService IdentityService = IoC.Get<IIdentityService>();


        public static bool IsLoggedIn(this HttpContextBase context)
        {
            return IdentityService.GetIdentity(context) as UserIdentity != null;
        }

        /// <summary>
        /// Gets the currently logged in user.  Returns null if no user is logged in.
        /// </summary>
        public static UserThing UserThing(this HttpContextBase context)
        {
            var userIdentity = IdentityService.GetIdentity(context) as UserIdentity;
            if (userIdentity == null)
                return null;

            return Things.GetOrNull<UserThing>(userIdentity.UserId);
        }

        /// <summary>
        /// Gets the currently logged in user.  Returns null if no user is logged in.
        /// </summary>
        public static IUser UserThing2(this HttpContextBase context)
        {
            var userIdentity = IdentityService.GetIdentity(context) as UserIdentity;
            if (userIdentity == null)
                return null;

            return Repos.Users.Get(userIdentity.UserId);
        }


        public static int UserId(this Controller controller)
        {
            var userIdentity = IdentityService.GetIdentity(controller.HttpContext) as UserIdentity;
            if (userIdentity == null)
            {
                return 0;
            }
            else
            {
                return userIdentity.UserId;
            }

        }

        public static int? UserIdOrDefault(this Controller controller)
        {
            return UserIdOrDefault(controller.HttpContext);
        }


        public static int? UserIdOrDefault(this HttpContextBase context)
        {
            var userIdentity = IdentityService.GetIdentity(context) as UserIdentity;
            if (userIdentity == null)
            {
                return new int?();
            }
            else
            {
                return userIdentity.UserId;
            }

        }



        public static bool IsLoggedIn(this Controller controller)
        {
            return controller.HttpContext.IsLoggedIn();
        }

        /// <summary>
        /// Gets the currently logged in user.  Returns null if no user is logged in.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static UserThing UserThing(this Controller controller)
        {
            return controller.HttpContext.UserThing();
        }

        /// <summary>
        /// Gets the current user's identity.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static Identity Identity(this Controller controller)
        {
            return IdentityService.GetIdentity(controller.HttpContext);
        }

        public static Identity Identity(this HttpContextBase context)
        {
            return IdentityService.GetIdentity(context);
        }


        /// <summary>
        /// Meant to replace UserThing with an interface-based user that can be mocked.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static IUser UserThing2(this Controller controller)
        {
            return controller.HttpContext.UserThing2();
        }

 

    }
}