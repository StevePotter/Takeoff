using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web;
using Takeoff;

namespace Mediascend.Web
{

    public static class ApplicationEx
    {

        /// <summary>
        /// The application's name.  This should coorespond to an application in our web database.
        /// </summary>
        public static string AppName
        {
            get
            {
                return _appName.Value;
            }
        }
        private static Lazy<string> _appName = new Lazy<string>(() =>
        {
            return ConfigUtil.GetRequiredAppSetting("AppName");
        });

        public static string AppUrlPrefix
        {
            get
            {
                return _appUrlPrefix.Value;
            }
        }
        private static Lazy<string> _appUrlPrefix = new Lazy<string>(() =>
        {
            return ConfigurationManager.AppSettings["AppUrlPrefix"];
        });


        /// <summary>
        /// The https prefix for this app.  If empty, https will not be supported.
        /// </summary>
        public static string AppUrlPrefixSecure
        {
            get
            {
                return _appUrlPrefixSecure.Value;
            }
        }
        private static Lazy<string> _appUrlPrefixSecure = new Lazy<string>(() =>
        {
            return ConfigurationManager.AppSettings["AppUrlPrefixSecure"];
        });


        

        public static bool EnableHttps
        {
            get
            {
                return AppUrlPrefixSecure.HasChars();
            }
        }

        /// <summary>
        /// When true, it's okay to use ajax form submit on the login form post (no cross domain shit, including http to https).
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool UseAjaxPostForLogin(HttpRequestBase request)
        {
            return !ApplicationEx.EnableHttps || request.IsSecureConnection;
        }

    }
}
