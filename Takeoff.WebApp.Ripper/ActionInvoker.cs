using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using Takeoff.Controllers;

namespace Takeoff.WebApp.Ripper
{
    /// <summary>
    /// ActionInvoker for test rig.
    /// </summary>
    public class ActionInvoker : ControllerActionInvokerWithExcludeFilter
    {
        public ActionInvoker()
        {

        }

        protected override IDictionary<string, object> GetParameterValues(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            return ActionParams;
        }

        public override bool InvokeAction(ControllerContext controllerContext, string actionName)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (String.IsNullOrEmpty(actionName))
            {
                throw new ArgumentException("actionName");
            }

            ControllerDescriptor controllerDescriptor = GetControllerDescriptor(controllerContext);
            ActionDescriptor actionDescriptor = FindAction(controllerContext, controllerDescriptor, actionName);
            if (actionDescriptor != null)
            {
                FilterInfo filterInfo = GetFilters(controllerContext, actionDescriptor);

                try
                {
                    AuthorizationContext authContext = InvokeAuthorizationFilters(controllerContext, filterInfo.AuthorizationFilters, actionDescriptor);
                    if (authContext.Result != null)
                    {
                        Result = authContext.Result;
                        // the auth filter signaled that we should let it short-circuit the request
                        InvokeActionResult(controllerContext, authContext.Result);
                    }
                    else
                    {

                        IDictionary<string, object> parameters = GetParameterValues(controllerContext, actionDescriptor);
                        ActionExecutedContext postActionContext = InvokeActionMethodWithFilters(controllerContext, filterInfo.ActionFilters, actionDescriptor, parameters);
                        Result = postActionContext.Result;
                        InvokeActionResultWithFilters(controllerContext, filterInfo.ResultFilters, postActionContext.Result);
                    }
                }
                catch (ThreadAbortException)
                {
                    // This type of exception occurs as a result of Response.Redirect(), but we special-case so that
                    // the filters don't see this as an error.
                    throw;
                }
                catch (Exception ex)
                {
                    // something blew up, so execute the exception filters
                    ExceptionContext exceptionContext = InvokeExceptionFilters(controllerContext, filterInfo.ExceptionFilters, ex);
                    if (!exceptionContext.ExceptionHandled)
                    {
                        throw;
                    }
                    InvokeActionResult(controllerContext, exceptionContext.Result);
                }

                return true;
            }

            // notify controller that no method matched
            return false;
            //            return false;
            //            return base.InvokeAction(controllerContext, actionName);
        }

        public static IDictionary<string, object> ActionParams { get; set; }

        public static ActionResult Result { get; set; }

        public static Func<ActionResult> InvokeActionCallback { get; set; }

        protected override ActionResult InvokeActionMethod(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
        {
            return InvokeActionCallback();
        }
    }
}
