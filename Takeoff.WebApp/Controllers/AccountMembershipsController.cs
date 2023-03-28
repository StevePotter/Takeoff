using System;
using System.Collections.Generic;
using System.Dynamic;
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
    /// Lets account owners view and modify the users on their account.
    /// </summary>
    /// <remarks>TODO: combine account/members with this.</remarks>
    [SubController("/account/memberships", true)]
    [RestrictIdentityAttribute(AllowDemo = false)]
    [TrialSignupRequired]
    public class AccountMembershipsController : BasicController
    {

        public ActionResult Index()
        {
            var user = this.UserThing();

            var memberships = new List<AccountMemberships_AccountsUserBelongsTo>();
            foreach (var membership in user.AccountMemberships.Values)
            {
                var account = Things.GetOrNull<AccountThing>(membership.AccountId);
                if (account != null && account.CreatedByUserId != user.Id)
                {
                    var accountOwner = Things.Get<UserThing>(account.CreatedByUserId);
                    memberships.Add(new AccountMemberships_AccountsUserBelongsTo
                                        {
                                            Id = membership.Id,
                                            AccountId = account.Id,
                                            AccountOwnerName = accountOwner.DisplayName,

                                        });
                }
            }
            return View(new AccountMemberships_Index
                            {
                                AccountsUserBelongsTo = memberships.ToArray(),
                            });
        }

        /// <summary>
        /// /account/memberships/{id}
        /// </summary>
        /// <param name="id">The ID of the membership record.</param>
        /// <returns></returns>
        public ActionResult Details(int id)
        {
            var user = this.UserThing();
            var membership = Things.Get<AccountMembershipThing>(id);
            if ( membership.UserId != user.Id)
            {
                throw new ArgumentException("You aren't a member if this account.");
            }
            var account = Things.GetOrNull<AccountThing>(membership.AccountId);
            if (account.CreatedByUserId == user.Id)
            {
                throw new InvalidOperationException("Cannot get details if you own the account.");
            }
            var accountOwner = Things.Get<UserThing>(account.CreatedByUserId);
            var productions = Productions.GetProjectsWithAccess(Things.Get<UserThing>(membership.UserId)).Where(p => p.AccountId == membership.AccountId).ToArray();

            return View(new AccountMemberships_Details
                            {
                                Id = id,
                                JoinedAccountOn = user.UtcToLocal(membership.CreatedOn),
                                AccountOwner = new UserSummary
                                                   {
                                                       Email = accountOwner.Email,
                                                       Id = accountOwner.Id,
                                                       Name = accountOwner.DisplayName,
                                                   },
                                Productions = productions.Select(p => new AccountMemberships_Details.Production
                                                                          {
                                                                              Id = p.Id,
                                                                              Title = p.Title,
                                                                          }).ToArray(),
                            });
        }


        /// <summary>
        /// Removes the user from the account.  Deletes membership to the account and all things within it.  This can only be executed by the account owner.
        /// </summary>
        /// <param name="id">ID of the membership record.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var user = this.UserThing();
            var membership = Things.Get<AccountMembershipThing>(id);
            if (membership.UserId != user.Id)
            {
                throw new ArgumentException("You aren't a member if this account.");
            }
            var account = Things.GetOrNull<AccountThing>(membership.AccountId);
            if (account.CreatedByUserId == user.Id)
            {
                throw new InvalidOperationException("Cannot get details if you own the account.");
            }
            Accounts.RemoveMemberFromAccount(membership.AccountId, user);
            return this.RedirectToAction(c => c.Index());
        }

    }
}
