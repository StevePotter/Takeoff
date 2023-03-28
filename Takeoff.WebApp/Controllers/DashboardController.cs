using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.ViewModels;

namespace Takeoff.Controllers
{
    [RestrictIdentity(AllowSemiAnonymous = false)]
    public class DashboardController : BasicController
    {
        /// <summary>
        /// The user's dashboard.  
        /// </summary>
        /// <returns></returns>
        /// <param name="message">A notification message that will be shown when the page first loads.</param>
        /// <param name="messageType">If message is set, this indicates the type of message that will be shown.  Valid values are "info", "warning", "success", "error".</param>
        public ActionResult Index(string message, string messageType)
        {
            var model = new Dashboard_Index();
            if ( message.HasChars())
                model.StartupMessageBody = message;
            if (messageType.HasChars())
                model.StartupMessageType = messageType.ConvertTo<StartupMessageType>();
            var user = this.UserThing();
            var account = user.Account;
            model.Account = account;
            var productions = Productions.GetProjectsWithAccess(user).OrderByDescending(p => p.LastChangeDate);
            model.Productions = productions.Select(p => new Dashboard_Index.Production
                                                            {
                                                                Id = p.Id,
                                                                CreatedOn = p.CreatedOn.ForJavascript(),
                                                                Title = p.Title,
                                                                LastChangeDate = p.LastChangeDate.GetValueOrDefault().ForJavascript(),
                                                                OwnerName = p.Owner.MapIfNotNull(o => o.DisplayName, string.Empty),
                                                            });
            //get all the membership requests they can approve/reject
            var membershipRequests = productions.SelectMany(p => p.ChildrenOfType<MembershipRequestThing>().Where(m => !m.IsInvitation)).Where(mr => mr.HasPermission(Permissions.Delete, user)).ToList();

            using (var db = DataModel.ReadOnly)
            {
                membershipRequests.AddRange((from mr in db.MembershipRequests where mr.IsInvitation.GetValueOrDefault() && mr.UserId == user.Id select mr.ThingId).FilterDeletedThings(db).Select(id => Things.GetOrNull<MembershipRequestThing>(id)));
            }
            model.MembershipRequests =
                membershipRequests.Where(t => t != null).Select(
                    t => (MembershipRequestThingDashboardView)t.CreateViewData("Dashboard", this.Identity())).Where(
                        t => t.Creator != null && t.ProductionId > 0).Select(m => new Dashboard_Index.MembershipRequest
                                                                                      {
                                                                                          Id = m.Id,
                                                                                          ProductionId = m.ProductionId,
                                                                                          ProductionTitle = m.ProductionTitle,
                                                                                          UserId = m.UserId,
                                                                                          Email = m.Email,
                                                                                          Name = m.Name,
                                                                                          IsInvitation = m.IsInvitation
                                                                                      }).ToList();

            var actions = productions.Where(c => c.Activity != null).SelectMany(c => c.Activity.Select(asource => new { Source = asource, Id = c.Id })).OrderByDescending(s => s.Source.Id).ToArray();
            int max = 10;
            var activity = new List<ProductionActivityItem>();
            foreach (var currAction in actions)
            {
                var project = Things.GetOrNull<ProjectThing>(currAction.Id);
                if (project == null)
                    continue;

                ThingBase changedThing = null;
                if (currAction.Source.ThingId == project.Id)
                    changedThing = project;
                else
                {
                    changedThing = project.FindDescendantById(currAction.Source.ThingId);
                }
                //in this case the thing was deleted.  so we ignore the change.  the Delete change will maybe be included in the change set or a ancestor was delted
                if (changedThing != null)
                {
                    var contents = changedThing.GetActivityPanelContents(currAction.Source, false, this.Identity());
                    if (contents != null)
                    {
                        activity.Add(contents);
                        if (activity.Count == max)
                            break;
                    }
                }
            }
            model.Activity = activity;

            if (this.Request.IsAjaxRequest())
                return Json(
                    new
                    {
                        Activity = model.Activity,
                        Productions = model.Productions,
                    });

//            if (Request["n"].HasChars())
//                return View("Index2", model);
            return View("Index", model);
        }


        ////crappy temp hack around our routes
        [HttpPost]
        public ActionResult Create()
        {
            return Index(null, null);
        }
    }

    
}
