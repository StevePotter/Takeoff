using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using Takeoff.Controllers;
using Takeoff.Data;

namespace Takeoff.WebApp.Ripper
{
    /// <summary>
    /// ActionInvoker for test rig.
    /// </summary>
    public class ViewRenderingActionInvoker : ControllerActionInvokerWithExcludeFilter
    {
        public ViewRenderingActionInvoker()
        {

        }

        public static string ViewName { get; set;  }
        public static string ControllerName { get; set; }
        public static string ActionName { get; set; }
        public static IUser User { get; set; }
        public static object Model { get; set; }
        
        public static ActionResult Result { get; set; }


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
            //todo: if there is an action name, check for existence
            //use Ensure in case an action set the value already, which does happen
            if (ActionName.HasChars())
                controllerContext.Controller.ViewData.Ensure("ActionName", () => ActionName);
            if (ControllerName.HasChars())
                controllerContext.Controller.ViewData.Ensure("ControllerName", () => ControllerName);

            if (User != null)
            {
                controllerContext.Controller.ViewData.Ensure("User", () => User);
                controllerContext.Controller.ViewData.Ensure("Identity", () => new UserIdentity());
            }
            if (Model != null)
                controllerContext.Controller.ViewData.Model = Model;
            Result = new ViewResult { ViewName = ViewName.CharsOr(ActionName), ViewData = controllerContext.Controller.ViewData };
            Result.ExecuteResult(controllerContext);
            return true;

            // notify controller that no method matched
            return false;
        }


        protected override ActionResult InvokeActionMethod(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
        {

            //use Ensure in case an action set the value already, which does happen
            if ( ActionName.HasChars())
                controllerContext.Controller.ViewData.Ensure("ActionName", () => ActionName);
            if (ControllerName.HasChars())
                controllerContext.Controller.ViewData.Ensure("ControllerName", () => ControllerName);
            
            if (Model != null)
                controllerContext.Controller.ViewData.Model = Model;
            Result = new ViewResult { ViewName = ViewName.CharsOr(ActionName), ViewData = controllerContext.Controller.ViewData };

            return Result;
        }
    }
}
