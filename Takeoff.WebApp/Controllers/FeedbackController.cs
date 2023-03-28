using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Takeoff.Models;

namespace Takeoff.Controllers
{
    public class FeedbackController : Controller
    {


        [HttpPost]
        [HttpParameterValue("type", "AccountClosed-Subscriber", false)]
        public ActionResult Create(FeedbackForAccountClosed model)
        {
            //var to = ConfigUtil.AppSetting("SendBusinessEventEmailsTo");

            //var message = new OutgoingMessage
            //{
            //    To = to,
            //    Subject = "Biz Event " + bizEventInsertParams.Type,
            //    Template = "BusinessEvent",
            //};

            return this.Success(() => this.View("Success"), null);
        }
    
    }

    public class FeedbackForAccountClosed
    {
        public int? UserId { get; set; }
        public string Body { get; set; }
        public bool AllowContact { get; set; }
    }
}
