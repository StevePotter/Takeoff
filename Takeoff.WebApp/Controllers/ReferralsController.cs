using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mediascend.Web;
using Takeoff.Data;
using Takeoff.Models;
using Recurly;
using MvcContrib;
using Takeoff.ViewModels;

namespace Takeoff.Controllers
{

    [TrialSignupRequired]
//note: this can be put back but we would have to provide some way to store credits, then upon signing up, transfer those credits over    [Trial2Forbidden]
    [RestrictIdentityAttribute(AllowDemo = false, RequireAccount = true)]
    public class ReferralsController : BasicController
    {

        public ActionResult Index()
        {
            var user = this.UserThing();
            var account = user.Account;
            return View();
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }


    }
}
