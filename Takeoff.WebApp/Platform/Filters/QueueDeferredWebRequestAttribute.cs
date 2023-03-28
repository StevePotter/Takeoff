using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq.Expressions;
using System.Messaging;
using System.Web;
using System.Web.Mvc;
using MvcContrib;

namespace Takeoff
{

    /// <summary>
    /// Used by controllers that wish to send a request to the Mule background processor to perform an action.  In this way, an action 
    /// can be offloaded to increase app response time and free up web server resources.   This is helpful for things like 
    /// sending emails, working with external services, etc.
    /// 
    /// Basically you register urls for some actionmethod during action execution.  After action is over, the queues are put into a single message and sent to MSMQ.  Mule app picks them up, then executes them on an exact copy of this web app running on a separate local web server.  
    /// 
    /// Add this to all controllers and action methods thay need to create background requests.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class QueueDeferredWebRequestAttribute : ActionFilterAttribute
    {
        public static MessageQueue MessageQueue
        {
            get
            {
                if (_MessageQueue == null)
                    _MessageQueue = new MessageQueue(ConfigUtil.GetRequiredAppSetting("DeferredWebRequestQueue"));
                return _MessageQueue;
            }
        }
        private static MessageQueue _MessageQueue;

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (!ApplicationSettings.EnableDeferredRequests)
                return;

            base.OnResultExecuted(filterContext);

            var requests = (List<string>)filterContext.HttpContext.Items[DeferredWebRequestHelpers.HttpItemsKeyForDeferredRequests];
            if (requests.HasItems())
            {
                //as per https://connect.microsoft.com/VisualStudio/feedback/details/94943/messagequeue-send-crashes-if-called-by-multiple-threads, you gotta use a message object
                MessageQueue.Send(new Message(requests.ToArray()));
                filterContext.HttpContext.Items.Remove(DeferredWebRequestHelpers.HttpItemsKeyForDeferredRequests);
            }
        }
    }

    /// <summary>
    /// Defines a controller or action that can be executed by a request from the Mule app.  Verification is done through a special http header.
    /// 
    /// NOTE: mule does not pass the [AuthorizeBetter] filter right now so if you use MuleOnly on a controller, make sure there is no Authorize attribute on the controller class.
    /// </summary>
    public class DeferredRequestOnly : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsDeferredRequest())
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

    }

    /// <summary>
    /// Returns unauthorized if the request wasn't local or from an authorized mule.
    /// </summary>
    public class DeferredRequestOrLocalOnly : DeferredRequestOnly
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsDeferredRequest() && !filterContext.HttpContext.Request.IsLocal)
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

    }

}