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

namespace Takeoff.Views.Staff_Accounts
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Staff_Accounts/Index.cshtml")]
    public class Index : System.Web.Mvc.WebViewPage<dynamic>
    {
        public Index()
        {
        }
        public override void Execute()
        {

            
            #line 1 "..\..\Views\Staff_Accounts\Index.cshtml"
  
    Layout = "~/Views/Staff/staff-layout.cshtml";
    ViewData["SubTitle"] = "Accounts";


            
            #line default
            #line hidden
WriteLiteral("\r\n");


DefineSection("JsDocReady", () => {

WriteLiteral("\r\npageAccounts();\r\n");


});

WriteLiteral("\r\n");


DefineSection("Header", () => {

WriteLiteral("\r\n<h1>Accounts</h1>\r\n");


});

WriteLiteral("\r\n\r\n<p>Excludes demo accounts.</p>\r\n<table cellspacing=\"0\" id=\"dataTable\" class=\"" +
"data table table-striped\">\r\n    <thead>\r\n    </thead>\r\n    <tbody>\r\n     </tbody" +
">\r\n</table>\r\n\r\n");


        }
    }
}
#pragma warning restore 1591
