using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Messaging;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Takeoff
{
    /// <summary>
    /// Takes all the method invokes that have been deferred during a web requests and sends them to a MSMQ, where they are picked up 
    /// by a job and invoked.  
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class QueueDeferredMethodInvokesAttribute : ActionFilterAttribute
    {

        public static MessageQueue MessageQueue
        {
            get
            {
                if (_MessageQueue == null)
                    _MessageQueue = new MessageQueue(ConfigUtil.GetRequiredAppSetting("DeferredInvokesQueue"));
                return _MessageQueue;
            }
        }
        private static MessageQueue _MessageQueue;

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);

            var invokes = (List<Expression<Action>>)filterContext.HttpContext.Items[DeferredMethodInvokerHelper.HttpItemsKeyForDeferredInvokes];
            if (invokes.HasItems())
            {
                SendMessagesToInvoke(invokes);
                filterContext.HttpContext.Items.Remove(DeferredMethodInvokerHelper.HttpItemsKeyForDeferredInvokes);
            }
        }

        public static void SendMessagesToInvoke(IEnumerable<Expression<Action>> invokes)
        {
            var serializedInvokes = invokes.Select(e => JsonConvert.SerializeObject(DeferredMethodInvoking.CreateMethodInvokeParameters(e))).ToArray
                    ();
            //as per https://connect.microsoft.com/VisualStudio/feedback/details/94943/messagequeue-send-crashes-if-called-by-multiple-threads, you gotta use a message object
            MessageQueue.Send(new Message(serializedInvokes));
        }
    }
}