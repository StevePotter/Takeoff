using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Resources;
using MvcContrib;
using Takeoff.ViewModels;

namespace Takeoff.Controllers
{
    /// <summary>
    /// Recieves a post from cloudmailin to process incoming email.  This 
    /// </summary>
    public class IncomingMailController : BasicController
    {
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Create(IncomingMail email)
        {
            var addr = new IncomingMailAction
                            {
                                Action = "verify",
                                Data = "something",
                                UserId = "69",
                            }.GetEmailAddress();

            if (!IncomingMailUtil.IsValidRequest(Request))
            {
                return new NonHtmlResponse
                {
                    Data = new
                    {
                        ErrorCode = ErrorCodes.InvalidInput,
                        ErrorDescription = "Bad Signature",
                    },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            if (email.Disposable.HasChars())
            {
                IncomingMailAction action = IncomingMailAction.Parse(email.Disposable);
            }
            //http://docs.cloudmailin.com/http_status_codes
            return new HttpStatusCodeResult(200);
        }

        


    }
}
