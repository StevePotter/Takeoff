using System.Linq;
using System.Security;
using System.Web.Mvc;

using Mediascend.Web;
using MvcContrib;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Controllers;
using System;
using Takeoff.ViewModels;

namespace Takeoff.Controllers
{
    [RestrictIdentity]
    public class MembershipRequestsController : BasicController
    {
        /// <summary>
        /// Creates a membership request for the thing id passed.  Includes an option personalized note.  Right now only productions are supported.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [RequiresVerification]
        [ValidateInput(false)]//so some chars like brackets can be included.  they are html escaped 
        public ActionResult Create(Productions_Details_RequestAccess model)
        {
            var user = this.UserThing();
            var production = Things.Get<ProjectThing>(model.ProductionId);

            //this would happen if someone gets an acceptance email and refreshes the page
            if (user.IsMemberOf(production))
            {
                return this.RedirectToAction<ProductionsController>(p => p.Details(production.Id, null, null, null, null));
            }

            //this action should never really even be run if the user is banned, but handle it gracefully anyway.  really, this could throw an exception
            var productionCreator = production.Owner;
            if (IsUserBanned(user, production) || !(bool)productionCreator.GetSettingValue(UserSettings.EnableMembershipRequests))
            {
                return View("Production-MembershipRequests-Disabled");
            }

            //if they refresh, we could end up with a duplicate membership request
            if (!HasPendingMembershipRequest(production, user))
            {
                var request = production.AddChild(new MembershipRequestThing
                {
                    CreatedByUserId = user.Id,
                    InviteeId = user.Id,
                    Note = model.Note,
                }).Insert();
                this.DeferRequest(Url.Action<EmailController>(c => c.SendMembershipRequest(request.Id)));
            }

            return View("Created");
        }

        public ActionResult Details(int id)
        {
            var request = Things.Get<MembershipRequestThing>(id);
            var production = (ProjectThing)request.Container;

            var requestedBy = Things.Get<UserThing>(request.InviteeId);
            var account = Things.Get<AccountThing>(request.AccountId);
            var productionOwner = production.Owner;
            
            var currUser = this.UserThing();
            if (production.Owner.Id != currUser.Id)
            {
                throw new SecurityException("You are not authorized to do this.");
            }

            return View(new MembershipRequests_Details
                            {
                                CreatedOn = this.UserThing().UtcToLocal(request.CreatedOn),
                                RequestId = request.Id, 
                                Note = request.Note,
                                RequestedByName = requestedBy.DisplayName,
                                RequestedByEmail = requestedBy.Email,
                                ProductionId = production.Id,
                                ProductionTitle = production.Title,
                            });
        }

        /// <summary>
        /// Approves a membership request.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="role">Only when it's not an invitation.  Otherwise use the MembershipRequestThing.Role property.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Approve(int id, string role)
        {
            var request = Things.Get<MembershipRequestThing>(id);
            var production = Things.Get<ProjectThing>(request.ParentId.Value);
            var invitee = Things.Get<UserThing>(request.InviteeId);
            var productionOwner = production.Owner;

            if (request.IsInvitation)
            {
                var invitedBy = Things.GetOrNull<UserThing>(request.CreatedByUserId) ?? productionOwner;
                if (invitee.Id != this.UserThing().Id)
                {
                    throw new SecurityException("Only the invitee can reject an invitation.");
                }

                //add them as a member
                var account = Things.Get<AccountThing>(production.AccountId);
                if (!invitee.IsMemberOf(account))
                    Accounts.AddUserAccess(account, invitee, request.Role);
                production.AddMember(invitee, invitedBy.Id);

                //remove request  send email to everyone
                request.Delete();
                this.DeferRequest(Url.Action<EmailController>(c => c.SendNewProductionMember(invitee.Id, production.Id, invitedBy.Id, false)));

                return View("InvitationAccepted", new MembershipRequests_InvitationAccepted
                                                      {
                                                          InvitedByDisplayName = invitedBy.DisplayName,
                                                          InvitedById = invitedBy.Id,
                                                          ProductionId = production.Id,
                                                      });
            }
            else
            {
                var isAccountMember = invitee.IsMemberOf(Things.Get<AccountThing>(production.AccountId));//if not, role will need to be chosen

                if (!isAccountMember && role == null)
                {
                    ViewData["RequestId"] = id;
                    return View("ChooseRoleForApproval");
                }

                if (!isAccountMember && !role.EqualsAny("Staff", "Client"))//note this same code is in ProductionMembersController so be careful
                    throw new ArgumentException("Invalid Role");

                var currUser = this.UserThing();
                if (production.Owner.Id != currUser.Id)
                {
                    throw new SecurityException("You are not authorized to do this.");
                }

                if (!invitee.IsMemberOf(production))//handle page refreshes
                {
                    DeleteMembershipRequestsForUser(request.InviteeId, production);
                    Productions.AddMember(production, currUser.Id, invitee.Id, role);
                    this.DeferRequest(Url.Action<EmailController>(c => c.SendNewProductionMember(invitee.Id, production.Id, currUser.Id, true)));
                }

                return this.RedirectToAction<DashboardController>(c => c.Index("This person is now a member of the production.", null));
            }
        }

