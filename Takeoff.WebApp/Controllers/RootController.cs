using System;
using System.Net;
using System.Web.Mvc;

using Mediascend.Web;
using MvcContrib;
using System.Linq;
using Takeoff.Models;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Takeoff.Resources;
using Takeoff.ViewModels;
namespace Takeoff.Controllers
{
    [RootUrl]
    [ExcludeFilter(typeof(CheckSuspendedAccountAttribute))]
    public class RootController : BasicController
    {

        /// <summary>
        /// The default page for the application
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(StartupMessage model)
        {
            //if the user came to the site for a new session and had their password saved in a cookie, we can expect that they'll want to go straight to their dashboard.  however, if they click on the app home page after being logged in, we let them go there
            if (this.IsLoggedIn() && (Request.UrlReferrer == null || (!Request.UrlReferrer.AbsoluteUri.StartsWith(ApplicationEx.AppUrlPrefix) && (!ApplicationEx.EnableHttps || !Request.UrlReferrer.AbsoluteUri.StartsWith(ApplicationEx.AppUrlPrefixSecure)))))
            {
                return this.RedirectToAction <DashboardController>(c => c.Index(null, null), UrlType.RelativeHttps);
            }

            //this is for the login portion of the page
            ViewData["redirectUrl"] = Url.Action<DashboardController>(c => c.Index(null, null), UrlType.RelativeHttps);
            ViewData["useAjaxPostForLogin"] = ApplicationEx.UseAjaxPostForLogin(Request);
            return View(model);
        }


        public ActionResult Gallery(string id)
        {
            ViewData["initialTab"] = id.HasChars() ? id.ToLowerInvariant() : "signup";
            return View();
        }

        public ActionResult Pricing()
        {
            var plans = this.Repository<Takeoff.Data.IPlansRepository>().GetPlansForSale().Select(p => new PlanForSale(p)).ToArray();
            return  this.View(plans);
        }

        public ActionResult UserAgreement()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult WhyVerify()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        public ActionResult Try()
        {
            return View();
        }

        public ActionResult ThanksTo()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        /// <summary>
        /// Users are redirected here when they don't have cookies installed.
        /// </summary>
        /// <returns></returns>
        [ExcludeFilter(typeof(CheckCookiesAttribute))]
        public ActionResult NoCookies()
        {
            ViewData["CheckForCookies"] = null;//avoid an infinite redirect loop

            return View();
        }

        public ActionResult SendFileToSupport()
        {
            return View();
        }


    }
}
