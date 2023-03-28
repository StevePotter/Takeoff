using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Mediascend.Web;
using MvcContrib;
using Newtonsoft.Json.Linq;
using Recurly;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Platform;
using Takeoff.Resources;
using Takeoff.ViewModels;

namespace Takeoff.Controllers
{
    public class AccountController : BasicController
    {
        private static readonly IIdentityService IdentityService = IoC.GetOrNull<IIdentityService>();

        public ActionResult GetStatus(string email)
        {
            return Json(Users.GetStatus(email));
        }


        #region Log in/out

        public ActionResult IsUserLoggedIn()
        {
            return Json(this.IsLoggedIn());
        }


        /// <summary>
        /// Combination login/signup page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [RootUrl]
        public ActionResult Login(Account_Login model)
        {
            ModelState.Clear();//avoids validation error being shown when email or password is missing from parameters

            //if the user copied a link with login in its url or they had a login page up, logged in on another tab, and refreshed teh original login page, this will happen.  In this case jsut redirect em.
            if (this.IsLoggedIn())
            {
                if (model.ReturnUrl.HasChars())
                    return this.Redirect(model.ReturnUrl);
                else
                    return this.RedirectToAction<DashboardController>(c => c.Index(null, null));
            }

            //model.UseAjax = ApplicationEx.UseAjaxPostForLogin(Request);
            return View(model);
        }

        [ActionName("Login")]
        [HttpPost]
        [RootUrl]
        public ActionResult LoginPost(Account_Login model)
        {
            //if the user copied a link with login in its url or they had a login page up, logged in on another tab, and refreshed teh original login page, this will happen.  In this case jsut redirect em.
            if (this.IsLoggedIn())
            {
                if (model.ReturnUrl.HasChars())
                    return this.Redirect(model.ReturnUrl);
                else
                    return this.RedirectToAction<DashboardController>(c => c.Index(null, null));
            }

            if (!ModelState.IsValid)
                return View(model);

            bool ignorePassword = false;
            if ( Request.IsLocal)
            {
                ignorePassword = ApplicationSettings.LoginWithAnyPassword;                
            }

            var result = Users.Login(model.Email, model.Password, model.TimezoneOffset, model.RememberMe, ignorePassword);
            switch (result)
            {
                case Users.LoginResult.Success:
                    if (model.ReturnUrl.HasChars())
                        return this.Redirect(model.ReturnUrl);
                    else
                        return this.RedirectToAction<DashboardController>(c => c.Index(null, null));
                    break;
                case Users.LoginResult.NotFound:
                    ModelState.AddModelError(string.Empty, S.Login_Validation_EmailNotFound);
                    break;
                case Users.LoginResult.WrongPassword:
                    ModelState.AddModelError("Password", S.Login_Validation_WrongPassword);
                    break;
                case Users.LoginResult.NoPassword:
                    //todo: give them a link to change their password
                    ModelState.AddModelError("Password", S.Login_Validation_WrongPassword);
                    break;
            }
            return View(model);
        }


        [RootUrl]
        public ActionResult Logout(bool? logoutIfPersistent)
        {
            if ( this.Identity() == null)
            {
                return View("Signout-Done");                
            }

            //if they have a persistent cookie, they might be a bit confused and think they won't have to log in next time.  this happened a lot.  so we put in an extra step for these people
            if (!logoutIfPersistent.HasValue)
            {
                if (FormsAuthentication.Decrypt(Request.Cookies[FormsAuthentication.FormsCookieName].Value).IsPersistent)
                {
                    return View("Signout-Confirm");
                }
            }

            var user = this.UserThing2();
            var account = user.MapIfNotNull(u => u.Account);

            IdentityService.ClearIdentity(HttpContext);
            
            if ( account != null && account.Status == AccountStatus.TrialAnonymous)
            {
                return View("Signout-AnonymousTrial");
            }

            return View("Signout-Done");
        }

        #endregion

        #region Expiration, Limits etc

