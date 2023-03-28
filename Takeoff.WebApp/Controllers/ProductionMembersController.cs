using System.Linq;
using System.Web.Mvc;

using Mediascend.Web;
using MvcContrib;
using Takeoff.Data;
using Takeoff.Models;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Dynamic;

namespace Takeoff.Controllers
{
    [RestrictIdentity]
    public class ProductionMembersController : BasicController
    {

        /// <summary>
        /// Returns a object containing the id, name, and email of 
        /// 1.  all members of the current user's account, if they have one
        /// 2.  all members of productions the user is current a part of
        /// 
        /// That aren't already in the current production of course.  This is used in the autocomplete dropdown.
        /// </summary>
        /// <param name="productionId"></param>
        /// <returns></returns>
        [AuthorizeProductionAccess(ProductionIdParam = "productionId")]
        public ActionResult GetPotentials(int productionId)
        {
            var user = this.UserThing();

            var users = new HashSet<int>();
            //first we add everyone that is a member of the current user's account or any productions they are a member of
            var account = user.Account;
            if (account != null)
            {
                users.Add(account.ChildrenOfType<AccountMembershipThing>().Select(a => a.UserId));
            }
            //add everyone on every production this user is a part of
            foreach (var currProduction in Productions.GetProjectsWithAccess(user))
            {
                users.Add(currProduction.GetMembers());
            }

            //now remove the current user and anyone else who was invited to this production or is already a member
            var production = Things.Get<ProjectThing>(productionId).VerifyCreate(typeof(MembershipThing), this.UserThing());
            users.Remove(user.Id);
            users.Remove(production.GetMembers());
            users.Remove(production.ChildrenOfType<MembershipRequestThing>().Select(r => r.InviteeId));

            var nonMembers = users.Select(id => (UserView)Things.Get(id).CreateViewData(this.Identity())).OrderBy(u => u.Name).ToArray();
            return Json(nonMembers);
        }


        /// <summary>
        /// Invites a person with the given user ID or email to this production.
        /// </summary>
        /// <param name="productionId"></param>
        /// <param name="emailOrUserId2"></param>
        /// <param name="role"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is similar to the reverse of MembershipRequestsController.Create but due to some major differences, they weren't combined.  But changes to either should be reviewed as to whether or not they apply to the other.
        /// </remarks>
        [HttpPost]
        [RestrictIdentity(AllowDemo = false)]
        [TrialSignupRequired]
        [ValidateInput(false)]//so some chars like brackets can be included.  they are html escaped 
        [RequiresVerification()]
        public ActionResult Invite(int productionId, string emailOrUserId, string role, string note)
        {
            Args.HasChars(role);
            if (!role.EqualsAny("Staff", "Client"))
                throw new ArgumentException("Invalid Role");

            var invitedBy = this.UserThing();
            var production = Things.Get<ProjectThing>(productionId).VerifyCreate(typeof(MembershipThing), this.UserThing());
            var account = Things.Get<AccountThing>(production.AccountId);

            //first check for an existing member
            IUser invitee;
            var userId = emailOrUserId.ToIntTry();
            if (!userId.HasValue)
            {
                var email = (string)emailOrUserId;
                invitee = Repos.Users.GetByEmail((string)email);
                //they are new, send out the invitation and wait for their response
                if (invitee == null)
                {
                    //create the user
                    dynamic signupSource = new ExpandoObject();
                    signupSource.Type = "Invitation";
                    signupSource.ProjectId = productionId;
                    invitee = Users.AddInvitedUser(invitedBy.Id, email, email, signupSource, false);

                    //create the request and send them an email alert
                    var request = production.AddChild(new MembershipRequestThing
                    {
                        CreatedByUserId = invitedBy.Id,
                        InviteeId = invitee.Id,
                        Note = note,
                        IsInvitation = true,
                        Role = role,
                    }).Insert();
                    this.DeferRequest(Url.Action<EmailController>(c => c.SendMembershipRequest(request.Id)));
                    return Json(new { Type = "Invited", Request = request.CreateViewData(this.Identity()) });

                }
            }
            else 
            {
                invitee = Things.Get<UserThing>(userId.Value);
            }

            //this should only happen if user double submits or something
            if (invitee.IsMemberOf(production))
            {
                return Json("AlreadyMember");
            }
            //first check to see whether they were already invited.  This should only happen if user double submits or something
            var membershipRequest = production.FindChild<MembershipRequestThing>(r => r.InviteeId == invitee.Id);
            if (membershipRequest != null)
            {
                return Json("AlreadyInvited");
            }

            bool enableInvites = (bool)invitee.GetSettingValue(UserSettings.EnableInvitations);
            if (!enableInvites)
            {
                return Json("Rejected");
            }

            var autoresponse = MembershipRequestsController.InvitationAutoResponse(invitedBy, invitee);
            if (!autoresponse.HasValue)
            {
                //create the request and send them an email alert
                var request = production.AddChild(new MembershipRequestThing
                {
                    CreatedByUserId = this.UserId(),
                    CreatedOn = DateTime.UtcNow,
                    InviteeId = invitee.Id,
                    Note = note,
                    IsInvitation = true,
                    Role = role,
                }).Insert();
                this.DeferRequest(Url.Action<EmailController>(c => c.SendMembershipRequest(request.Id)));
                return Json(new { Type = "Invited", Request = request.CreateViewData(this.Identity()) });
            }
            else if (autoresponse.Value)
            {
                //in the small chance they aren't already on the account, add them
                if (!invitee.IsMemberOf(account))
                    Accounts.AddUserAccess(account, invitee, role);
                var membership = production.AddMember(invitee, invitedBy.Id);
                this.DeferRequest(Url.Action<EmailController>(c => c.SendNewProductionMember(invitee.Id, productionId, invitedBy.Id, true)));
                return Json(new { Type = "Added", Membership = membership.CreateViewData(this.Identity()) });
            }
            else
            {
                //they don't accept invitations 
                return Json("Rejected");
            }
        }



        [HttpPost]
        public ActionResult Delete(int id)
        {
            Things.Get<MembershipThing>(id).Verify(Permissions.Delete, this.UserThing()).Delete();
            return this.Empty();
        }

        /// <summary>
        /// Deletes the current user from the production.
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteCurrent(int projectId)
        {
            //had to put ToArray or else you get error "Collection was modified; enumeration operation may not execute."
            Things.Get<ProjectThing>(projectId).ChildrenOfType<MembershipThing>().Where(m => m.UserId == this.UserId()).ToArray().Each(m => m.Delete());
            return this.RedirectToAction<DashboardController>(c => c.Index("You have left the production.", null));
        }

    }

}