        [HttpPost]
        public ActionResult Reject(int id, string preventFutureRequests)
        {
            var request = Things.Get<MembershipRequestThing>(id);
            var invitee = Things.Get<UserThing>(request.InviteeId);
            var production = Things.Get<ProjectThing>(request.ParentId.Value);
            var productionOwner = production.Owner;
            var invitedBy = Things.Get<UserThing>(request.CreatedByUserId) ?? productionOwner;

            var currUser = this.UserThing();
            if (request.IsInvitation)
            {
                if (invitee.Id != this.UserThing().Id)
                {
                    throw new SecurityException("Only the invitee can approve or reject an invitation.");
                }

                //remove request  send email to everyone
                request.Delete();
                return View("InvitationRejected", new MembershipRequests_InvitationRejected
                {
                    InvitedByDisplayName = invitedBy.DisplayName,
                    InvitedById = invitedBy.Id,
                    ProductionId = production.Id,
                });
            }
            else
            {
                if (production.Owner.Id != currUser.Id)
                {
                    throw new SecurityException("You are not authorized to do this.");
                }

                DeleteMembershipRequestsForUser(request.InviteeId, production);
                return View(new MembershipRequests_Reject
                                {
                                    RequestedById = invitee.Id,
                                    RequestedByName = invitee.DisplayName,
                                });
            }
        }

        [HttpPost]
        public ActionResult Cancel(int id)
        {
            var request = Things.Get<MembershipRequestThing>(id);
            if (request.HasPermission(Permissions.Delete, this.UserThing()))
            {
                request.Delete();
            }
            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult Resend(int id)
        {
            var request = Things.Get<MembershipRequestThing>(id);
            request.Verify(Permissions.Delete, this.UserThing());

            this.DeferRequest(Url.Action<EmailController>(c => c.SendMembershipRequest(request.Id)));

            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult DenyFutureRequestsFromUser(int userId)
        {
            var requestedBy = Things.GetOrNull<UserThing>(userId).EnsureExists(userId);
            var currUser = this.UserThing();

            if (!IsUserBanned(requestedBy, currUser))//just in case they refresh teh page, don't insert multiple records
            {
                using (var db = new DataModel())
                {
                    db.MembershipRequestAutoReponses.InsertOnSubmit(new Models.Data.MembershipRequestAutoReponse
                    {
                        UserId = currUser.Id,
                        Accept = false,
                        IsInvitation = false,
                        UserRequestedBy = requestedBy.Id
                    });
                    db.SubmitChanges();
                }
            }
            return this.RedirectToAction<DashboardController>(c => c.Index("This person is blocked.  You can reverse this in Account / Privacy.", null));
        }

        [HttpPost]
        public ActionResult SetFutureInvitationsAutoResponse(int userId, bool accept, string redirectTo)
        {
            var requestedBy = Things.GetOrNull<UserThing>(userId).EnsureExists(userId);
            var currUser = this.UserThing();

            using (var db = new DataModel())
            {
                //handles double submits and fringe cases
                db.MembershipRequestAutoReponses.DeleteAllOnSubmit(from r in db.MembershipRequestAutoReponses where r.IsInvitation.GetValueOrDefault() && r.Accept && r.UserRequestedBy == userId select r);

                db.MembershipRequestAutoReponses.InsertOnSubmit(new Models.Data.MembershipRequestAutoReponse
                {
                    UserId = currUser.Id,
                    Accept = accept,
                    IsInvitation = true,
                    UserRequestedBy = requestedBy.Id
                });
                db.SubmitChanges();
            }
            return this.Redirect(redirectTo);
        }


        private static void DeleteMembershipRequestsForUser(int userId, ProjectThing production)
        {
            //in the off chance of multiple requests being present, we make sure to clear them all
            var requests = production.ChildrenOfType<MembershipRequestThing>().Where(m => m.InviteeId == userId).ToArray();
            foreach (var currRequest in requests)
            {
                currRequest.Delete();
            }
        }

        public static bool IsUserBanned(UserThing requestedBy, ProjectThing production)
        {
            var owner = production.Owner;
            return IsUserBanned(requestedBy, owner);
        }

        public static bool IsUserBanned(UserThing requestedBy, UserThing user)
        {
            using (var db = DataModel.ReadOnly)
            {
                return ((from r in db.MembershipRequestAutoReponses where !r.IsInvitation.GetValueOrDefault() && !r.Accept && r.UserId == user.Id && r.UserRequestedBy == requestedBy.Id select r).Count()) > 0;
            }
        }

        public static bool HasPendingMembershipRequest(ProjectThing production, UserThing user)
        {
            return production.FindChild<MembershipRequestThing>(r => r.InviteeId == user.Id) != null;
        }


        public static bool? InvitationAutoResponse(IUser invitedBy, IUser invitee)
        {
            //they are already in the system.  check for auto accept or reject
            using (var db = DataModel.ReadOnly)
            {
                var response = (from r in db.MembershipRequestAutoReponses where r.IsInvitation.GetValueOrDefault() && r.UserId == invitee.Id && r.UserRequestedBy == invitedBy.Id select r).FirstOrDefault();
                if (response == null)
                    return new bool?();

                return response.Accept;
            }

        }


    }
}
