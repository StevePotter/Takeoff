using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Takeoff
{
    /// <summary>
    /// A repository for app settings.
    /// </summary>
    public static class ApplicationSettings
    {
        static ApplicationSettings()
        {
            //basically a dump of all the potential urls we might want to use
            const string forbidden = "home,about,mobile,customers,help,signin,extras,extra,news,new,announcement,contact,legal,default,features,feature,file,files,about,index,gallery,pricing,useragreement,whyverify,privacy,nocookies,sendfiletosupport,tour,login," +
                                     "logout,passwordreset,root,productionfiles,membershiprequests,demo,accountmemberships,productionvideos,invoices,productionmembers," +
                                     "support,staff,productions,production,comment,comments,productioncomments,dashboard,prompts,error,account,uploadtests,account,accounts" +
                                     "jobs,job,encoding,transcoding,log,email,accountmembers,signup,video,videos,user,users,wedding,weddings,movie,movies";

            ForbiddenVanityUrls = new HashSet<string>(forbidden.Split(',').Select(f => f.Trim()), StringComparer.OrdinalIgnoreCase);
        }

        public static HashSet<string> ForbiddenVanityUrls { get; private set; }


        /// <summary>
        /// Indictes whether the razor precompiled viewengine is used.  That engine is great for faster warmup in production, as well as rendering views without a web server.  It is not good during development.
        /// </summary>
        public static bool UsePrecompiledViewEngine
        {
            get
            {
                if (!_UsePrecompiledViewEngine.HasValue)
                {
                    _UsePrecompiledViewEngine =
                        Convert.ToBoolean(ConfigurationManager.AppSettings["UsePrecompiledViewEngine"].CharsOr("true"));
                }
                return _UsePrecompiledViewEngine.GetValueOrDefault();
            }
            set
            {
                _UsePrecompiledViewEngine = value;
            }
        }
        private static bool? _UsePrecompiledViewEngine;


        public static bool LoginWithAnyPassword
        {
            get
            {
                if (!_LoginWithAnyPassword.HasValue)
                {
                    _LoginWithAnyPassword =
                        Convert.ToBoolean(ConfigurationManager.AppSettings["LoginWithAnyPassword"].CharsOr("true"));
                }
                return _LoginWithAnyPassword.GetValueOrDefault();
            }
            set
            {
                _LoginWithAnyPassword = value;
            }
        }
        private static bool? _LoginWithAnyPassword;


        public static bool EnableAjaxPolling
        {
            get
            {
                if (!_EnableAjaxPolling.HasValue)
                {
                    _EnableAjaxPolling = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableAjaxPolling"].CharsOr("true"));
                }
                return _EnableAjaxPolling.GetValueOrDefault();
            }
            set
            {
                _EnableAjaxPolling = value;
            }
        }
        private static bool? _EnableAjaxPolling;


        public static bool EnableGoogleAnalytics
        {
            get
            {
                if (!_EnableGoogleAnalytics.HasValue)
                {
                    _EnableGoogleAnalytics = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableGoogleAnalytics"].CharsOr("true"));
                }
                return _EnableGoogleAnalytics.GetValueOrDefault();
            }
            set
            {
                _EnableGoogleAnalytics = value;
            }
        }
        private static bool? _EnableGoogleAnalytics;

        
        /// <summary>
        /// Indicates whether calls to mule via msmq are enabled.   Default is true.
        /// </summary>
        public static bool EnableDeferredRequests
        {
            get
            {
                if (!_EnableDeferredRequests.HasValue)
                {
                    _EnableDeferredRequests =
                        Convert.ToBoolean(ConfigurationManager.AppSettings["EnableDeferredRequests"].CharsOr("true"));
                }
                return _EnableDeferredRequests.Value;
            }
            set
            {
                _EnableDeferredRequests = value;
            }
        }

        private static bool? _EnableDeferredRequests;



        public const string QuickAccessFeatureName = "QuickAccess";
        public static string GuestAccessPasswordIV = ConfigurationManager.AppSettings["ProductionQuickAccessPasswordIV"];
        public static string GuestAccessPasswordEncryptionKey = ConfigurationManager.AppSettings["ProductionQuickAccessPasswordEncryptionKey"];

        public static string FilePickerApiKey = ConfigurationManager.AppSettings["FilePickerApiKey"];

    
    }


}