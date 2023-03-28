using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Takeoff
{
    public static class DeferredMethodInvokerHelper
    {
        public const string HttpItemsKeyForDeferredInvokes = "__deferredInvokes";


        /// <summary>
        /// Invokes the expression in a windows service by serializing the params in a message queue.  Invoke will happen sometime soon but it's not known exactly when.  This is nice for things like logging that don't need to happen right away.
        /// </summary>
        public static void Defer(this Controller controller, Expression<Action> action)
        {
            controller.HttpContext.Defer(action);
        }

        /// <summary>
        /// Invokes the expression in a windows service by serializing the params in a message queue.  Invoke will happen sometime soon but it's not known exactly when.  This is nice for things like logging that don't need to happen right away.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="action"></param>
        public static void Defer(this HttpContextBase context, Expression<Action> action)
        {
            CurrentDeferredInvokes(context).Add(action);
        }


        private static List<Expression<Action>> CurrentDeferredInvokes(HttpContextBase context)
        {
            var items = context.Items;
            var requests = items[HttpItemsKeyForDeferredInvokes];
            if (requests == null)
                requests = items[HttpItemsKeyForDeferredInvokes] = new List<Expression<Action>>();

            return (List<Expression<Action>>)requests;
        }

    }

}