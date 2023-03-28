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
        private static void RenderViewsForRoot()
        {
            new ViewRenderer<RootController>
                {
                    ActionExpr = (a => a.Index(null))
                }.Render();

            //about is not in the app yet. i wanted to share it with everyone first
            new ViewRenderer<RootController>
                {
                    ActionExpr = (a => a.About())
                }.Render();

            new ViewRenderer<SupportController>
                {
                    ActionExpr = (a => a.Index())
                }.Render();

            new ViewRenderer<RootController>
                {
                    ActionExpr = (a => a.WhyVerify())
                }.Render();
            
            new ViewRenderer<RootController>
            {
                ActionExpr = (a => a.Try())
            }.Render();

            new ViewRenderer<RootController>
                {
                    ActionExpr = (a => a.Privacy())
                }.Render();

            new ViewRenderer<RootController>
                {
                    ActionExpr = (a => a.Pricing()),
                    Model = Plan.PlansForSale,
                }.Render();

            new ViewRenderer<RootController>
                {
                    ActionExpr = (a => a.UserAgreement())
                }.Render();
        }
    }
}
