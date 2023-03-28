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
    [SubController("/account/members", true)]
    [RestrictIdentityAttribute(AllowDemo = false, RequireAccount = true)]
    [TrialSignupRequired]
    public class AccountMembersController : BasicController
    {

        public ActionResult Index()
        {
            var user = this.UserThing();
            var account = user.Account;
            var members = account.ChildrenOfType<AccountMembershipThing>().
                Where(am => am.UserId != user.Id).
                Select(d => CreateViewModelFromData(d, user)).ToArray();
            return View(new AccountMembers_Index{ Memberships = members });
        }

        /// <summary>
        /// /account/members/{id}
        /// </summary>
        /// <param name="id">The ID of the membership record.</param>
        /// <returns></returns>
        public ActionResult Details(int id)
        {
            var user = this.UserThing();
            var account = user.Account;
            ViewData["AccountId"] = account.Id;

            var membership = account.FindDescendantById(id).CastTo<IAccountMembership>();
            var productions =
                Productions.GetProjectsWithAccess(Things.Get<UserThing>(membership.UserId)).Where(
                    p => p.AccountId == membership.AccountId).ToArray();

            return View(new AccountMembers_Details
                            {
                               Membership = CreateViewModelFromData(membership, user),
                               Productions = productions.Select(p => new AccountMembers_Details.Production{ Id = p.Id, Title = p.Title}).ToArray(),
                            });
        }


        [HttpPost]
        public ActionResult Update(AccountMembers_Update model)//int userId, int accountId, string role)
        {
            var user = this.UserThing();
            if (!model.Role.EqualsAny("Staff", "Client"))
                throw new ArgumentException("Invalid Role");
            var account = user.Account;
            var membership = account.FindDescendantById(model.Id).CastTo<AccountMembershipThing>();
            if (membership == null)
                throw new ArgumentException("Data not found.");
            membership.TrackChanges();
            membership.Role = model.Role;
            membership.Update();
            return this.Empty();
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
            var account = user.Account;
            var membership = account.FindDescendantById(id).CastTo<AccountMembershipThing>();
            if (membership == null)
                throw new ArgumentException("Data not found.");
            var userToRemove = Things.Get<UserThing>(membership.UserId);
            Accounts.RemoveMemberFromAccount(account.Id, userToRemove);
            return this.RedirectToAction<AccountMembersController>(c => c.Index());
        }


        private static AccountMembers_Membership CreateViewModelFromData(IAccountMembership m, IUser currentUser)
        {
            var member = Things.GetOrNull<UserThing>(m.UserId);
            return new AccountMembers_Membership
            {
                MembershipId = m.Id,
                Role = m.Role,
                CreatedOn = currentUser.UtcToLocal(m.CreatedOn),
                MemberName = member.MapIfNotNull(u => u.DisplayName),
                MemberEmail = member.MapIfNotNull(u => u.Email),
                MemberId = m.UserId,
            };
        }

    }
}
