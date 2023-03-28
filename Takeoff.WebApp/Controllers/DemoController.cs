using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Resources;
using MvcContrib;

namespace Takeoff.Controllers
{
    public class DemoController : BasicController
    {
        [HttpGet]
        public ActionResult Index()
        {
            if (this.IsLoggedIn())
                return View("AlreadyLoggedIn", this.UserThing2());
            return View();
        }

        [HttpPost]
        public ActionResult Create(int? timezoneOffset)
        {
            //an accident
            if (this.IsLoggedIn())
            {
                return this.RedirectToAction<DashboardController>(c => c.Index(null, null), UrlType.RelativeHttps);
            }

            var date = this.HttpContext.RequestDate();

            var user = Users.Signup(null, "You", null, null, timezoneOffset.GetValueOrDefault(), new { Type = "Demo" }, date);
            this.IdentityService().SetIdentity(new UserIdentity
            {
                UserId = user.Id
            }, IdentityPeristance.TemporaryCookie, HttpContext);

            //note this is also done in SettingsController.CreateAccount
            var account = Accounts.Create(user, TakeoffRoles.Owner, date, "demo", AccountStatus.Demo);

            this.LogBusinessEvent("DemoCreate");

            return this.RedirectToAction(c => c.Sample());
        }


        /// <summary>
        /// Gives the person the option to create a sample production or not after they signed up for the demo.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Sample()
        {
            if (!this.IsLoggedIn())
            {
                return this.RedirectToAction(c => c.Index());
            }

            return View();
        }


        [HttpPost]
        [ActionName("Sample")]
        public ActionResult SamplePost()
        {
            if (!this.IsLoggedIn())
            {
                return this.RedirectToAction(c => c.Index());
            }

            var user = this.UserThing2();
            var account = user.Account;

            //create a sample production but only if one hasn't been created...which avoids double submits
            int sampleId = this.Repository<IProductionsRepository>().GetSampleProductionId(account.Id);
            if (!sampleId.IsPositive())
                sampleId = Productions.CreateSampleProduction(user, account, SampleProduction.ClientName, SampleProduction.V1Comments, SampleProduction.V1FileName, SampleProduction.V1MobileFileName, SampleProduction.V2Comments, SampleProduction.V2FileName, SampleProduction.V2MobileFileName, SampleProduction.VideoLocation).Id;

            return this.RedirectToAction<ProductionsController>(o => o.Details(sampleId, null, null, null, null));
        }

    }
}