        /// <summary>
        /// Redirected to when the current account has met a given limit, or doesn't have access to a given section.
        /// </summary>
        /// <returns></returns>
        /// <param name="type">The type of limit that has been reached.  Values include "videos", "assets", and "productions"</param>
        [HttpGet]
        public ActionResult LimitReached(string type, bool? isOwner)
        {
            if (!isOwner.GetValueOrDefault())
            {
                return View("LimitReached-NotOwner");
            }
            var user = this.UserThing2();
            var account = user.MapIfNotNull(u => u.Account);
            if ( account == null )
            {
                return View("LimitReached-NotOwner");                
            }

            var plan = account.MapIfNotNull(a => a.Plan);
            string limitation;
            switch (type.CharsOrEmpty().ToLowerInvariant())
            {
                case "assetfilesize":
                    limitation = S.Account_LimitReachedMsg_AssetFileSize;
                    break;
                case "assetcount":
                    limitation = S.Account_LimitReachedMsg_AssetCount;
                    break;
                case "assettotalsize":
                    limitation = S.Account_LimitReachedMsg_AssetTotalSize;
                    break;
                case "assetalltimecount":
                    limitation = S.Account_LimitReachedMsg_AssetAllTimeCount;
                    break;
                case "productions":
                    limitation = S.Account_LimitReachedMsg_ProductionCount;
                    break;
                case "videofilesize":
                    limitation = S.Account_LimitReachedMsg_VideoFileSize;
                    break;
                case "videocount":
                    limitation = S.Account_LimitReachedMsg_VideoCount;
                    break;
                case "videouploadcount":
                    limitation = string.Format(S.Account_LimitReachedMsg_VideoUploadCount, plan.VideosPerBillingCycleMax.GetValueOrDefault());
                    break;
                default:
                    limitation = S.Account_LimitReachedMsg_GenericLimit;
                    break;
            }

            if (account.Status == AccountStatus.Demo)
            {
                return View("LimitReached-Demo", new Account_LimitReached { LimitationMessage = limitation });
            }
            if (account.Status == AccountStatus.TrialAnonymous)
            {
                return View("LimitReached-TrialAnonymous", new Account_LimitReached { LimitationMessage = limitation });
            }
            if (account.Status == AccountStatus.Trial2)
            {
                return View("LimitReached-Trial2", new Account_LimitReached { LimitationMessage = limitation });
            }

            return View(new Account_LimitReached { LimitationMessage = limitation });
        }

        #endregion


        #region Password Reset

        /// <summary>
        /// Creates a page for the user to reset their password.
        /// </summary>
        [HttpGet]
        [RootUrl]
        public ActionResult PasswordReset(Account_PasswordReset model)
        {
            ModelState.Clear();//avoids validation error if certain parameters aren't included.  validation is meant for the POST action

            if (model.ResetKey.HasNoChars())    //this is generally linked to from the login page
            {
                return View("PasswordReset-Request", model);
            }

            if (model.Email.HasNoChars())   //a reset key but no email?  give em an error message
            {
                return View("PasswordReset-InvalidLink");
            }

            if (this.IsLoggedIn() && !model.Email.EqualsCaseInsensitive(this.UserThing2().Email))//they are logged in with one account and tried to reset for another.  this happened a lot in testing/development although it should be rare in reality
            {
                IdentityService.ClearIdentity(HttpContext);
            }

            var user = this.IsLoggedIn() ? this.UserThing2() : Repos.Users.GetByEmail(model.Email);
            if (user == null)   //email doesn't exist so give them option to reenter
            {
                return View("PasswordReset-InvalidLink");
            }
            else if (!user.PasswordResetKey.EqualsCaseSensitive(model.ResetKey))    //old link
            {
                return View("PasswordReset-InvalidLink");
            }
            else
            {
                return View(model);
            }
        }

