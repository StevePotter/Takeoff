using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security;
using Takeoff.Data;
using Takeoff.Models;


using System.IO;
using Recurly;
namespace Takeoff.Models
{

    /// <summary>
    /// DAL layer for accounts and users assigned to them.
    /// </summary>
    /// <remarks>
    /// This also handles assigning roles to users at the account level.  It might really belong elsewhere (Permissions) but for now this is the deal.
    /// </remarks>
    public static class Accounts
    {
        /// <summary>
        /// Adds the user to the account with the role specified. This does NOT check for an existing membership for the user, so check that yourself!
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="userId"></param>
        /// <param name="roleName"></param>
        public static void AddUserAccess(AccountThing account, IUser user, string roleName)
        {
            var membership = account.AddChild(new AccountMembershipThing
            {
                CreatedByUserId = user.Id,
                OwnerUserId = user.Id,
                CreatedOn = DateTime.UtcNow,
                UserId = user.Id,
                Role = roleName,
                TargetId = account.Id,
            }).Insert();

            //add a copy of the membership underneath the user
            //user.AddChild(membership.CreateLinkedThing<MembershipThing>()).Insert();
        }



        /// <summary>
        /// Creates an account for the given user and adds them under a given role.
        /// This does NOT check for an existing accoutn with the user as the owner (which ain't allowed right now), so check for that.
        /// Also does't call Insert() so you must do that.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="applicationId"></param>
        /// <param name="roleName"></param>
        /// <param name="createdOn"></param>
        /// <returns></returns>
        public static AccountThing Create(IUser user, string roleName, DateTime createdOn, string planId, AccountStatus status, DateTime? trialEndsAt = null)
        {
            //using (var insertBatcher = new CommandBatcher())
            //{
            var account = new AccountThing
            {
                IsContainer = true,
                CreatedByUserId = user.Id,
                CreatedOn = createdOn,
                PlanId = planId,
                TrialPeriodEndsOn = trialEndsAt,
                Status = status,
                CurrentBillingPeriodStartedOn = DateTime.UtcNow,
            };
            account.Insert();

            var membership = new AccountMembershipThing
            {
                CreatedByUserId = user.Id,
                CreatedOn = createdOn,
                UserId = user.Id,
                Role = roleName,
                TargetId = account.Id,
                AccountId = account.Id//todo: fix this so inserts can be combined and this is automatic
            };
            account.AddChild(membership).Insert();

            //add a copy of the membership underneath the user
            //user.AddChild(membership.CreateLinkedThing<AccountMembershipThing>()).Insert();

            // }
            return account;
        }

        /// <summary>
        /// Removes all membership access for the user on the given account.  The user cannot be the account owner.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="user"></param>
        public static void RemoveMemberFromAccount(int accountId, UserThing user)
        {
            var accountOwnedByUser = user.Account;
            if (accountOwnedByUser != null && accountOwnedByUser.Id == accountId)
                throw new InvalidOperationException("Account owner cannot be deleted from an account they own.");

            foreach (var thingMembership in user.EntityMemberships.Values)
            {
                var thing = Things.GetOrNull(thingMembership.Id);//have to get the membershipthing fresh so we can construct its thing tree and make proper notifications
                if (thing != null && thing.AccountId == accountId)
                {
                    thing.Delete();
                }
            }
        }


        /// <summary>
        /// Deletes teh user, their account, their memberships, and their projects from the system.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="user"></param>
        public static void DeleteAccountAndMaybeUser(UserThing user, DateTime deletedOn, bool deleteUser = true)
        {
            var account = user.Account;
            //will delete account and productions within it
            if (account != null)
            {
                if (account.IsSubscribed())
                {
                    var recurlyAccount = new RecurlyAccount(account.Id.ToInvariant());
                    recurlyAccount.CloseAccount();
                }
                account.Delete(deletedOn);

                //delete productions on this account
                using (var db = DataModel.ReadOnly)
                {
                    var productions = (from t in db.Things
                                       where
                                           t.DeletedOn == null && t.Type == Things.ThingType(typeof (ProjectThing)) &&
                                           t.AccountId == account.Id
                                       select t).ToArray();
                    (from t in db.Things where t.DeletedOn == null && t.Type == Things.ThingType(typeof(ProjectThing)) && t.AccountId == account.Id select t.Id).ToArray().Select(id => Things.GetOrNull<ProjectThing>(id)).Where(p => p != null).Each(project => project.Delete(deletedOn));
                }
                user.RemoveFromCache();//clear their memberships from cache
            }

            if (deleteUser)
            {
                using (var db = new DataModel())
                {
                    //remove the user from any other projects
                    foreach (var thingMembership in user.EntityMemberships.Values)
                    {
                        var thing = Things.GetOrNull<MembershipThing>(thingMembership.Id);//have to get the membershipthing fresh so we can construct its thing tree and make proper notifications
                        if (thing != null)
                            thing.Delete(deletedOn);
                    }

                    //delete all membership requests this user generated or was the invitee on
                    foreach (var requestId in (from mr in db.MembershipRequests
                                               join mrt in db.Things on mr.ThingId equals mrt.Id
                                               where (
                                                   mrt.DeletedOn == null && (mrt.CreatedByUserId == user.Id || mr.UserId == user.Id)
                                               )
                                               select mrt.Id))
                    {
                        var request = Things.GetOrNull<MembershipRequestThing>(requestId);
                        if (request != null)
                            request.Delete(deletedOn);
                    }
                    //delete auto responses (which are plain db records and not things).  you could techincally use a commandbatcher instead
                    db.MembershipRequestAutoReponses.DeleteAllOnSubmit(from m in db.MembershipRequestAutoReponses where m.UserId == user.Id || m.UserRequestedBy == user.Id select m);
                    db.SubmitChanges();
                }
                user.Delete(deletedOn);
            }
        }


        public static void UpdateRole(int userId, int accountId, string role, UserThing user)
        {
            Args.HasChars(role);
            if (!role.EqualsAny("Staff", "Client"))
                throw new ArgumentException("Invalid Role");
            var account = user.Account;
            if (account == null)
                throw new InvalidOperationException("Not an account owner.");
            if (account.Id != accountId)
                throw new ArgumentException("accountId was not valid");
            if (userId == user.Id)
                throw new InvalidOperationException("Account owner's role cannot be modified.");

            var membership = account.ChildrenOfType<AccountMembershipThing>().Where(am => am.UserId == userId).First();
            membership.TrackChanges();
            membership.Role = role;
            membership.Update();
        }



    }


}
