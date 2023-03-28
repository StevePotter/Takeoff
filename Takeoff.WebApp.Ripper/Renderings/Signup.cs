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
        private static void RenderViewsForSignup()
        {
            //new ViewRenderer<SignupController>
            //{
            //    ActionExpr = (a => a.Index(null, null, null)),
            //    Description = "Choose a plan.",
            //    View = "Pricing",
            //    Model = Plan.PlansForSale,
            //    AddViewNameToFileName = false,
            //}.Render();

            new ViewRenderer<SignupController>
            {
                ActionExpr = (a => a.Index(null, null, null)),
                View = "Signup-AlreadyDone",
                Scenario = "AlreadyDone",
                Description = "User hits a signup link but they've already signed up.",
                User = User.SubscribedFreelance
            }.Render();

            
            new ViewRenderer<SignupController>
            {
                ActionName = "Guest-Success",
            }.Render();


            ////new ViewRenderer<SignupController>
            ////{
            ////    ActionExpr = (a => a.Account(null)),
            ////    Description = "Enters basic account information.",
            ////    Model = new Signup_Account
            ////    {
            ////        PlanId = "Freelance",
            ////    },
            ////}.Render();

            ////new ViewRenderer<SignupController>
            ////{
            ////    ActionExpr = (a => a.Account(null)),
            ////    Description = "Enters basic account information.",
            ////    Scenario = "Invalid",
            ////    OnRendering = c =>
            ////                      {
            ////                          c.ModelState.AddRequiredError<Signup_Account>((Signup_Account)c.ViewData.Model, m => m.FirstName);
            ////                          c.ModelState.AddRequiredError<Signup_Account>((Signup_Account)c.ViewData.Model, m => m.LastName);
            ////                      },
            ////    Model = new Signup_Account
            ////    {
            ////        PlanId = "Freelance",
            ////        Email = "whatientered@gmail.com",
            ////    },
            ////}.Render();

            //new ViewRenderer<SignupController>
            //{
            //    View = "Account-SubscribeChoice",
            //    Description = "Option to enter a credit card now or later.",
            //    Model = new Signup_Account_SubscribeChoice
            //    {
            //    },
            //}.Render();

            new ViewRenderer<SignupController>
            {
                ActionExpr = (c => c.Index(null, null, null)),
            }.Render();
            new ViewRenderer<SignupController>
            {
                View = "Guest",
            }.Render();
            new ViewRenderer<SignupController>
            {
                View = "Index-TrialUserInfoRequested",
                Model = new Signup_Trial {},
            }.Render();
            new ViewRenderer<SignupController>
            {
                View = "DirectPurchase-ChoosePlan",
            }.Render();
            new ViewRenderer<SignupController>
            {
                View = "DirectPurchase",
                Model = new Signup_DirectPurchase
                                                          {
                                                              PlanId = PlanIds.Solo
                                                          }
            }.Render();

            new ViewRenderer<SignupController>
            {
                ActionExpr = (c => c.Subscribe(null, null)),
                Model = new Signup_Subscription
                {
                    Account = Account.Trial2,
                    Plan = Plan.Freelance,
                }
            }.Render();

            new ViewRenderer<SignupController>
            {
                ActionExpr = (c => c.Subscribe(null, null)),
                Model = new Signup_Subscription
                {
                    Account = Account.TrialFreelanceBeta,
                    Plan = Plan.Freelance,
                },
                Scenario = "TrialBeta",
            }.Render();

            new ViewRenderer<SignupController>
            {
                ActionExpr = (c => c.Subscribe(null, null)),
                Model = new Signup_Subscription
                {
                    Account = Account.TrialExpired,
                    Plan = Plan.Freelance,
                },
                Scenario = "TrialExpired",
            }.Render();


            new ViewRenderer<SignupController>
            {
                ActionExpr = (c => c.Subscribe(null, null)),
                Model = new Signup_Subscription
                {
                    Account = Account.TrialExpiredBeta,
                    Plan = Plan.Freelance,
                },
                Scenario = "TrialExpiredBeta",
            }.Render();


            new ViewRenderer<SignupController>
            {
                ActionExpr = (c => c.Subscribe(null, null)),
                View = "Subscribe-Success",
                Scenario = "Success",
                Model = new Signup_Subscribe_Success { }
            }.Render();

            new ViewRenderer<SignupController>
            {
                ActionExpr = (c => c.Sample()),
                Model = new Signup_Sample
                {

                }
            }.Render();
        }

    }
}