        /// <summary>
        /// Occurs when a person requests a password reset email or when they change their password.
        /// </summary>
        [HttpPost]
        [ActionName("PasswordReset")]
        [ValidateJsonAntiForgeryToken]
        public ActionResult PasswordResetPost(Account_PasswordReset model)
        {
            //all they included was the email so it's for the reset request
            if (model.Email.HasChars() && !model.ResetKey.HasChars() && !model.NewPassword.HasChars())
            {
                var result = Users.RequestPasswordReset(model.Email);
                if (result == null) //success
                {
                    this.DeferRequest(Url.Action<EmailController>(c => c.PasswordReset(model.Email)));
                    return this.Result(() => this.SuccessMessage(S.PasswordResetRequestSent_Text, S.PasswordResetRequestSent_PageHeading, S.PasswordResetRequestSent_PageTitle), () => result);
                }
                else
                {
                    return this.Result(() => this.View("PasswordReset-Request-UserNotFound"), () => result);
                }
            }
            else
            {
                if (model.ResetKey.HasNoChars())
                {
                    throw new ArgumentException("No reset key passed.");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                else
                {
                    var result = Users.ResetPassword(model.Email, model.ResetKey, model.NewPassword);
                    if (result == null)
                    {
                        return this.RedirectToAction<DashboardController>(c => c.Index(S.PasswordReset_Success, null));
                    }
                    else
                    {
                        throw new InvalidOperationException("Could not reset password.  Reason: " + result);
                    }
                }

            }
        }


        #endregion

        #region Verification

        /// <summary>
        /// Generates the verification page for the user to set their password and stuff, resulting in complete verification.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="verificationKey"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Verify(Account_Verify model)
        {
            ModelState.Clear();

            IUser user;
            if (this.IsLoggedIn())
            {
                user = this.UserThing2();
                if (model.Email.HasNoChars() || model.Email.EqualsCaseInsensitive(this.UserThing2().Email))
                {
                    if (user.IsVerified)
                    {
                        return View("Verify-AlreadyDone");
                    }
                    else if (user.VerificationKey.EqualsCaseSensitive(model.VerificationKey))
                    {
                        return CompleteVerification(model, user);                        
                    }
                    else
                    {
                        this.DeferRequest(Url.Action<EmailController>(c => c.SendUserVerify(this.UserThing().Id)));
                        return this.Result(() => this.SuccessMessage(S.Account_Verify_EmailSuccess_Msg, S.Account_Verify_EmailSuccess_PageHeading, S.Account_Verify_EmailSuccess_PageTitle), () => "EmailSent");
                    }
                }
                else
                {
                    //they are logged in with one account and tried to verify another.  this happened a lot in testing/development although it should be rare in reality
                    IdentityService.ClearIdentity(HttpContext);
                }
            }

            if (!model.VerificationKey.HasChars() || !model.Email.HasChars())
            {
                return View("Verify-Request", new Account_Verify_Request());
            }

            user = this.IsLoggedIn() ? this.UserThing2() : Repos.Users.GetByEmail(model.Email);
            if (user == null || (!user.IsVerified && !model.VerificationKey.EqualsCaseInsensitive(user.VerificationKey)))
            {
                return View("Verify-InvalidLink");
            }

            if (user.IsVerified)
            {
                return View("Verify-AlreadyDone");
            }

            //log them in 
            if (!this.IsLoggedIn())
            {
                IoC.Get<IIdentityService>().SetIdentity(new UserIdentity
                {
                    UserId = user.Id,
                }, IdentityPeristance.TemporaryCookie, HttpContext);                
            }

            return CompleteVerification(model, user);
        }

        private ActionResult CompleteVerification(Account_Verify model, IUser user)
        {
            user.TrackChanges();
            user.IsVerified = true;
            Repos.Users.Update(user, "EmailVerified");

            if (model.ReturnUrl.HasChars())
                return Redirect(model.ReturnUrl);
            else
                return View("Verify-Success");
        }

        /// <summary>
        /// Submitted by the Verify view form.  This is submitted when the user hadn't entered a password or email yet.
        /// </summary>
        /// <param name="verificationKey"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="nickname"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("Verify")]
        public ActionResult VerifyPost(Account_Verify_Request model)
        {
            if (!ModelState.IsValid)
                return View("Verify-Request", model);

            var user = Repos.Users.GetByEmail(model.Email);
            if (user == null)//this really should just be the same form but a validation error
            {
                ModelState.AddModelError("Email", S.Account_Verify_UserNotFound_Msg);
                return this.Result(() => View("Verify-Request"), () => "UserNotFound");
            }
            else if (user.IsVerified)
            {
                return this.Result(() => View("Verify-AlreadyDone"), () => "AlreadyVerified");
            }
            else
            {
                this.DeferRequest(Url.Action<EmailController>(c => c.SendUserVerify(user.Id)));
                return this.Result(() => this.SuccessMessage(S.Account_Verify_EmailSuccess_Msg, S.Account_Verify_EmailSuccess_PageHeading, S.Account_Verify_EmailSuccess_PageTitle), () => "EmailSent");
            }
        }

        #endregion


        [OldUrl("/Settings")]
        [OldUrl("/Settings/Index")]//because emails might have referenced this link
        [RestrictIdentity(AllowDemo = false)]
        public ActionResult Index()
        {
            var account = this.UserThing2().Account;
            if (account == null)
            {
                return this.View("Index-NoAccount");
            }
            else
            {
                return this.View("Index-HasAccount", account);
            }
        }

        [RestrictIdentity(AllowDemo = false)]
        [TrialSignupRequired]
        public ActionResult MainInfo()
        {
            var user = this.UserThing2();
            return View("MainInfo", new Account_MainInfo
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CurrentPassword = string.Empty
                });
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken()]
        [RestrictIdentity(AllowDemo = false)]
        [TrialSignupRequired]
        public ActionResult MainInfo(Account_MainInfo model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = this.UserThing2();
            //ensure the password is cool
            if (!Users.IsPasswordCorrect(user, model.CurrentPassword))
            {
                //if they tried more than 5 times to enter the password, kick em out
                var pwAttempts = Session["pwattempts"].MapIfNotNull(v => (int)v, 0) + 1;
                Session["pwattempts"] = pwAttempts;
                if ((int)pwAttempts >= ConfigUtil.GetRequiredAppSetting<int>("AccountMainInfoMaxPasswordAttempts"))
                {
                    var msg =
                        "You entered the wrong password too many times.  For security reasons, we need you to log back in.";
                    IdentityService.ClearIdentity(HttpContext);
                    return this.RedirectToAction<RootController>(c => c.Index(new StartupMessage { StartupMessageBody = msg, StartupMessageType = StartupMessageType.Warning }));
                }

                ModelState.AddModelError("CurrentPassword", @"The 'current' password you entered doesn't match your actual password.");
                return View(model);
            }

            user.TrackChanges();
            if (!user.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (Repos.Users.GetByEmail(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "Someone else in Takeoff already uses this email address.");
                    return View(model);
                }
                user.Email = model.Email;
            }
            if (model.NewPassword.HasChars())
                user.UpdatePassword(model.NewPassword);
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UpdateDisplayName();
            user.Update();

            model.CurrentPassword = null;
            model.NewPassword = null;
            Session.Remove("pwattempts");

            ViewData["StartupMessage"] = new Takeoff.ViewModels.StartupMessage
                                             {
                                                 StartupMessageBody = "Your information has been updated.",
                                                 StartupMessageType = StartupMessageType.Success,
                                             };
            return View(model);
        }

