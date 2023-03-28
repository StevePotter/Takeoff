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
        private static void RenderViewsForDashboard()
        {
            var productions = new List<Dashboard_Index.Production>();
            var membershipRequests = new List<Dashboard_Index.MembershipRequest>();
            var activity = new List<ProductionActivityItem>();
            var model = new Dashboard_Index
                            {
                                Productions = productions,
                                Activity = activity,
                                MembershipRequests = membershipRequests
                            };

            var renderer = new ViewRenderer<DashboardController>
                               {
                                   ActionExpr = (a => a.Index(null, null)),
                                   Model = model,
                                   Scenario = "Guest",
                                   User = User.Guest,
                                   //this one uses a custom app logo 
                                   FillViewData = (viewData =>
                                                       {
                                                           viewData["AppLogo"] = new AppLogo
                                                                                     {
                                                                                         Url = "rip-assets/account-logo.png",
                                                                                         Width = 83,
                                                                                         Height = 70,
                                                                                     };
                                                       }),
                               }.Render();

            //add a production
            productions.Add(new Dashboard_Index.Production
                                {
                                    CreatedOn = DateTime.Today.ToUniversalTime().ForJavascript(),
                                    Id = 43,
                                    LastChangeDate = DateTime.Now.ToUniversalTime().ForJavascript(),
                                    OwnerName = DummyData.Names[2],
                                    Title = "Your Mom's Production",
                                });
            productions.Add(new Dashboard_Index.Production
                                {
                                    CreatedOn = DateTime.Today.AddDays(-5).ToUniversalTime().ForJavascript(),
                                    Id = 443,
                                    LastChangeDate = DateTime.Now.AddDays(-1).ToUniversalTime().ForJavascript(),
                                    OwnerName = DummyData.Names[1],
                                    Title = "Birds and Bees",
                                });

            var account = new Account
                              {
                                  Status = AccountStatus.Trial,
                                  DaysLeftInTrial = 20,
                                  Plan = Plan.Freelance
                              };
            model.StartupMessageBody = "I'm a startup message!";
            model.Account = account;
            renderer.Scenario = "TrialAccount";
            renderer.Render();
            
            

            model.StartupMessageBody = null;
            model.Account.Status = AccountStatus.Demo;
            renderer.Scenario = "Demo";
            renderer.Render();

            model.Account.Status = AccountStatus.Subscribed;
            membershipRequests.Add(new Dashboard_Index.MembershipRequest
                                       {
                                           IsInvitation = true,
                                           Email = "someone@somewhere.com",
                                           Id = 45,
                                           Name = "Tad Jones",
                                           ProductionId = 44,
                                           ProductionTitle = "My Movie",
                                           UserId = 4
                                       });
            activity.Add(new ProductionActivityItem()
                             {
                                 CssClass = "commented",
                                 Date = DateTime.Today.ToUniversalTime().ForJavascript(),
                                 Html = @"Chris wrote <a href='#'>Hey there world</a> somewhere.",
                             });
            renderer.View = null;
            renderer.Scenario = null;
            //this will be the default view
            renderer.Render();

        }
    }
}
