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

        private static void RenderSharedViews()
        {
            new ViewRenderer<RootController>
            {
                View = "AlreadyLoggedIn",
                IsSharedView = true,
                Model = User.TrialFreelance,
            }.Render();


            new ViewRenderer<RootController>
            {
                View = "DemoForbidden",
                IsSharedView = true,
            }.Render();

            new ViewRenderer<RootController>
            {
                View = "Account-Required",
                IsSharedView = true,
            }.Render();

            new ViewRenderer<RootController>
            {
                View = "NoCookies",
                IsSharedView = true,
            }.Render();


            new ViewRenderer<RootController>
            {
                View = "InternetExplorerOldWarning",
                IsSharedView = true,
            }.Render();

            new ViewRenderer<RootController>
            {
                View = "LinkDoesNotApply",
                IsSharedView = true,
            }.Render();

            new ViewRenderer<RootController>
            {
                View = "Account-Suspended-NotOwner",
                IsSharedView = true,
            }.Render();

            new ViewRenderer<RootController>
            {
                View = "Account-Suspended",
                IsSharedView = true,
            }.Render();

            new ViewRenderer<RootController>
            {
                View = "RequiresVerification",
                IsSharedView = true,
            }.Render();

            new ViewRenderer<RootController>
            {
                View = "SuccessMessage",
                IsSharedView = true,
                Model = new Message
                {
                    Heading = "Test message",
                    PageTitle = "Message Page Title",
                    Text = "Got some text here for ya"
                }
            }.Render();

            RenderLimitsReached();
        }

        private static void RenderLimitsReached()
        {
            new ViewRenderer<AccountController>
                {
                    ActionExpr = a => a.LimitReached(null, null),
                    User = User.Demo,
                    View = "LimitReached-Demo",
                    Model = new Account_LimitReached {LimitationMessage = S.Account_LimitReachedMsg_ProductionCount},
                }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.LimitReached(null, null),
                View = "LimitReached-NotOwner",
                Model = new Account_LimitReached { LimitationMessage = S.Account_LimitReachedMsg_ProductionCount },
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.LimitReached(null, null),
                User = User.Trial2Anonymous,
                View = "LimitReached-TrialAnonymous",
                Model = new Account_LimitReached { LimitationMessage = S.Account_LimitReachedMsg_ProductionCount },
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.LimitReached(null, null),
                User = User.Trial2SignedUp,
                View = "LimitReached-Trial2",
                Model = new Account_LimitReached { LimitationMessage = S.Account_LimitReachedMsg_ProductionCount },
            }.Render();



            new ViewRenderer<AccountController>
                {
                    ActionExpr = a => a.LimitReached(null, null),
                    User = User.SubscribedFreelance,
                    Model = new Account_LimitReached {LimitationMessage = S.Account_LimitReachedMsg_ProductionCount},
                }.Render();

            new ViewRenderer<AccountController>
                {
                    ActionExpr = a => a.LimitReached(null, null),
                    User = User.Demo,
                    View = "LimitReached-NotOwner",
                    Model = new Account_LimitReached {LimitationMessage = S.Account_LimitReachedMsg_ProductionCount},
                }.Render();
        }
    }
}