        [RestrictIdentity(RequireAccount = true)]
        public ActionResult Logo()
        {
            var user = this.UserThing2();
            var account = user.Account;
            var logo = account.Logo;
            return View("Logo", new Account_Logo
            {
                CurrentLogoUrl = logo.MapIfNotNull(l => l.GetUrlHttps())
            });
        }

        static Size MaxLogoSize = new Size(200, 70);
        static long LogoQuality = 70;


        [HttpPost]
        [ValidateJsonAntiForgeryToken()]
        [RestrictIdentity(RequireAccount = true)]
        public ActionResult Logo(Account_Logo model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = this.UserThing2();
            var account = user.Account.CastTo<AccountThing>();
            model.CurrentLogoUrl = account.Logo.MapIfNotNull(i => i.GetUrlHttps());//so the invalid original view can still have the current logo's value

            string logoFileName = null;
            Size logoSize = Size.Empty;
            string logoFileLocation = null;
            if (model.Logo != null && model.Logo.InputStream != null)
            {
                var fileStream = model.Logo.InputStream;
                logoFileLocation = account.FilesLocation.CharsOr(ConfigUtil.GetRequiredAppSetting("ProductionBucket").EndWith("/") + account.Id.ToInvariant());//default value 

                var nameAndSize = WebImageUtil.ResizeAndUpload(fileStream, logoFileLocation, MaxLogoSize, LogoQuality);
                if (nameAndSize == null)
                {
                    ModelState.AddModelError("Logo", Strings.Productions_Create_InvalidImageType_Msg);
                    return View(model);
                }
                logoFileName = nameAndSize.Item1;
                logoSize = nameAndSize.Item2;
            }
            account.TrackChanges();
            //delete a current standard logo
            if (account.LogoImageId.HasValue)
            {
                var logo = account.FindDescendantById(account.LogoImageId.Value);
                if (logo != null)
                    logo.Delete();
                account.LogoImageId = null;
            }

            if (logoFileName.HasChars())
            {
                var newLogo = account.AddChild(new ImageThing
                                                   {
                                                       CreatedByUserId = user.Id,
                                                       CreatedOn = this.RequestDate(),
                                                       Location = logoFileLocation,
                                                       OriginalFileName = model.Logo.FileName,
                                                       FileName = logoFileName,
                                                       DeletePhysicalFile = true,
                                                       Height = logoSize.Height,
                                                       Width = logoSize.Width,
                                                   }).Insert();
                account.LogoImageId = newLogo.Id;
                model.CurrentLogoUrl = newLogo.MapIfNotNull(i => i.GetUrlHttps());
            }
            account.Update();

            return View(model);
        }


