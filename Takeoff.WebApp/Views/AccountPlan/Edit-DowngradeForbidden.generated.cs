﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Takeoff.Views.AccountPlan
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using Mediascend;
    using Mediascend.Web;
    using MvcContrib;
    using Takeoff;
    using Takeoff.Controllers;
    using Takeoff.Data;
    using Takeoff.Models;
    using Takeoff.Mvc;
    using Takeoff.Resources;
    using Takeoff.ViewModels;
    using Takeoff.Views.Helpers;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "1.3.2.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/AccountPlan/Edit-DowngradeForbidden.cshtml")]
    public class Edit_DowngradeForbidden : System.Web.Mvc.WebViewPage<dynamic>
    {
        public Edit_DowngradeForbidden()
        {
        }
        public override void Execute()
        {

            
            #line 1 "..\..\Views\AccountPlan\Edit-DowngradeForbidden.cshtml"
  
    Layout = "~/Views/Account/Layout.cshtml";
    
    ViewData["Heading"] = "Account Plan";


            
            #line default
            #line hidden
WriteLiteral("<div class=\"");


            
            #line 6 "..\..\Views\AccountPlan\Edit-DowngradeForbidden.cshtml"
       Write(Html.AlertErrorClass(true));

            
            #line default
            #line hidden
WriteLiteral("\">\r\nSorry but to avoid abuse, we don\'t allow downgrading if you haven\'t officiall" +
"y subscribed to Takeoff.\r\n<div class=\"alert-actions\">\r\n<a class=\"btn\" href=\"");


            
            #line 9 "..\..\Views\AccountPlan\Edit-DowngradeForbidden.cshtml"
                 Write(Url.Action<SignupController>(c => c.Subscribe(ViewData["PostSubscribeUrl"].ValueOr(string.Empty), null)));

            
            #line default
            #line hidden
WriteLiteral("\">Subscribe Now</a> \r\n</div>\r\n</div>\r\n");


        }
    }
}
#pragma warning restore 1591
