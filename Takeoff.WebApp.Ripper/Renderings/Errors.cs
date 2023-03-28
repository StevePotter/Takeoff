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
        private static void RenderViewsForErrors()
        {
            new ViewRenderer<RootController>
                {
                    View = "Error.ThingNotFound",
                    IsSharedView = true,
                    FileName = "Error_ThingNotFound.html",
                }.Render();

            new ViewRenderer<RootController>
                {
                    View = "Error.NoPermission",
                    IsSharedView = true,
                    FileName = "Error_NoPermission.html",
                }.Render();

            new ViewRenderer<RootController>
                {
                    View = "Error-Billing",
                    IsSharedView = true,
                    FileName = "Error_Billing.html",
                }.Render();

            new ViewRenderer<RootController>
                {
                    View = "Error",
                    IsSharedView = true,
                    FileName = "Error.html",
                }.Render();

            new ViewRenderer<ErrorController>
                {
                    ActionExpr = c => c.PageNotFound(),
                }.Render();
        }
    }
}