        /// <summary>
        /// EnsureFirstLastNameAttribute uses this to upgrade to first/last name system.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [RestrictIdentity(AllowDemo = true)]
        [ExcludeFilter(typeof(EnsureNecessaryUserInfoAttribute))]//attribute redirects here so exclude it from this action
        [ExcludeFilter(typeof(CheckSuspendedAccountAttribute))]//caused an infinite loop
        public ActionResult NecessaryInfo(string returnUrl)
        {
            var user = this.UserThing();

            //handles refresh and accidents
            if (user.FirstName.HasChars() && user.LastName.HasChars() && user.Password.HasChars() && user.SignupSource != null)
            {
                return returnUrl.HasChars() ? (ActionResult)this.Redirect(returnUrl) : (ActionResult)this.RedirectToAction<DashboardController>(c => c.Index(null, null));
            }

            var model = new Account_NecessaryInfo
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Password = user.Password == null ? null : string.Empty,//used just for null check.  hashed pw means nothing
                ReturnUrl = returnUrl,
                SignupSource = user.SignupSource == null ? null : string.Empty,//just for nll check
            };

            return View(model);
        }

        /// <summary>
        /// EnsureFirstLastNameAttribute uses this to upgrade to first/last name system.
        /// </summary>
        [RestrictIdentity(AllowDemo = false)]
        [TrialSignupRequired]
        [HttpPost]
        [ValidateJsonAntiForgeryToken()]
        [ExcludeFilter(typeof(EnsureNecessaryUserInfoAttribute))]
        [ExcludeFilter(typeof(CheckSuspendedAccountAttribute))]
        public ActionResult NecessaryInfo(Account_NecessaryInfo model)
        {
            var user = this.UserThing();

            //validate.  it's not really necessary to do server-side validation because once they are redirected, the EnsureNecessaryUserInfoAttribute will catch their missing info and bring them back here.  but it is useful for complex validations like password enforcement
            ModelState.Clear();
            if (model.FirstName.HasNoChars() && user.FirstName.HasNoChars())
            {
                ModelState.AddRequiredError(model, m => m.FirstName);
            }
            if (model.LastName.HasNoChars() && user.LastName.HasNoChars())
            {
                ModelState.AddRequiredError(model, m => m.LastName);
            }
            if (model.Password.HasNoChars() && user.Password.HasNoChars())
            {
                ModelState.AddRequiredError(model, m => m.Password);
            }
            if (user.SignupSource == null && model.SignupSource.HasNoChars())
            {
                ModelState.AddRequiredError(model, m => m.SignupSource);
            }

            if (!ModelState.IsValid)
                return View(model);

            //update the user's data
            user.TrackChanges();
            bool updateDisplayName = false;
            if (user.FirstName.HasNoChars())
            {
                user.FirstName = model.FirstName;
                updateDisplayName = true;
            }
            if (user.LastName.HasNoChars())
            {
                user.LastName = model.LastName;
                updateDisplayName = true;
            }
            if (updateDisplayName)
            {
                user.UpdateDisplayName();
            }
            if (user.Password.HasNoChars())
            {
                user.UpdatePassword(model.Password);
            }
            if (user.SignupSource == null)
            {
                dynamic signupSource = new JObject();
                signupSource.Type = model.SignupSource;
                if (model.SignupSource.Equals("Other", StringComparison.OrdinalIgnoreCase) && model.SignupSourceOther.HasChars())
                {
                    signupSource.Details = model.SignupSourceOther;
                }
                user.SignupSource = signupSource;
                if (user.SignupSource == null && model.SignupSource.HasChars())
                {
                    signupSource = new JObject();
                    user.SignupSource = signupSource;
                }
            }

            user.Update(description: "NecessaryInfo");

            if (model.ReturnUrl.HasChars())
            {
                return this.Redirect(model.ReturnUrl);
            }
            else
            {
                if (user.SignupSource.ProjectId != null)
                {
                    //make sure the production hasn't been deleted and the user still has access to the production
                    var productionId = Convert.ToInt32((object)user.SignupSource.ProjectId);
                    var production = Things.GetOrNull<ProjectThing>(productionId);
                    if (production != null && user.IsMemberOf(productionId))
                    {
                        return this.RedirectToAction<ProductionsController>(o => o.Details(productionId, null, null, null, null));
                    }
                }
                return this.RedirectToAction<DashboardController>(c => c.Index(null, null));
            }
        }

