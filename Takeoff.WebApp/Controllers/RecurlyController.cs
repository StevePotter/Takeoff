using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Amazon.S3.Model;
using Amazon.SimpleDB.Model;
using Amazon.SimpleEmail.Model;
using Mediascend.Web;
using System.Net.Mail;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using Takeoff.Data;
using Takeoff.Models;
using System.Diagnostics;
using System.Web.Script.Serialization;
using Takeoff.Models.Data;
using Takeoff.Transcoder;
using Takeoff.ViewModels;
using TweetSharp;
using PostmarkDotNet;

namespace Takeoff.Controllers
{
    /// <summary>
    /// Endoing for recurly webhooks.
    /// </summary>
    [LogRequest(true, true)]
    [ValidateInput(false)]
    [VerifyRecurlyWebhook]
    public class RecurlyController : BasicController
    {
        //link is /recurly, but all are posts so they map to Create action

        //http://docs.recurly.com/configuration/push-notifications
        public ActionResult Create(XDocument model)
        {
            string type = model.Root.Name.ToString();
            switch (type)
            {
                case "renewed_subscription_notification":
                    ProcessRenewedSubscription(model);
                    LogWebHook(model, true);
                    break;
                case "failed_payment_notification":
                    ProcessFailedPayment(model);
                    LogWebHook(model, true);
                    break;
                case "successful_payment_notification":
                    ProcessSuccessfulPayment(model);
                    LogWebHook(model, true);
                    break;
                case "reactivated_account_notification":
                    ProcessReactivatedAccount(model);
                    LogWebHook(model, true);
                    break;
                case "canceled_subscription_notification":
                    ProcessCancelledSubscription(model);
                    LogWebHook(model, true);
                    break;
                default:
                    LogWebHook(model, false);
                    break;

            }
            return this.Content("Word up Recurly!");
        }

        private AccountThing GetAccount(XDocument model, bool throwExceptionIfDoesntExist = true)
        {
            //current_period_started_at
            int accountId = model.Descendants("account_code").First().Value.ToInt();
            return throwExceptionIfDoesntExist ? Things.Get<AccountThing>(accountId) : Things.GetOrNull<AccountThing>(accountId);
        }



        private void LogWebHook(XDocument model, bool processed)
        {
            int? accountId = model.Descendants("account_code").FirstOrDefault().MapIfNotNull(e => e.Value.ToIntTry(), new int?());

            RecurlyWebHookLogs.Log(new RecurlyWebHookLogItem
                                       {
                                           AccountId = accountId,
                                           Type = model.Root.Name.ToString(),
                                           ReceivedOn = this.RequestDate(),
                                           Processed = processed,
                                           ServerRequestId = this.HttpContext.LogId(),
                                       });
        }

        private void ProcessRenewedSubscription(XDocument model)
        {
            var account = GetAccount(model);
            DateTime startsOn = DateTime.SpecifyKind(DateTime.Parse(model.Descendants("current_period_started_at").First().Value), DateTimeKind.Utc);
            account.TrackChanges();
            account.CurrentBillingPeriodStartedOn = startsOn;
            account.Update();
            this.LogBusinessEvent("SubscriptionRenewed", accountId: account.Id);

        }

        private void ProcessFailedPayment(XDocument model)
        {
            var account = GetAccount(model, false);
            if (account == null || account.Status == AccountStatus.Pastdue)
                return;
            account.TrackChanges();
            account.Status = AccountStatus.Pastdue;
            account.Update();
            this.LogBusinessEvent("PaymentFailed", new
            {
                AccountId = account.Id,
            }, sendEmail: true);

        }


        private void ProcessSuccessfulPayment(XDocument model)
        {
            var account = GetAccount(model);
            if (account.Status == AccountStatus.Subscribed)
                return;
            account.TrackChanges();
            account.Status = AccountStatus.Subscribed;
            account.Update();
            this.LogBusinessEvent("PaymentSucceeded", new
            {
                AccountId = account.Id,
            });
        }

        /// <summary>
        /// Account was reactivated after being cancelled.
        /// </summary>
        /// <param name="model"></param>
        private void ProcessReactivatedAccount(XDocument model)
        {
            var account = GetAccount(model);
            if (account.Status == AccountStatus.Subscribed)
                return;
            account.TrackChanges();
            account.Status = AccountStatus.Subscribed;
            account.Update();
        }

        private void ProcessCancelledSubscription(XDocument model)
        {
            var account = GetAccount(model, false);
            if (account == null || account.Status == AccountStatus.ExpiredNonpayment)
                return;
            account.TrackChanges();
            account.Status = AccountStatus.ExpiredNonpayment;
            account.Update();
        }
        
    }

    public class VerifyRecurlyWebhook : ActionFilterAttribute
    {
        private readonly static string RecurlyWebhookAuthUsername = ConfigUtil.GetRequiredAppSetting("RecurlyWebhookAuthUsername");
        private readonly static string RecurlyWebhookAuthPassword = ConfigUtil.GetRequiredAppSetting("RecurlyWebhookAuthPassword");

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //basic authentication is explained here: http://en.wikipedia.org/wiki/Basic_access_authentication
            var authorizationHeaderValue = filterContext.RequestContext.HttpContext.Request.Headers["Authorization"].CharsOrEmpty();//charsorempty stuff just makes it easier to avoid null reference exceptions which would complicate logic to return a 401
            //header looks like "Basic sadfasdf3443=".  So trim off the basic
            authorizationHeaderValue = authorizationHeaderValue.After("Basic").CharsOrEmpty().Trim();
            var matchWith = "{0}:{1}".FormatString(RecurlyWebhookAuthUsername, RecurlyWebhookAuthPassword).EncodeBase64();
            if (!matchWith.Equals(authorizationHeaderValue) && ConfigUtil.GetRequiredAppSetting<bool>("RecurlyWebhookEnableAuthorization"))
            {
                filterContext.Result = new Http401NoLoginPageResult();
            }
        }
    }



}
