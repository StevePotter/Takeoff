﻿using System;
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
        private static void RenderViewsForTrial()
        {
            //new ViewRenderer<SignupController>
            //{
            //    ActionName = "Trial-Success",
            //    User = User.Trial2SignedUp,
            //    Model = new Signup_Trial_Success
            //                {
            //                    ReturnUrl = "#"
            //                }
            //}.Render();
        }


    }
}