        [HttpGet]
        [RestrictIdentity(AllowDemo = false)]
        [TrialSignupRequired]
        public ActionResult Billing()
        {
            ViewBag.User = this.UserThing();
            return View("Billing", new Account_Subscription());
        }

        [RestrictIdentity(AllowDemo = false)]
        [HttpPost]
        [ValidateJsonAntiForgeryToken()]
        [TrialSignupRequired]
        public ActionResult Billing(Account_Subscription data)
        {
            var user = this.UserThing();
            AccountThing account = user.Account;
            //check if the month/year combo is invalid
            if (new DateTime(data.CreditCardYear, data.CreditCardMonth, 1).AddMonths(1).Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(string.Empty, "The card has already expired.");
            }

            //if the model isn't valid, the errors will be shown to the user
            if (!ModelState.IsValid)
            {
                return View(data);
            }

            try
            {
                var billingInfo = new RecurlyBillingInfo(account.Id.ToInvariant());
                billingInfo.PostalCode = data.PostalCode;
                billingInfo.FirstName = data.FirstName;
                billingInfo.LastName = data.LastName;
                billingInfo.CreditCard.ExpirationMonth = data.CreditCardMonth;
                billingInfo.CreditCard.ExpirationYear = data.CreditCardYear;
                billingInfo.CreditCard.VerificationValue = data.CreditCardVerificationCode;
                billingInfo.CreditCard.Number = data.CreditCardNumber;
                billingInfo.Update();
                return View("Billing-Success");
            }
            catch (Recurly.ValidationException ex)
            {
                //recurly included "Billing info." in lots of the error messages.  so we remove that crap
                if (ex.Errors.HasItems())
                    ex.Errors.Select(e => e.Message.Strip("Billing info.").UpperFirstChar()).Each(e => ModelState.AddModelError(string.Empty, e));
                return View(data);
            }
        }

        /// <summary>
        /// Deletes the user from the system.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [HttpGet]
        [ExcludeFilter(typeof(CheckSuspendedAccountAttribute))]
        [RestrictIdentity(AllowDemo = false)]
        [TrialSignupRequired]
        public ActionResult Close()
        {
            var account = this.UserThing().Account;
            if (account == null)
                return View("Close-Guest");
            else
            {
                ViewData["IsSubscribed"] = account.IsSubscribed();
                return View("Close-AccountHolder");
            }
        }

