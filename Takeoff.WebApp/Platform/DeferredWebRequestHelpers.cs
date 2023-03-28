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

    public static class DeferredWebRequestHelpers
    {
        public const string HttpItemsKeyForDeferredRequests = "__deferredWebRequests";

        public readonly static string UrlPrefix = ConfigUtil.GetRequiredAppSetting("DeferredWebRequestUrlPrefix").EndWithout(@"/");

        const string DeferredRequestHttpHeaderName = "x-deferredrequest";//so web app knows it's from deferred...otherwise certain action methods can't be executed. 

        /// <summary>
        /// Adds a request to perform the action asynchronously.  This requires a UrlHelper on the controller, so make sure controller.Url is set to an instance.
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        /// <param name="expression"></param>
        public static bool DeferRequest<TController>(this Controller controller, Expression<Action<TController>> expression) where TController : Controller
        {
            DeferRequest(controller, controller.Url.Action<TController>(expression));
            return true;
        }


        /// <summary>
        /// Adds a new request to the list of urls that Mule will run outside of the web server.
        /// </summary>
        /// <param name="relativeUrl"></param>
        public static void DeferRequest(this Controller controller, string relativeUrl)
        {
            controller.HttpContext.DeferRequest(relativeUrl);
        }

        public static void DeferRequest(this HttpContextBase context, string relativeUrl)
        {
            var absoluteUrl = UrlPrefix + relativeUrl.StartWith(@"/");
            CurrentRequests(context).Add(absoluteUrl);
        }


        private static List<string> CurrentRequests(HttpContextBase context)
        {
            var items = context.Items;
            var requests = items[HttpItemsKeyForDeferredRequests];
            if (requests == null)
                requests = items[HttpItemsKeyForDeferredRequests] = new List<string>();

            return (List<string>)requests;
        }


        public static bool IsDeferredRequest(this HttpRequestBase request)
        {
            return IsDeferredRequest(request.Headers);
        }

        public static bool IsDeferredRequest(this HttpRequest request)
        {
            return IsDeferredRequest(request.Headers);
        }

        private static bool IsDeferredRequest(NameValueCollection requestHeaders)
        {
            var header = requestHeaders[DeferredRequestHttpHeaderName];//look out for exceptions that could occur during unit testing or something
            if (header.HasChars())
            {
                var keyHashed = header.Split(':');
                var key = keyHashed[0];
                var hashed = keyHashed[1];
                if (key.Hash(ConfigurationManager.AppSettings["DeferredRequestSecretKey"], "HMACSHA1").Equals(hashed))
                {
                    return true;
                }
            }
            return false;
        }
    }

}