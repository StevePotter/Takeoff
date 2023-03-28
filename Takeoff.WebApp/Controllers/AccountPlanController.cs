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

    /// <summary>
    /// Lets users upgrade and downgrade their current plan.
    /// </summary>
    [SubController("/account/subscription", true)]
    [TrialSignupRequired]
    [Trial2Forbidden]
    [RestrictIdentityAttribute(AllowDemo = false, RequireAccount = true)]
    public class AccountPlanController : BasicController
    {
        //
        // /account/subscription/

        public ActionResult Index()
        {
            var user = this.UserThing();
            var account = user.Account;
            return View(new AccountPlan_Index
                            {
                                Plan = account.Plan,
                                AvailablePlans = this.Repository<Takeoff.Data.IPlansRepository>().GetPlansForSale().Select(p => new PlanForSale(p)).ToArray(),
                            });
        }

        [HttpGet]
        public ActionResult Edit(string plan)
        {
            var user = this.UserThing();
            var account = user.Account;
            ViewBag.User = user;
            ViewBag.Account = account;
            var currentPlan = account.Plan;
            if (currentPlan.Id.EqualsCaseSensitive(plan))
                throw new ArgumentException("This is your current account.");

            var newPlan = Repos.Plans.Get(plan);
            if (newPlan == null)
                throw new ArgumentException("Invalid plan.");
            if (!newPlan.AllowSignups)
                throw new ArgumentException("Sorry but you can't switch to this plan.");
            var isUpgrade = newPlan.PriceInCents > currentPlan.PriceInCents;
            if (isUpgrade)
            {
                return View("Edit-Upgrade", new AccountPlan_Edit
                                                {
                                                    CurrentPlan = currentPlan,
                                                    NewPlan = newPlan,
                                                });
            }
            else
            {
                var invalidDowngrade = account.CanDowngradeTo(newPlan);
                if (invalidDowngrade != null)
                {
                    return View("Edit-DowngradeInvalid", new AccountPlan_Edit_DowngradeInvalid
                                                             {
                                                                 LimitsExceeded = invalidDowngrade,
                                                             });
                }
                //downgrades are only possible if they are subscribed.  Otherwise they might not be serious about paying for this and just want a temporary account to abuse
                if (account.IsSubscribed())
                {
                    return View("Edit-Downgrade", new AccountPlan_Edit
                    {
                        CurrentPlan = currentPlan,
                        NewPlan = newPlan,
                    });
                }
                else
                {
                    ViewData["PostSubscribeUrl"] = Request.Url.PathAndQuery;
                    return View("Edit-DowngradeForbidden");
                }
            }
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateJsonAntiForgeryToken]
        [LogRequest(true, true)]
        public ActionResult EditPost(string plan)
        {
            var user = this.UserThing();
            var account = user.Account;
            if (account == null)
                throw new InvalidOperationException("You must have an account.");
            var currentPlan = account.Plan;
            if (currentPlan.Id.EqualsCaseSensitive(plan))
                throw new ArgumentException("This is your current account.");

            var newPlan = Repos.Plans.Get(plan);
            if (newPlan == null)
                throw new ArgumentException("Invalid plan.");
            if (!newPlan.AllowSignups)
                throw new ArgumentException("Sorry but you can't switch to this plan.");

            var isUpgrade = newPlan.PriceInCents > currentPlan.PriceInCents;
            if (!isUpgrade)
            {
                var invalidDowngrade = account.CanDowngradeTo(newPlan);
                if (invalidDowngrade != null)
                    throw new InvalidOperationException("You cannot downgrade to this plan.");
            }
            
            try
            {
                account.ChangePlan(plan, true);
            }
            catch (Recurly.ValidationException ex)
            {
                //recurly included "Billing info." in lots of the error messages.  so we remove that crap
                if (ex.Errors.HasItems())
                    ex.Errors.Select(e => e.Message.Strip("Billing info.").UpperFirstChar()).Each(e => ModelState.AddModelError(string.Empty, e));
            }
            catch
            {
                ViewData["ExceptionView"] = "Error-Billing";//give them a special error message for unknown billing crisis
                throw;
            }

            if (ModelState.IsValid)
            {
                this.LogBusinessEvent("PlanChanged", new
                {
                    Upgrade = isUpgrade,
                    NewPlan = newPlan.Id,
                    OldPlan = currentPlan.Id,
                }, sendEmail: true);
                return this.RedirectToAction<DashboardController>(c => c.Index("Your plan has been changed.", null));
            }
            return this.Empty();
        }



    }
}
