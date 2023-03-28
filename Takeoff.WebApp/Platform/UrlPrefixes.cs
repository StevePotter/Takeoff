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
using Takeoff.ViewModels;

namespace Takeoff
{
    public static class UrlHelperForPrefixes
    {
//        public static IAppUrlPrefixes Prefixes = ;

        /// <summary>
        /// Gets the relative or absolute path, depending on secure and whether current request is secure, to the root of the app.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="secure"></param>
        /// <returns></returns>
        public static IAppUrlPrefixes UrlPrefixes(this UrlHelper helper)
        {
            var context = helper.RequestContext.HttpContext;
            var fromItems = context.Items["__UrlPrefixes"];
            if (fromItems != null)
                return (IAppUrlPrefixes)fromItems;

            var prefixes = IoC.Get<IAppUrlPrefixes>();
            prefixes.Fill(context, helper);
            context.Items["__UrlPrefixes"] = prefixes;
            return prefixes;
        }


        public static string Action(this UrlHelper helper, string actionName, string controllerName, UrlType type)
        {
            return Action(helper, actionName, controllerName, null, type);
        }

        public static string Action(this UrlHelper helper, string actionName, string controllerName, object routeValues, UrlType type)
        {
            return helper.UrlPrefixes().FromRelative(helper.Action(actionName, controllerName, routeValues), type);
        }

        public static string Action<TController>(this UrlHelper helper, Expression<Action<TController>> expression, UrlType type) where TController : Controller
        {
            var relative = helper.Action<TController>(expression);
            return helper.UrlPrefixes().FromRelative(relative, type);
        }

        /// <summary>
        /// Necessary because calling it Action caused aspx views to use the default Action overload that treated type as routeValues.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string Action2(this UrlHelper helper, string actionName, string controllerName, UrlType type)
        {
            return Action2(helper, actionName, controllerName, null, type);
        }

        /// <summary>
        /// Necessary because calling it Action caused aspx views to use the default Action overload that treated type as a string for some reason.
        /// </summary>
        public static string Action2(this UrlHelper helper, string actionName, string controllerName, object routeValues, UrlType type)
        {
            return helper.UrlPrefixes().FromRelative(helper.Action(actionName, controllerName, routeValues), type);
        }


        /// <summary>
        /// Shortcut to the Details action for typical controllers.  The controller should take a parameter called id.
        /// </summary>
        public static string ActionDetails(this UrlHelper helper, string controllerName, object id, UrlType type)
        {
            return Action(helper, "Details", controllerName, new { id = id, area = "" }, type);
        }


        /// <summary>
        /// Shortcut to the Details action for typical controllers.  The controller should take a parameter called id.  Url will be relative to app root, no matter what hte protocol.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="controllerName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string ActionDetails(this UrlHelper helper, string controllerName, object id)
        {
            return helper.Action("Details", controllerName, new { id = id, area = "" });
        }


        public static string Production(this UrlHelper helper, ProjectThing production, UrlType type)
        {
            if ( production == null)
                throw new ArgumentNullException("production");
            string relativeUrl;
            if ( production.VanityUrl.HasChars() )
            {
                relativeUrl = production.VanityUrl;
            }
            else
            {
                relativeUrl = "productions/" + production.Id.ToInvariant();
            }
            return helper.UrlPrefixes().FromRelative(relativeUrl, type);
        }

    }

    public interface IAppUrlPrefixes
    {
        /// <summary>
        /// A prefix that will be relative to the app root regardless of protocol.
        /// </summary>
        string Relative { get; }

        /// <summary>
        /// A non https prefix that will be relative if the current page is http (not https).  Otherwise it will be absolute.
        /// </summary>
        string RelativeHttp { get; }

        /// <summary>
        /// An http prefix that will be relative if the current page is https.  Otherwise it will be absolute.
        /// </summary>
        string RelativeHttps { get; }

        /// <summary>
        /// A prefix for an absolute url to the non https root of the app.
        /// </summary>
        string AbsoluteHttp { get; }

        /// <summary>
        /// A prefix for an absolute url to the https root of the app.
        /// </summary>
        string AbsoluteHttps { get; }

        /// <summary>
        /// A prefix for assets, which may be on a CDN or local.
        /// </summary>
        string Asset { get; }

        string Get(UrlType type);

        /// <summary>
        /// Takes a url that is relative to the app root, such as "/Account/Verify", and converts it to the proper type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string FromRelative(string appRootUrl, UrlType type);

