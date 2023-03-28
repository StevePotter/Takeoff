using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CommandLine;
using Takeoff.Controllers;
using System.IO;
using System.Web;
using Moq;
using System.Security.Principal;
using System.Collections;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Specialized;
using System.Web.WebPages;
using System.Web.Hosting;
using System.Reflection;
using Ninject;
using System.Threading;
using System.Linq.Expressions;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Resources;
using Takeoff.ThingRepositories;
using Takeoff.ViewModels;

namespace Takeoff.WebApp.Ripper
{
    partial class Program
    {
        private static void RenderViewsForMembershipRequests()
        {

            new ViewRenderer<ProductionsController>
            {
                ActionName = "Details",
                User = User.Guest,
                View = "Video-Invitation",
                Model = new Production_Details_Invitation
                {
                    InvitedByName = User.View1.Name,
                    InvitedByEmail = User.View1.Email,
                    MembershipRequestId = 45,
                    Note = "Please join the awesomeness!",
                    ProductionTitle = "The Awesomest Movie Ever",
                    RequestCreatedOn = DateTime.Parse("5/3/11 9:34 am").ToUniversalTime(),
                    Thumbnails = new VideoThumbnail[]
                                                 {
                                                   new VideoThumbnail
                                                       {
                                                           Height = 101,
                                                           Width = 180,
                                                           Url = "rip-assets/videothumb-1.jpg"
                                                       },
                                                   new VideoThumbnail
                                                       {
                                                           Height = 101,
                                                           Width = 180,
                                                           Url = "rip-assets/videothumb-2.jpg"
                                                       },
                                                   new VideoThumbnail
                                                       {
                                                           Height = 101,
                                                           Width = 180,
                                                           Url = "rip-assets/videothumb-3.jpg"
                                                       },

                                                 },
                }
            }.Render();


            new ViewRenderer<ProductionsController>
            {
                ActionName = "Details",
                User = User.Guest,
                View = "Production-RequestAccess",
                Model = new Productions_Details_RequestAccess
                {
                    ProductionId = 50,
                }
            }.Render();
            new ViewRenderer<ProductionsController>
            {
                ActionName = "Details",
                User = User.Guest,
                View = "Details-MembershipRequestSent",
            }.Render();
            //no requests being taken now
            new ViewRenderer<ProductionsController>
            {
                ActionName = "Details",
                User = User.Guest,
                View = "Production-MembershipRequests-Disabled",
            }.Render();
            //after membership request was created
            new ViewRenderer<MembershipRequestsController>
            {
                ActionName = "Create",
                User = User.Guest,
                View = "Created",
            }.Render();

            new ViewRenderer<MembershipRequestsController>
            {
                ActionName = "Details",
                User = User.Guest,
                View = "ChooseRoleForApproval",
                FillViewData = d =>
                {
                    d["RequestId"] = 1;
                }
            }.Render();

            new ViewRenderer<MembershipRequestsController>
            {
                ActionExpr = c => c.Approve(0, null),
                User = User.TrialFreelance,
                View = "InvitationAccepted",
                Scenario = "AutoAcceptInvites",
                Model = new MembershipRequests_InvitationAccepted
                            {
                                InvitedById = User.SubscribedFreelance.Id,
                                InvitedByDisplayName = User.SubscribedFreelance.DisplayName,
                                ProductionId = 45,
                            }
            }.Render();

            new ViewRenderer<MembershipRequestsController>
            {
                ActionExpr = c => c.Reject(0,null),
                User = User.TrialFreelance,
                View = "InvitationRejected",
                Scenario = "AutoDeclineInvites",
                Model = new MembershipRequests_InvitationRejected
                {
                    InvitedById = User.SubscribedFreelance.Id,
                    InvitedByDisplayName = User.SubscribedFreelance.DisplayName,
                    ProductionId = 45,
                }
            }.Render();


            new ViewRenderer<MembershipRequestsController>
            {
                ActionExpr = c => c.Reject(default(int), null),
                User = User.Guest,
                Model = new MembershipRequests_Reject
                {
                    RequestedById = 56,
                    RequestedByName = "Billy Bob"
                }
            }.Render();

            new ViewRenderer<MembershipRequestsController>
            {
                ActionExpr = c => c.Details(default(int)),
                User = User.Guest,
                Model = new MembershipRequests_Details
                {
                    Note = "You rule!",
                    CreatedOn = DateTime.UtcNow,
                    ProductionId = 45,
                    ProductionTitle = "My production",
                    RequestedByEmail = "asdf@asdf.com",
                    RequestedByName = "Bill Right",
                    RequestId = 4561
                },

            }.Render();
        }
    }
}
