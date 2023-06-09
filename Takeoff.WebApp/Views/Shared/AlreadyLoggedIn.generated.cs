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

namespace Takeoff.Views.Shared
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Shared/AlreadyLoggedIn.cshtml")]
    public class AlreadyLoggedIn : System.Web.Mvc.WebViewPage<IUser>
    {
        public AlreadyLoggedIn()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\Shared\AlreadyLoggedIn.cshtml"
  
    Layout = "~/Views/Shared/Layout-App-Slim.cshtml";    


            
            #line default
            #line hidden

DefineSection("Header", () => {

WriteLiteral("\r\n    <h1>");


            
            #line 7 "..\..\Views\Shared\AlreadyLoggedIn.cshtml"
    Write("You'll Need to Log Out");

            
            #line default
            #line hidden
WriteLiteral("</h1>\r\n");


});

WriteLiteral("\r\n<p class=\"");


            
            #line 9 "..\..\Views\Shared\AlreadyLoggedIn.cshtml"
     Write(Html.AlertWarningClass());

            
            #line default
            #line hidden
WriteLiteral("\">\r\n");


            
            #line 10 "..\..\Views\Shared\AlreadyLoggedIn.cshtml"
Write("Sorry but you can't do this while you are logged in.");

            
            #line default
            #line hidden
WriteLiteral("\r\n</p>\r\n<div class=\"actions\">\r\n<a href=\"");


            
            #line 13 "..\..\Views\Shared\AlreadyLoggedIn.cshtml"
     Write(Url.Action<AccountController>(c => c.Logout(null)));

            
            #line default
            #line hidden
WriteLiteral("\" class=\"btn\">");


            
            #line 13 "..\..\Views\Shared\AlreadyLoggedIn.cshtml"
                                                                        Write("Sign Out");

            
            #line default
            #line hidden
WriteLiteral("</a>\r\n<a href=\"");


            
            #line 14 "..\..\Views\Shared\AlreadyLoggedIn.cshtml"
    Write(Url.Action("Index", "Dashboard"));

            
            #line default
            #line hidden
WriteLiteral("\" class=\"btn\">");


            
            #line 14 "..\..\Views\Shared\AlreadyLoggedIn.cshtml"
                                                    Write("Visit Dashboard");

            
            #line default
            #line hidden
WriteLiteral("</a>\r\n</div>");


        }
    }
}
#pragma warning restore 1591
