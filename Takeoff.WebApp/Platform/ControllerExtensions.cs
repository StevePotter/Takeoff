using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.ViewModels;
using System.Linq.Expressions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Takeoff.Resources;

namespace Takeoff.Controllers
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Adds a warning banner to the top area of the page.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="message"></param>
        public static void AddBanner(this Controller controller, string message, bool htmlEncode)
        {
            var banners = (List<Banner>)controller.ViewBag.__banners;
            if (banners == null)
            {
                banners = new List<Banner>();
                controller.ViewBag.__banners = banners;
            }
            banners.Add(new Banner
            {
                HtmlEncode = htmlEncode,
                Source = message,
            });
        }


        public static ViewResult SuccessMessage(this Controller controller, string message, string heading = "", string pageTitle = "")
        {
            controller.ViewData.Model = new Message
            {
                Heading = heading,
                Text = message,
                PageTitle = pageTitle,
            };

            return new ViewResult
            {
                ViewName = "SuccessMessage",
                ViewData = controller.ViewData,
                TempData = controller.TempData,
            };
        }


        public static ViewResult ErrorMessage(this Controller controller, string message, string heading = "", string pageTitle = "")
        {
            controller.ViewData.Model = new Message
            {
                Heading = heading,
                Text = message,
                PageTitle = pageTitle,
            };

            return new ViewResult
            {
                ViewName = "ErrorMessage",
                ViewData = controller.ViewData,
                TempData = controller.TempData,
            };
        }


        /// <summary>
        /// Returns a generic error about a link that doesn't apply to the current user.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static ViewResult ObsoleteLink(this Controller controller)
        {            
            return new ViewResult
            {
                ViewName = "LinkDoesNotApply",
                ViewData = controller.ViewData,
                TempData = controller.TempData,
            };
        }


        /// <summary>
        /// Handles all the logic for returning html, json, or xml for regular, AJAX, and API (in future) calls.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="viewResult"></param>
        /// <param name="dataForJsonXmlAjaxApi"></param>
        /// <returns></returns>
        /// <remarks>Later on you can plug in xml responses as well as returning xml/json for api requests. </remarks>
        public static ActionResult Result(this Controller controller, Func<ViewResult> viewResult, Func<object> dataForAjaxOrApi)
        {
            var request = controller.HttpContext.Request;
            if (request.IsWebPageRequestOrNonAjaxFormPost())
            {
                return viewResult();
            }
            else
            {
                //later on you can do xml
                return new JsonResult
                {
                    Data = dataForAjaxOrApi(),
                };
            }
        }



        /// <summary>
        /// Handles logic for returning results depending on whether it's ajax, mobile, or API calls.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="nonAjaxHtmlRequest"></param>
        /// <param name="dataForJsonXmlAjaxApi"></param>
        /// <returns></returns>
        /// <remarks>Later on you can plug in xml responses as well as returning xml/json for api requests. </remarks>
        public static ActionResult Result(this HttpContextBase context, Func<ViewResult> nonAjaxHtmlRequest, Func<object> dataForAjaxOrApi)
        {
            var request = context.Request;
            if (request.IsAjaxRequest())
            {
                //later on you can do xml
                return new JsonResult
                {
                    Data = dataForAjaxOrApi(),
                };
            }
            else
            {
                return nonAjaxHtmlRequest();
            }
        }


        

        /// <summary>
        /// Used when doing "manual" validation, this adds a pretty, translated "required" validation message to the modelstate.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="modelState"></param>
        /// <param name="model"></param>
        /// <param name="modelPropertyExpr"></param>
        public static void AddRequiredError<TModel>(this ModelStateDictionary modelState, TModel model, Expression<Func<TModel, object>> modelPropertyExpr)
        {
            var property = modelPropertyExpr.Body.CastTo<MemberExpression>().Member;
            var name = property.Attribute<DisplayNameAttribute>().MapIfNotNull(a => a.DisplayName) ?? property.Name;
            var requiredAtt = property.Attribute<RequiredAttribute>();
            var msg = requiredAtt == null ? string.Format(S.Validation_Required, name) : requiredAtt.FormatErrorMessage(name);
            modelState.AddModelError(property.Name, msg);
        }


        public static ActionResult Forbidden(this Controller controller, string htmlViewName = "NoPermission", string errorCode = null, string errorDescription = null, string resolveUrl = null)
        {
            if (controller.Request.IsWebPageRequestOrNonAjaxFormPost())//contenttype checks ensure xml, json, etc are excluded
            {
                return new ViewResult
                {
                    ViewName = htmlViewName ?? "NoPermission",
                };
            }
            return new NonHtmlErrorResponse
            {
                ErrorCode = errorCode,
                ErrorDescription = errorDescription,
                StatusCode = HttpStatusCode.Forbidden
            };
        }

        /// <summary>
        /// Returns a 401 or login page, depending on request type.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static ActionResult Login(this Controller controller)
        {
            var request = controller.HttpContext.Request;
            if (request.IsWebPageRequestOrNonAjaxFormPost())//contenttype checks ensure xml, json, etc are excluded
            {
                return new ViewResult
                {
                    ViewName = "Login",
                    ViewData = new ViewDataDictionary(new Account_Login
                    {
                        ReturnUrl = controller.HttpContext.Request.Url.MapIfNotNull(m => m.OriginalString),
                    }),
                };
            }
            return new Http401NoLoginPageResult();
        }


        public static ActionResult Invalid(this Controller controller)
        {
            return controller.Invalid(null);
        }

        public static ActionResult Invalid(this Controller controller, Func<ViewResult> viewResult)
        {
            if (controller.Request.IsWebPageRequestOrNonAjaxFormPost() && viewResult != null)//contenttype checks ensure xml, json, etc are excluded
            {
                return viewResult();
            }
            Dictionary<string,object> invalidProperties = new Dictionary<string, object>();
            foreach (KeyValuePair<string, ModelState> modelProperty in controller.ModelState)
            {
                if (modelProperty.Value.Errors.HasItems())
                {
                    invalidProperties.Add(modelProperty.Key.CharsOrEmpty(), modelProperty.Value.Errors.Select(e => e.ErrorMessage).Where(e => e.HasChars()).ToArray());
                }
            }
            return new NonHtmlResponse
            {
                Data = new {
                    Errors = invalidProperties,
                    ErrorCode = ErrorCodes.InvalidInput,
                    ErrorDescription = ErrorCodes.InvalidInputDescription,
                },
                StatusCode = HttpStatusCode.BadRequest
            };
        }

        public static ActionResult Success(this Controller controller, Func<ActionResult> htmlResponse, object nonHtmlResponseData)
        {
            if (controller.Request.IsWebPageRequestOrNonAjaxFormPost() && htmlResponse != null)//contenttype checks ensure xml, json, etc are excluded
            {
                return htmlResponse();
            }

            return new NonHtmlResponse
            {
                Data = nonHtmlResponseData,
                StatusCode = HttpStatusCode.OK
            };
        }


        public static ActionResult DataNotFound(this Controller controller)
        {
            if (controller.Request.IsWebPageRequestOrNonAjaxFormPost())//contenttype checks ensure xml, json, etc are excluded
            {
                return new ViewResult
                {
                    ViewName = "Error.ThingNotFound",
                };
            }
            return new NonHtmlErrorResponse
            {
                ErrorCode = ErrorCodes.NotFound,
                ErrorDescription = ErrorCodes.NotFoundDescription,
                StatusCode = HttpStatusCode.NotFound
            };
        }

        public static IIdentityService IdentityService(this Controller controller)
        {
            return IoC.Get<IIdentityService>();
        }


        public static T Repository<T>(this Controller controller)
        {
            return IoC.Get<T>();
        }

        //return CreateErrorResult(filterContext, "Productions-NotFound", null, ErrorCodes.NotFound, ErrorCodes.NotFoundDescription, HttpStatusCode.NotFound);



    }
}

namespace Takeoff
{
    /// <summary>
    /// Data for a banner shown at the top of a page.  This is for important messages.
    /// </summary>
    public class Banner
    {
        public string Source
        {
            get;
            set;
        }

        public bool HtmlEncode
        {
            get;
            set;
        }

    }
}