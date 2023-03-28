using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Takeoff.Data;
using StackExchange.Profiling;

namespace Takeoff.Models
{
    public static class ExtensionMethods
    {

        #region DateTime

        /// <summary>
        /// Returns the number of milliseconds since 1/1/1970 UTC.  Use this to serialize json objects and create Date objects in JavaScript.  If Kind is unknown, we assume it's UTC.
        /// </summary>
        /// <param name="utc"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is similar to what asp.net ajax does except for two things:
        /// 1.  It doesn't try and add the funny Date constructor (which don't work with jquery)
        /// 2.  It doesn't call ToUniversalTime on the date.  This is CRUCIAL because the dates are already converted to UTC when inserted into DB, and a second call will screw them up.
        /// </remarks>
        public static long ForJavascript(this DateTime utc)
        {
            if (utc.Kind == DateTimeKind.Local)
                throw new Exception("Should be specified as UTC when set.");
            return (long)(utc - jsUtc).TotalMilliseconds;
        }
        static DateTime jsUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromForJavascript(this long milliseconds)
        {
            return jsUtc.AddMilliseconds(milliseconds);
        }


        #endregion


        public static string HtmlEncode(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return System.Web.HttpUtility.HtmlEncode(value);
        }
    }

    public static class HttpContextFactory
    {
        private static HttpContextBase _mockHttpContext;

        /// <summary>
        /// Access the HttpContext using the Abstractions.
        /// </summary>
        public static HttpContextBase Current
        {
            get
            {
                var curr = HttpContext.Current;
                if (_mockHttpContext == null && curr != null)
                {
                    return new HttpContextWrapper(curr);
                }
                return _mockHttpContext;
            }

            set
            {
                _mockHttpContext = value;
            }
        }
    }


    public class HttpContextThreadCache: IThreadCacheService
    {

        public System.Collections.IDictionary Items
        {
            get
            {
                return HttpContextFactory.Current.Items;
            }
        }
    }
}
