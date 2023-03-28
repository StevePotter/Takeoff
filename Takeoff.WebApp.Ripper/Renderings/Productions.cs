
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
        private static void RenderViewsForProductions()
        {
            new ViewRenderer<ProductionsController>
            {
                ActionExpr = a => a.Create(null),
                User = User.Guest,
                Model = new Productions_Create
                {
                }
            }.Render();

            new ViewRenderer<ProductionsController>
            {
                ActionExpr = a => a.Create(null),
                User = User.Guest,
                View = "Create.NoAccount",
            }.Render();

            new ViewRenderer<ProductionsController>
            {
                ActionName = "Details",
                View = "Productions-NotFound",
                Scenario = "NotFound_NoIdentity",
            }.Render();

            new ViewRenderer<ProductionsController>
            {
                ActionName = "Details",
                View = "Productions-NotFound",
                User = User.SubscribedFreelance,
                Scenario = "NotFound_Subscriber",
            }.Render();


            //the big one
            new ViewRenderer<ProductionsController>
            {
                ActionName = "Details",
                User = User.Guest,
                Model = new Production_Details
                {
                    EnableChangeSyncing = false,
                    InitialFocus = null,
                    IsMember = true,
                    PreferHtml5Video = true,
                    Data = new ProjectThingView
                    {
                        CanAddComment = true,
                        CanAddFile = true,
                        CanAddMember = true,
                        CanAddVideo = true,
                        Title = "My Production",
                        CreatedOn = DummyData.Date1.ForJavascript(),
                        Files = new FileThingView[] { },
                        Members = new MembershipThingView[]
                                                              {
                                                                  new MembershipThingView
                                                                      {
                                                                       Id   = 4,
                                                                       CreatedOn = DummyData.Date2.ForJavascript(),
                                                                       Creator = User.View1,
                                                                       Email = User.View2.Email,
                                                                       Name = User.View2.Name,
                                                                       UserId = User.View2.Id,
                                                                       
                                                                       
                                                                      }
                                                              },
                        CurrentVideo = new VideoThingDetailView
                        {
                            CanDelete = true,
                            CanEdit = true,
                            IsSourceDownloadable = true,
                            HasVideo = true,
                            HasSource = true,
                            WatchUrl = "http://to-d.s3.amazonaws.com/site-rip-sample.mp4",
                            Creator = User.View1,
                            Title = "Awesomeness",
                            CreatedOn = DummyData.Date4.ForJavascript(),
                            Comments = new CommentThingView[]
                                                                                   {
                                                                                   new VideoCommentThingView
                                                                                       {
                                                                                           StartTime = 8,
                                                                                           Body = "Hey there dude.  This is really great.",
                                                                                           CanDelete = true,
                                                                                           CanEdit = true,
                                                                                           CreatedOn = DummyData.Date5.ForJavascript(),
                                                                                           Creator = User.View2,
                                                                                           Id = 92,
                                                                                           Replies = new CommentThingView[]{},
                                                                                       }
                                                                                   }

                        },
                        MembershipRequests = new MembershipRequestThingView[] { },
                        Creator = new UserView
                        {
                            Email = "mail@mail.com",
                            Name = DummyData.Names[10],
                            Id = 45
                        },
                        Videos = new VideoThingSummaryView[]
                                                             {
                                                                 new VideoThingSummaryView
                                                                     {
                                                                         CreatedOn = DummyData.Date3.ForJavascript(),
                                                                         CanDelete = true,
                                                                         CanEdit = true,
                                                                         Creator = User.View1,
                                                                        HasVideo = true,
                                                                        Id = 4,
                                                                        IsSourceDownloadable = true,
                                                                        Title = "Test Video",
                                                                        Thumbnails = new VideoThumbnailThingView[]
                                                                                         {
                                                                                             new VideoThumbnailThingView
                                                                                                 {
                                                                                                     Id = 4,
                                                                                                     Time = 4,
                                                                                                     Height = 101,
                                                                                                     Width = 180,
                                                                                                     Url = "rip-assets/videothumb-1.jpg"
                                                                                                 },
                                                                                             new VideoThumbnailThingView
                                                                                                 {
                                                                                                     Id = 4,
                                                                                                     Time = 6,
                                                                                                     Height = 101,
                                                                                                     Width = 180,
                                                                                                     Url = "rip-assets/videothumb-2.jpg"
                                                                                                 },
                                                                                             new VideoThumbnailThingView
                                                                                                 {
                                                                                                     Id = 4,
                                                                                                     Time = 8,
                                                                                                     Height = 101,
                                                                                                     Width = 180,
                                                                                                     Url = "rip-assets/videothumb-3.jpg"
                                                                                                 },

                                                                                         }
                                                                         
                                                                     }
                                                             }
                    }
                }
            }.Render();


            new ViewRenderer<ProductionsController>
            {
                ActionExpr = a => a.Login(50, null),
                View = "Production-Login",
                FileName = "Productions_Login.html",
                Model = new Productions_Login
                {
                    ProductionId = 155,
                },
            }.Render();

            new ViewRenderer<ProductionsController>
            {
                ActionExpr = a => a.Login(50, null),
                View = "Production-Login-SemiAnonymousAccess",
                FileName = "Productions_Login_SemiAnonymousAccess.html",
                Model = new Productions_Login
                {
                    ProductionId = 155,
                },
            }.Render();


        }

    }
}