        void Fill(HttpContextBase context, UrlHelper helper);
    }

    public class AppUrlPrefixes : IAppUrlPrefixes
    {
        public AppUrlPrefixes()
        {
            var enableSecure = ApplicationEx.EnableHttps;
            if (enableSecure)
            {
                AbsoluteHttp = ApplicationEx.AppUrlPrefix.EndWith("/");
                AbsoluteHttps = ApplicationEx.AppUrlPrefixSecure.EndWith("/");
            }
            else
            {
                AbsoluteHttp = ApplicationEx.AppUrlPrefix.EndWith("/");
                AbsoluteHttps = AbsoluteHttp;
            }
        }
        public void Fill(HttpContextBase context, UrlHelper helper)
        {
            var enableSecure = ApplicationEx.EnableHttps;
            var isSecureRequest = enableSecure && context.Request.IsSecureConnection;

            Asset = helper.Asset("");
            Relative = helper.Content("~/");

            if (enableSecure)
            {
                RelativeHttp = isSecureRequest ? AbsoluteHttp : helper.Content("~/");
                RelativeHttps = isSecureRequest ? helper.Content("~/") : AbsoluteHttps;
            }
            else
            {
                RelativeHttp = helper.Content("~/");
                RelativeHttps = RelativeHttp;
            }
            
        }

        /// <summary>
        /// A prefix that will be relative to the app root regardless of protocol.
        /// </summary>
        public string Relative
        {
            get;
            set;
        }

        /// <summary>
        /// A non https prefix that will be relative if the current page is http (not https).  Otherwise it will be absolute.
        /// </summary>
        public string RelativeHttp
        {
            get;
            set;
        }

        /// <summary>
        /// An http prefix that will be relative if the current page is https.  Otherwise it will be absolute.
        /// </summary>
        public string RelativeHttps
        {
            get;
            set;
        }

        /// <summary>
        /// A prefix for an absolute url to the non https root of the app.
        /// </summary>
        public string AbsoluteHttp
        {
            get;
            set;
        }

        /// <summary>
        /// A prefix for an absolute url to the https root of the app.
        /// </summary>
        public string AbsoluteHttps
        {
            get;
            set;
        }


        /// <summary>
        /// A prefix for assets, which may be on a CDN or local.
        /// </summary>
        public string Asset
        {
            get;
            set;
        }


        public string Get(UrlType type)
        {
            switch (type)
            {
                case UrlType.Relative:
                    return Relative;
                case UrlType.AbsoluteHttp:
                    return AbsoluteHttp;
                case UrlType.AbsoluteHttps:
                    return AbsoluteHttps;
                case UrlType.RelativeHttp:
                    return RelativeHttp;
                case UrlType.RelativeHttps:
                    return RelativeHttps;
                default:
                    throw new ArgumentException();
            }
        }


        /// <summary>
        /// Takes a url that is relative to the app root, such as "/Account/Verify", and converts it to the proper type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string FromRelative(string appRootUrl, UrlType type)
        {
            switch (type)
            {
                case UrlType.Relative:
                    return Relative.EndWith("/") + appRootUrl.StartWithout("/");
                case UrlType.AbsoluteHttp:
                    return AbsoluteHttp.EndWith("/") + appRootUrl.StartWithout("/");
                case UrlType.AbsoluteHttps:
                    return AbsoluteHttps.EndWith("/") + appRootUrl.StartWithout("/");
                case UrlType.RelativeHttp:
                    return RelativeHttp.EndWith("/") + appRootUrl.StartWithout("/");
                case UrlType.RelativeHttps:
                    return RelativeHttps.EndWith("/") + appRootUrl.StartWithout("/");
                default:
                    throw new ArgumentException();
            }
        }
    }


    public enum UrlType
    {
        /// <summary>
        /// A url that will be relative to the app root regardless of protocol.
        /// </summary>
        Relative,

        /// <summary>
        /// A non https url that will be relative if the current page is http (not https).  Otherwise it will be absolute.
        /// </summary>
        RelativeHttp,

        /// <summary>
        /// An http url that will be relative if the current page is https.  Otherwise it will be absolute.
        /// </summary>
        RelativeHttps,

        /// <summary>
        /// A url for an absolute url to the non https root of the app.
        /// </summary>
        AbsoluteHttp,

        /// <summary>
        /// A url for an absolute url to the https root of the app.
        /// </summary>
        AbsoluteHttps,
    }

}
