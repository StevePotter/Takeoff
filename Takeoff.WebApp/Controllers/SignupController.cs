using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Mediascend.Web;
using MvcContrib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Recurly;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Models.Data;
using Takeoff.Resources;
using Takeoff.ViewModels;

namespace Takeoff.Controllers
{

    public enum SignupType
    {
        StartTrial,
        Guest,
        TrialUserInfoRequested,
        DirectPurchase
    }

    /// <summary>
    /// Handles everything involved in signing up for a new account.
    /// </summary>
    public class SignupController : BasicController
    {
        private class PreventAlreadySignedUpUser : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                var user = filterContext.HttpContext.UserThing2();
                if (user != null)//users can't sign up twice, duh.  semi anonymous should be able to though
                {
                    filterContext.Result = new BadRequest
                    {
                        ErrorCode = ErrorCodes.UserCannotSignupBecauseTheyAreLoggedIn,
                        ErrorDescription = ErrorCodes.UserCannotSignupBecauseTheyAreLoggedInDescription,
                        ViewName = "AlreadySignedUp",
                    };
                }
            }
        }

        private ActionResult UserCannotSignupBecauseTheyAreLoggedIn()
        {
            return new BadRequest
            {
                ErrorCode = ErrorCodes.UserCannotSignupBecauseTheyAreLoggedIn,
                ErrorDescription = ErrorCodes.UserCannotSignupBecauseTheyAreLoggedInDescription,
                ViewName = "AlreadySignedUp",
            };
        }

        private bool UserHasNecessaryInfoForSubscription(IUser user)
        {
            return user.Email.HasChars();
        }

        /// <summary>
        /// Begins the signup process.  In this step, they select the plan they want to go with.
        /// </summary>
        /// <returns></returns>
        /// <param name="type"> </param>
        /// <param name="returnUrl"> </param>
        /// <param name="planId"> </param>
        /// <param name="postSignupUrl">The orignal url to redirect back to when the signup is complete.</param>
        /// <remarks>
        /// Client will be in 1 of these scenarions:
        /// - They are signed in under a demo account
        /// - They are brand new and clicked Signup
        /// - They were a guest (had a user but no account) and want to create an account
        /// </remarks>
        [HttpGet]
        public ActionResult Index(SignupType? type, string returnUrl, string planId)
        {
            var user = this.UserThing2();
            //if they came from the dashboard or wherever, use the referring url to go back to when we're done.  url must come from same host or else if they hit a /signup link from an email or something, it could take them back to google or wherever
            if (!returnUrl.HasChars() && Request.UrlReferrer != null && Request.Url != null && Request.Url.Host.EqualsCaseInsensitive(Request.UrlReferrer.Host))
                returnUrl = Request.UrlReferrer.OriginalString;

            if (!type.HasValue)
            {
                //do our best to automatically determine the signup type
                if (user != null)
                {
                    var account = user.Account;
                    if (account != null)
                    {
                        if (account.Status == AccountStatus.TrialAnonymous)
                        {
                            return View("Index-TrialUserInfoRequested", new Signup_Trial
                            {
                                ReturnUrl = returnUrl
                            });
                        }
                        else if ( account.Status == AccountStatus.Trial2)
                        {
                            return View("Subscribe-ChoosePlan");
                        }
                    }
                    else if (UserHasNecessaryInfoForSubscription(user))
                    {
                        return View("Subscribe-ChoosePlan");                        
                    }
                }
                if (user != null)
                    return UserCannotSignupBecauseTheyAreLoggedIn();
                return View("Index");
            }

            switch (type.Value)
            {
                case SignupType.Guest:
                    if (user != null)
                        return UserCannotSignupBecauseTheyAreLoggedIn();
                    return View("Guest");
                case SignupType.DirectPurchase:
                    if (user != null)
                        return UserCannotSignupBecauseTheyAreLoggedIn();
                    if (planId.HasChars())
                        return View("DirectPurchase", new Signup_DirectPurchase
                                                          {
                                                              PlanId = planId,                                                              
                                                          });
                    return View("DirectPurchase-ChoosePlan");
                case SignupType.TrialUserInfoRequested:
                    if (user == null)
                        return this.Login();
                    return View("Index-TrialUserInfoRequested", new Signup_Trial
                    {
                        ReturnUrl = returnUrl
                    });
                case SignupType.StartTrial:
                    if (user != null)
                        return UserCannotSignupBecauseTheyAreLoggedIn();
                    return View("Index");
                default:
                    return new BadRequest();
            }
        }

        /// <summary>
        /// Guest signup.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpParameterValue("type", "Guest", false)]
        [HttpPost]
        [PreventAlreadySignedUpUser]
        public ActionResult Create(Signup_Guest model)
        {
            //semi-anonymous users should be logged out at this point
            this.IdentityService().ClearIdentity(HttpContext);

            const string view = "Guest";

            var userRepository = this.Repository<IUsersRepository>();
            if (model.Email.HasChars())
            {
                switch (userRepository.GetExistanceByEmail(model.Email))
                {
                    case EntityExistance.Active:
                        ModelState.AddModelError(string.Empty, S.Signup_Account_EmailIsActive);
                        break;
                    case EntityExistance.Deleted:
                        ModelState.AddModelError(string.Empty, S.Signup_Account_EmailExistedButWasDeleted);
                        break;
                }
            }

            if (!ModelState.IsValid)
            {
                return this.Invalid(() => this.View(view));
            }

            var user = Users.Signup(model.Email, null, model.FirstName, model.LastName, model.TimezoneOffset, null, this.RequestDate(), model.Password);
            //log them in
            this.IdentityService().SetIdentity(new UserIdentity
            {
                UserId = user.Id
            }, IdentityPeristance.TemporaryCookie, HttpContext);

            this.LogBusinessEvent("GuestSignup", new
            {
            }, sendEmail: true);

            this.DeferRequest(Url.Action<EmailController>(c => c.SendUserSignup(user.Id)));

            return this.Success(() => View("Guest-Success"), null);
        }

        [HttpParameterValue("type", "StartTrial", false)]
        [HttpPost]
        [PreventAlreadySignedUpUser]
        public ActionResult Create(int? timezoneOffset)
        {
            var date = this.HttpContext.RequestDate();

            var user = Users.Signup(null, "You", null, null, timezoneOffset.GetValueOrDefault(), new { Type = "Trial" }, date);
            this.IdentityService().SetIdentity(new UserIdentity
            {
                UserId = user.Id
            }, IdentityPeristance.TemporaryCookie, HttpContext);

            //note this is also done in SettingsController.CreateAccount
            var account = Accounts.Create(user, TakeoffRoles.Owner, date, PlanIds.TrialAnonymous, AccountStatus.TrialAnonymous);

            this.LogBusinessEvent("TrialCreate", new
            {
                IsAnonymous = true,
            }, sendEmail: true);

            return this.Success(() => this.RedirectToAction(c => c.Sample()), new
            {
                UserId = user.Id,
            });

        }

        [HttpParameterValue("type", "TrialUserInfoRequested", false)]
        [HttpPost]
        [RestrictIdentity]
        public ActionResult Create(Signup_Trial model)
        {
            var user = this.UserThing2();
            var account = user.MapIfNotNull(u => u.Account);
            if (user == null || account == null || account.Status != AccountStatus.TrialAnonymous)
            {
                return new BadRequest
                {
                    ErrorCode = ErrorCodes.UserCannotSignupBecauseTheyAlreadyDid,
                    ErrorDescription = ErrorCodes.UserCannotSignupBecauseTheyAlreadyDidDescription,
                    ViewName = "LinkDoesNotApply",
                };
            }

            if (!ModelState.IsValid)
                return this.View("Index-TrialUserInfoRequested", model);
            var userRepository = this.Repository<IUsersRepository>();
            switch (userRepository.GetExistanceByEmail(model.Email))
            {
                case EntityExistance.Active:
                    ModelState.AddModelError(string.Empty, S.Signup_Account_EmailIsActive);
                    return this.View("Index-TrialUserInfoRequested", model);
                case EntityExistance.Deleted:
                    ModelState.AddModelError(string.Empty, S.Signup_Account_EmailExistedButWasDeleted);
                    return this.View("Index-TrialUserInfoRequested", model);
            }

            userRepository.BeginUpdate(user);
            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UpdateDisplayName();
            user.UpdatePassword(model.Password);
            model.TimezoneOffset.IfHasVal(v => user.TimezoneOffset = v);
            userRepository.Update(user);

            var accountRepository = this.Repository<IAccountsRepository>();
            accountRepository.BeginUpdate(account);
            account.Status = AccountStatus.Trial2;
            account.PlanId = PlanIds.Trial;
            accountRepository.Update(account);

            this.LogBusinessEvent("TrialSignup", new
            {
                WasAnonymous = true,
            }, sendEmail: true);

            this.DeferRequest(Url.Action<EmailController>(c => c.SendUserSignup(user.Id)));

            //send them to dashboard without a specific url set
            return this.Success(() => View("Index-TrialUserInfo-Success", new Signup_Trial_Success
            {
                ReturnUrl = model.ReturnUrl.CharsOr(() => Url.Action<DashboardController>(c => c.Index(null, null))),
            }), null);
        }

        [HttpParameterValue("type", "DirectPurchase", false)]
        [HttpPost]
        public ActionResult Create(Signup_DirectPurchase model)
        {
            //semi-anonymous users should be logged out at this point
            this.IdentityService().ClearIdentity(HttpContext);

            var view = "DirectPurchase";
            if (!ModelState.IsValid)
                return this.View(view, model);
            var userRepository = this.Repository<IUsersRepository>();
            switch (userRepository.GetExistanceByEmail(model.Email))
            {
                case EntityExistance.Active:
                    ModelState.AddModelError(string.Empty, S.Signup_Account_EmailIsActive);
                    return this.View(view, model);
                case EntityExistance.Deleted:
                    ModelState.AddModelError(string.Empty, S.Signup_Account_EmailExistedButWasDeleted);
                    return this.View(view, model);
            }

            //to keep things simple and to avoid a situation where an account has no plan, and to avoid a massive page with plan, user info, and billing info (which is 
            var user = Users.Signup(model.Email, null, model.FirstName, model.LastName, model.TimezoneOffset, null, this.RequestDate(), model.Password);
            //log them in
            this.IdentityService().SetIdentity(new UserIdentity
            {
                UserId = user.Id
            }, IdentityPeristance.TemporaryCookie, HttpContext);

            //give them trial temporarily. 
            var account = Accounts.Create(user, TakeoffRoles.Owner, this.RequestDate(), PlanIds.Trial, AccountStatus.Trial2);

            this.LogBusinessEvent("DirectPurchaseSignupStep1", new
            {
                WasAnonymous = true,
            }, sendEmail: true);

            //wait until they pay to send the welcome email
//            this.DeferRequest(Url.Action<EmailController>(c => c.SendUserSignup(user.Id)));

            //plan id should be present but use Solo just in case
            var plan = this.Repository<IPlansRepository>().Get(model.PlanId.CharsOr(PlanIds.Solo));
            //send them to dashboard without a specific url set
            return this.Success(() => View("Subscribe", new Signup_Subscription
            {
                Account = account,
                Plan = plan,
                PlanId = model.PlanId,
            }), null);
        }


        /// <summary>
        /// Gives the person the option to create a sample production or not after they signed up for an account.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Sample()
        {
            if (!this.IsLoggedIn())
            {
                return this.RedirectToAction(c => c.Index(null, null, null));
            }

            return View();
        }

        [HttpPost]
        [ActionName("Sample")]
        public ActionResult SamplePost()
        {
            if (!this.IsLoggedIn())
            {
                return this.RedirectToAction(c => c.Index(null, null, null));
            }

            var user = this.UserThing2();
            var account = user.Account;

            //create a sample production but only if one hasn't been created...which avoids double submits
            int sampleId = this.Repository<IProductionsRepository>().GetSampleProductionId(account.Id);
            if (!sampleId.IsPositive())
                sampleId = Productions.CreateSampleProduction(user, account, SampleProduction.ClientName, SampleProduction.V1Comments, SampleProduction.V1FileName, SampleProduction.V1MobileFileName, SampleProduction.V2Comments, SampleProduction.V2FileName, SampleProduction.V2MobileFileName, SampleProduction.VideoLocation).Id;

            return this.RedirectToAction<ProductionsController>(o => o.Details(sampleId, null, null, null, null));
        }

        /// <summary>
        /// If they had an old account and it had a trial period ending before the trial period passed in, this returns the earlier trial.  This a nice way to prevent the following abuse: person signs up for free trial, does stuff, cancels account before trial expires (stays on as guest), and then signs up.  this would essentually allow for an unlimited trial.
        /// todo: give them a message indicating they had a previous account.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="trialEndsAt"></param>
        /// <returns></returns>
        private DateTime CheckPreviousAccountExpiration(IUser user, DateTime trialEndsAt)
        {
            var deletedAccount = Repos.Accounts.GetDeletedAccount(user.Id);
            if ( deletedAccount != null && deletedAccount.TrialPeriodEndsOn.HasValue &&
                    deletedAccount.TrialPeriodEndsOn.Value < trialEndsAt)
                {
                    trialEndsAt = deletedAccount.TrialPeriodEndsOn.Value;
                    this.AddBanner(
                        trialEndsAt.Date <= DateTime.Today
                            ? "Welcome back!  You previously had an account whose trial expired.  In order to re-activate your account, we need you to purchase Takeoff now."
                            : "Welcome back!  You previously had an account, so we will keep the same trial period as the old account.",
                        true);
                }
            return trialEndsAt;
        }


        #region Subscribe

        /// <summary>
        /// Lets a user set subscribe for the plan they just signed up for, during a trial,after a trial has expired, or if their account has been suspended due to nonpayment.
        /// </summary>
        /// <param name="postSignupUrl"></param>
        /// <param name="planId"> </param>
        /// <returns></returns>
        [HttpGet]
        [TrialSignupRequired]
        [RestrictIdentity]
        [CheckSuspendedAccount(
            StatusesToAllow = new[] {AccountStatus.ExpiredNonpayment, AccountStatus.TrialExpired, AccountStatus.Pastdue}
            )]
        public ActionResult Subscribe(string postSignupUrl, string planId)
        {
            UserThing user = this.UserThing();
            AccountThing account = user.Account;

            //handle vips and current subscribers
            if (account != null && (account.Status == AccountStatus.FreePlan || account.Status == AccountStatus.Subscribed))
            {
                return View("Signup-AlreadyDone");
            }

            IPlan plan = null;
            if (account == null || account.Status == AccountStatus.Trial2)//ignore trial plan so they can choose a new one
            {
                if ( planId.HasChars())
                    plan = this.Repository<IPlansRepository>().Get(planId);
                if (plan != null && !plan.AllowSignups)
                    plan = null;//reset it if they enter a bogus value
            }
            else
            {
                plan = account.Plan;
            }

            if (plan == null)
            {
                return View("Subscribe-ChoosePlan");
            }

            return View("Subscribe", new Signup_Subscription {PostSignupUrl = postSignupUrl, Account = account, Plan = plan, PlanId = planId});
                //need viewname because other actions render this action
        }


        [HttpPost]
        [RestrictIdentity]
        [CheckSuspendedAccount(StatusesToAllow = new[] { AccountStatus.ExpiredNonpayment, AccountStatus.TrialExpired, AccountStatus.Pastdue })]
        [ValidateJsonAntiForgeryToken]
        [ActionName("Subscribe")]
        [LogRequest(true, true)]
        [TrialSignupRequired]
        public ActionResult SubscribePost(Signup_Subscription model)
        {
            UserThing user = this.UserThing();
            AccountThing account = user.Account;

            //guest subscribed
            if ( account == null)
            {
                account = Accounts.Create(user, TakeoffRoles.Owner, this.RequestDate(), PlanIds.Trial, AccountStatus.Trial2);               
            }

            if (account.Status == AccountStatus.FreePlan || account.Status == AccountStatus.Demo || account.Status == AccountStatus.Subscribed)
            {
                return View("Signup-AlreadyDone");
            }

            IPlan plan = null;
            bool isNewPlan = false;
            if (account.Status == AccountStatus.Trial2)//ignore trial plan so they can choose a new one
            {
                isNewPlan = true;
                if (model.PlanId.HasChars())
                    plan = this.Repository<IPlansRepository>().Get(model.PlanId);
                if (plan != null && !plan.AllowSignups)
                    plan = null;//reset it if they enter a bogus value
            }
            else
            {
                plan = account.Plan;
            }

            model.Account = account;
            model.Plan = plan;

            if (plan == null)
            {
                return new BadRequest
                           {
                               ErrorDescription = "No plan was found."
                           };
            }

            AccountStatus originalStatus = account.Status;
            model.Account = account;            
            //check if the month/year combo is invalid
            if (new DateTime(model.CreditCardYear, model.CreditCardMonth, 1).AddMonths(1).Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(string.Empty, Strings.Signup_Subscribe_CardExpired_Message);
            }

            //if the model isn't valid, the errors will be shown to the user
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //give them a special error message for unknown billing crisis
            ViewData["ExceptionView"] = "Error-Billing";

            //this is slightly chatty with Recurly.  At best, it does a single signup call (for new accounts).  At its worst, it can do up to 4 (expired accounts)
            RecurlyAccount recurlyAccount = null;
            if (account.IsSubscribed())
            {
                try
                {
                    recurlyAccount = RecurlyAccount.Get(account.Id.ToInvariant());
                        //most likely it will exist already if they had subscribed in the past
                }
                catch (NotFoundException)
                {
                }
            }

            if (recurlyAccount == null)
            {
                //create the new account and store its billing info.  this does not make any calls to recurly
                recurlyAccount = new RecurlyAccount(account.Id.ToInvariant())
                                     {
                                         Email = user.Email,
                                         Username = user.DisplayName,
                                         FirstName = model.FirstName,
                                         LastName = model.LastName,
                                         BillingInfo = new RecurlyBillingInfo(account.Id.ToInvariant())
                                     };
                FillBillingInfo(model, recurlyAccount.BillingInfo);
                //subscription will create both the account and subscription in a single call.  if there is a card declined or something, modelstate will be invalid
                CreateSubscription(model, user, account, plan.Id, recurlyAccount);
            }
            else
            {
                //the account was in the system already so now we just update the billing info
                var billingInfo = new RecurlyBillingInfo(account.Id.ToInvariant());
                FillBillingInfo(model, billingInfo);
                try
                {
                    billingInfo.Update();
                }
                catch (ValidationException ex) //will happen if card is invalid
                {
                    //recurly included "Billing info." in lots of the error messages.  so we remove that crap
                    if (ex.Errors.HasItems())
                        ex.Errors.Select(e => e.Message.Strip("Billing info.").UpperFirstChar()).Each(
                            e => ModelState.AddModelError(string.Empty, e));
                    return View(model);
                }

                //find out the subscription state, if any.  if they had no subsription then start a fresh one.  if it's active, it's probably some kind of system mismatch or it was automatically activated when billing info was updated
                string subscriptionState = GetRecurlySubscriptionStatus(account);
                if (subscriptionState == null)
                {
                    CreateSubscription(model, user, account, plan.Id, recurlyAccount);
                }
                else if ("canceled".Equals(subscriptionState)) //just reactivate any cancelled subscription
                {
                    RecurlyClient.PerformRequest(RecurlyClient.HttpRequestMethod.Post,
                                                 "/accounts/" + account.Id.ToInvariant() + "/subscription/reactivate");
                }
            }

            if (ModelState.IsValid)
            {
                account.TrackChanges();
                account.Status = AccountStatus.Subscribed;
                if (!plan.Id.EqualsCaseInsensitive(account.PlanId))
                    account.PlanId = plan.Id;

                //happened during trial expiration.  needed to sync with recurly
                if (originalStatus == AccountStatus.TrialExpired || isNewPlan)
                {
                    account.CurrentBillingPeriodStartedOn = this.RequestDate();
                }
                account.Update();
                if (!user.IsVerified)
                    //if they enter their credit card, it is safe to assume the email address is verified.  i hope...
                {
                    user.TrackChanges();
                    user.IsVerified = true;
                    user.Update();
                }
                this.LogBusinessEvent("Subscribed", new
                                                        {
                                                            PriorStatus = originalStatus.ToString(),
                                                            Plan = plan.Id,
                                                        }, sendEmail: true);
                var getStartedUrl = model.PostSignupUrl.CharsOr(() => Url.Action<DashboardController>(d => d.Index(null, null)));
                return View("Subscribe-Success",
                                new Signup_Subscribe_Success
                                    {
                                        GetStartedUrl = getStartedUrl,
                                    });
            }

            return View(model);
        }


        /// <summary>
        /// Fills various filling info with what the user entered.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billingInfo"></param>
        private static void FillBillingInfo(Signup_Subscription data, RecurlyBillingInfo billingInfo)
        {
            billingInfo.PostalCode = data.PostalCode;
            billingInfo.FirstName = data.FirstName;
            billingInfo.LastName = data.LastName;
            billingInfo.CreditCard.ExpirationMonth = data.CreditCardMonth;
            billingInfo.CreditCard.ExpirationYear = data.CreditCardYear;
            billingInfo.CreditCard.VerificationValue = data.CreditCardVerificationCode;
            billingInfo.CreditCard.Number = data.CreditCardNumber;
        }

        /// <summary>
        /// Gets the status of the current account's subscription.  Returns null if no subscription was found.  Otherwise it returns "active" or "canceled"
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        private static string GetRecurlySubscriptionStatus(AccountThing account)
        {
            XDocument subscription = null;
            try
            {
                RecurlyClient.PerformRequest(RecurlyClient.HttpRequestMethod.Get,
                                             "/accounts/" + account.Id.ToInvariant() + "/subscription",
                                             (reader => { subscription = XDocument.Load(reader); }));
            }
            catch (NotFoundException)
            {
                return null;
            }
            return subscription.Descendants("state").FirstOrDefault().MapIfNotNull(e => e.Value);
        }

        /// <summary>
        /// Attempts to create a recurly subscription for the given account.  If there is a return common error it will fill ModelState errors for reporting.  Otherwise an exception is thrown.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="user"></param>
        /// <param name="account"></param>
        /// <param name="planId"> </param>
        /// <param name="recurlyAccount"></param>
        /// <returns></returns>
        private void CreateSubscription(Signup_Subscription data, IUser user, IAccount account, string planId, RecurlyAccount recurlyAccount)
        {
            try
            {
                var subscription = new RecurlySubscription(recurlyAccount)
                                       {
                                           PlanCode = planId,
                                           Quantity = 1,
                                           CouponCode = account.CreatedDuringBeta() ? "betaaccount" : null,
                                       };
                if (account.Status == AccountStatus.Trial && account.DaysLeftInTrial.GetValueOrDefault(0) > 0)
                {
                    subscription.TrialPeriodEndsAt = account.TrialPeriodEndsOn;
                }
                subscription.Create();
            }
            catch (ValidationException ex)
            {
                //recurly included "Billing info." in lots of the error messages.  so we remove that crap
                if (ex.Errors.HasItems())
                    ex.Errors.Select(e => e.Message.Strip("Billing info.").UpperFirstChar()).Each(
                        e => ModelState.AddModelError(string.Empty, e));
            }
            catch
            {
                throw;
            }
        }




        #endregion
    }
}