        [HttpPost]
        [ExcludeFilter(typeof(CheckSuspendedAccountAttribute))]
        [RestrictIdentity(AllowDemo = false)]
        [TrialSignupRequired]
        public ActionResult Close(bool deleteUser)
        {
            var user = this.UserThing();
            var account = user.Account;
            this.LogBusinessEvent("AccountClosed", new
                                                        {
                                                            KeepUser = !deleteUser
                                                        }, sendEmail: true);
            Accounts.DeleteAccountAndMaybeUser(user, this.RequestDate(), deleteUser);
            if (deleteUser)
            {
                IdentityService.ClearIdentity(HttpContext);
            }

            ViewData["UserId"] = user.Id;
            if ( account == null)
                return View("Closed-Guest");

            if (account.IsTrial())
                return View("Closed-Trial");

            if (account.IsSubscribed())
                return View("Closed-Subscriber");

            return View("Closed");//free plan, old demo, or some other crap that is super rare and we don't pay attention to
        }


        //[HttpGet]
        //public ActionResult TestClose()
        //{
        //    var user = this.UserThing();
        //    ViewData["UserId"] = user.Id; return View("Closed-Subscriber");

        //    return View("Closed");//free plan, old demo, or some other crap that is super rare and we don't pay attention to
        //}



        #region Privacy Section

        [RestrictIdentity(AllowDemo = false)]
        [TrialSignupRequired]
        [OldUrl("/Settings/Privacy")]//emails referenced this old link
        public ActionResult Privacy()
        {
            FillPrivacyViewData();

            return View();
        }

        [RestrictIdentity(AllowDemo = false)]
        [HttpPost]
        [TrialSignupRequired]
        public ActionResult Privacy(FormCollection forms)
        {
            UpdateSettingsFromFormCollection(forms, this.UserThing());
            ViewData["ShowUpdatedMsg"] = true;
            FillPrivacyViewData();

            return View();
        }

        [TrialSignupRequired]
        [RestrictIdentity(AllowDemo = false)]
        public ActionResult DeleteAutoResponse(int id)
        {
            using (var db = new DataModel())
            {
                db.MembershipRequestAutoReponses.DeleteAllOnSubmit(from m in db.MembershipRequestAutoReponses where m.Id == id && m.UserId == this.UserThing().Id select m);
                db.SubmitChanges();
            }

            FillPrivacyViewData();
            return View("Privacy");
        }


        private void FillPrivacyViewData()
        {
            var user = this.UserThing();
            var model = new Account_Privacy
                            {
                                EnableInvitations = (bool)user.GetSettingValue(UserSettings.EnableInvitations),
                                EnableMembershipRequests =
                                    (bool)user.GetSettingValue(UserSettings.EnableMembershipRequests),
                            };
            var responses = new List<Account_Privacy.AutoResponse>();
            using (var db = new DataModel())
            {
                var requestAutoReplies = (from m in db.MembershipRequestAutoReponses where m.UserId == user.Id select m).ToArray();
                var submitChanges = false;
                foreach (var data in (from m in db.MembershipRequestAutoReponses where m.UserId == user.Id select m))
                {
                    //this fixes a bug where when a user is deleted, their info might remain here
                    var requestedBy = Things.GetOrNull<UserThing>(data.UserRequestedBy);
                    if (requestedBy == null)
                    {
                        submitChanges = true;
                        db.MembershipRequestAutoReponses.DeleteOnSubmit(data);
                    }
                    else
                    {
                        responses.Add(new Account_Privacy.AutoResponse
                                          {
                                              Accept = data.Accept,
                                              Id = data.Id,
                                              IsInvitation = data.IsInvitation.GetValueOrDefault(),
                                              TargetUserEmail = requestedBy.Email,
                                              TargetUserName = requestedBy.DisplayName,
                                          });
                    }
                }
                if (submitChanges)
                    db.SubmitChanges();

                model.AutoResponses = responses.ToArray();
                ViewData.Model = model;
            }
        }

        #endregion

        public static void UpdateSettingsFromFormCollection(FormCollection forms, UserThing user)
        {

            //TODO: batch up the delete/insert/update for all the things to avoid many db hits
            foreach (var key in forms.AllKeys)
            {
                var userValue = forms[key];
                SettingThing.UpdateSetting(key, forms[key], user);
            }
        }


    }
}
