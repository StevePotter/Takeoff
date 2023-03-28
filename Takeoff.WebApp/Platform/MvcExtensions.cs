using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.Web.UI.WebControls;
using System.Runtime.Serialization.Json;
using System.IO;
using Newtonsoft.Json;
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
    /// <summary>
    /// Various extension methods for Mvc objects.
    /// </summary>
    public static class MvcExtensions
    {

        /// <summary>
        /// Encodes the object for use as a value in an html attribute.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Attribute(this HtmlHelper helper, object value)
        {
            return helper.Attribute(Convert.ToString(value));
        }

        /// <summary>
        /// Encodes the object for use as a value in an html attribute.
        /// </summary>
        public static string Attribute(this HtmlHelper helper, string value)
        {
            return HttpUtility.HtmlAttributeEncode(value);
        }

        /// <summary>
        /// Encodes the object for use as a json object graph.
        /// </summary>
        public static IHtmlString Json(this HtmlHelper helper, object value)
        {
            return helper.Raw(new JavaScriptSerializer().Serialize(value));
        }

        public static IHtmlString Json(this HtmlHelper helper, object value, bool indent)
        {
            return helper.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(value, indent ? Formatting.Indented : Formatting.None));
        }

        public static IEnumerable<SelectListItem> SelectItems(this Type enumType)
        {
            var values = System.Enum.GetValues(enumType);
            for (var i = 0; i < values.Length; i++)
            {
                yield return new SelectListItem
                {
                    Text = values.GetValue(i).ToString(),
                    Value = values.GetValue(i).ToString()
                
                };
            }
        }

        public static HttpVerbs HttpVerb(this Controller controller)
        {
            switch (controller.HttpContext.Request.GetHttpMethodOverride())
            {
                case "GET":
                    return HttpVerbs.Get;
                case "POST":
                    return HttpVerbs.Post;
                case "PUT":
                    return HttpVerbs.Put;
                case "DELETE":
                    return HttpVerbs.Delete;
                case "HEAD":
                    return HttpVerbs.Head;
                default:
                    throw new InvalidOperationException("No verb available");
            }
        }


        /// <summary>
        /// Returns true if it's a non-ajax, GET request to an html resource.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsWebPageRequest(this HttpRequestBase request)
        {
            return request.IsGet() && !request.IsAjaxRequest() && (string.IsNullOrWhiteSpace(request.ContentType) || 
                request.ContentType.Contains("htm", StringComparison.OrdinalIgnoreCase));//contenttype checks ensure xml, json, etc are excluded
        }

        /// <summary>
        /// Returns true if it's a web page request or a non-ajax form post.  In this case you can return a redirect or html.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsWebPageRequestOrNonAjaxFormPost(this HttpRequestBase request)
        {
            if (request.IsAjaxRequest())
                return false;
            //html content and form posts get a view result
            return (string.IsNullOrWhiteSpace(request.ContentType)
                || request.ContentType.Contains("htm", StringComparison.OrdinalIgnoreCase)
                || request.ContentType.Contains("form", StringComparison.OrdinalIgnoreCase)//form posts are "application/x-www-form-urlencoded" or "multipart/form-data"
                );
        }

        /// <summary>
        /// Returns an Emptyresult for a controller.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static EmptyResult Empty(this Controller controller)
        {
            return new EmptyResult();
        }

        public static HttpStatusCodeResult StatusCode(this Controller controller, HttpStatusCode code)
        {
            return new HttpStatusCodeResult((int)code);
        }

        public static RedirectResult RedirectToAction<T>(this Controller controller, Expression<Action<T>> action, UrlType urlType) where T : Controller
        {
            var url = controller.Url.Action(action, urlType);
            return new RedirectResult(url);
        }

        public static RedirectResult RedirectToAction<T>(this T controller, Expression<Action<T>> action, UrlType urlType) where T : Controller
        {
            var url = controller.Url.Action(action, urlType);
            return new RedirectResult(url);
        }


        public static string WriteHideCssIf(this HtmlHelper html, bool condition)
        {
            return WriteHideCssIf(html, condition, true);
        }

        public static string WriteHideCssIf(this HtmlHelper html, bool condition, bool addStyleAttribute)
        {

            return condition ? (addStyleAttribute ? " style=\"display:none;\" " : "display:none;") : string.Empty;
        }


        public static DateTime RequestDate(this Controller controller)
        {
            return controller.HttpContext.RequestDate();
        }

        /// <summary>
        /// When the request began.  This is useful for when passing log dates around, but the actual logging is deferred.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DateTime RequestDate(this HttpContextBase context)
        {
            var date = context.Items["beginDate"];
            if ( date == null)
            {
                var now = DateTime.UtcNow;
                context.Items["beginDate"] = now;
                return now;
            }
            return (DateTime)date;
        }

        public static bool IsPost(this HttpRequestBase request)
        {
            var method = request.HttpMethod;
            if (string.IsNullOrEmpty(method))
                return false;
            return "POST".Equals(method.ToUpperInvariant());
        }
        public static bool IsGet(this HttpRequestBase request)
        {
            var method = request.HttpMethod;
            if (string.IsNullOrEmpty(method))
                return false;
            return "GET".Equals(method.ToUpperInvariant());
        }


        
        /// <summary>
        /// If the value is in the dictionary, it returns the casted value.  Otherwise it returns the default value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="val"></param>
        /// <param name="valIfNotNull"></param>
        /// <returns></returns>
        public static T ValueOrDefault<T>(this ViewDataDictionary dictionary, string key, T defaultVal) 
        {
            object val;
            if (dictionary.TryGetValue(key, out val))
            {
                return (T)val;
            }
            else
            {
                return defaultVal;
            }
        }


        public static void AddBodyCss(this ViewDataDictionary viewData, params string[] classes)
        {
            viewData.BodyCssClasses().AddRange(classes);
        }

        public static void AddBodyCss(this HtmlHelper html, params string[] classes)
        {
            html.ViewData.BodyCssClasses().AddRange(classes);
        }

        public static List<string> BodyCssClasses(this ViewDataDictionary viewData)
        {
            return viewData.Ensure("_BodyClasses", () => new List<string>()).CastTo<List<string>>();
        }


    }



    public static class SectionExtensions
    {
        private static readonly object _o = new object();
        public static HelperResult RenderSection(this WebPageBase page,
                                string sectionName,
                                Func<object, HelperResult> defaultContent)
        {
            if (page.IsSectionDefined(sectionName))
            {
                return page.RenderSection(sectionName);
            }
            else
            {
                return defaultContent(_o);
            }
        }

        public static HelperResult RedefineSection(this WebPageBase page,
                                string sectionName)
        {
            return RedefineSection(page, sectionName, defaultContent: null);
        }

        public static HelperResult RedefineSection(this WebPageBase page,
                                string sectionName,
                                Func<object, HelperResult> defaultContent)
        {
            if (page.IsSectionDefined(sectionName))
            {
                page.DefineSection(sectionName,
                                   () => page.Write(page.RenderSection(sectionName)));
            }
            else if (defaultContent != null)
            {
                page.DefineSection(sectionName,
                                   () => page.Write(defaultContent(_o)));
            }
            return new HelperResult(_ => { });
        }
    }


    /// <summary>
    /// Offers custom, method-based validation.  This is similar to the CustomValidation attribuet that ships with the framework but is simpler to use.  Just define a method "Is{propname}Valid" and it'll just work.  
    /// The method can return true or false.  Or an error string.  Or a ValidationResult object.  Or null (success).
    /// </summary>
    public class MethodValidatonAttribute : ValidationAttribute
    {
        public MethodValidatonAttribute()
        {
        }
        public MethodValidatonAttribute(string methodName)
        {
        }

        private string MethodName;

        private MethodInfo ValidatorMethod;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (ValidatorMethod == null)
            {
                if (!MethodName.HasChars())
                    MethodName = "Is" + validationContext.DisplayName + "Valid";

                ValidatorMethod = validationContext.ObjectType.GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }

            var validationResult = ValidatorMethod.Invoke(validationContext.ObjectInstance, new object[] { value });
            if (validationResult == null)
                return ValidationResult.Success;
            if (validationResult is Boolean)
            {
                if ((bool)validationResult)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    string[] memberNames = (validationContext.MemberName != null) ? new string[] { validationContext.MemberName } : null;
                    return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName), memberNames);
                }
            }
            if (validationResult is ValidationResult)
                return (ValidationResult)validationResult;
            if (validationResult is string)
            {
                if (string.IsNullOrEmpty((string)validationResult))
                    return ValidationResult.Success;
                return new ValidationResult((string)validationResult);
            }
            throw new InvalidOperationException("Invalid return type.");
        }
    }

    public class HasCharactersAttribute:RequiredAttribute
    {
        public HasCharactersAttribute():this(CharsThatMatter.Any)
        {        
        }

        public HasCharactersAttribute(CharsThatMatter whatMatters)
        {
            WhatMatters = whatMatters;
        }

        public CharsThatMatter WhatMatters { get; set; }

        public override bool IsValid(object value)
        {
            return (value as string).HasChars(WhatMatters);
        }
    }

    public class EmailAttribute : RegularExpressionAttribute
    {
        public EmailAttribute()
            : base(@"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$")
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return base.FormatErrorMessage(name);
        }
    }

    public class PositiveIntegerAttribute : RegularExpressionAttribute
    {
        public PositiveIntegerAttribute()
            : base(@"\d{1,10}")
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return base.FormatErrorMessage(name);
        }

    }



}
