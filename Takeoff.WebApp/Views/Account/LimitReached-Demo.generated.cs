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

namespace Takeoff.Views.Account
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Account/LimitReached-Demo.cshtml")]
    public class LimitReached_Demo : System.Web.Mvc.WebViewPage<Account_LimitReached>
    {
        public LimitReached_Demo()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\Account\LimitReached-Demo.cshtml"
  
    Layout = "~/Views/Shared/Layout-App-Slim.cshtml";
    
    ViewData["PageTitle"] = S.Account_LimitReached_Demo_PageTitle;


            
            #line default
            #line hidden

DefineSection("Header", () => {

WriteLiteral("\r\n    <h1>");


            
            #line 9 "..\..\Views\Account\LimitReached-Demo.cshtml"
   Write(S.Account_LimitReached_Demo_PageHeading);

            
            #line default
            #line hidden
WriteLiteral("</h1>\r\n");


});

WriteLiteral("\r\n<p>\r\n    ");


            
            #line 12 "..\..\Views\Account\LimitReached-Demo.cshtml"
Write(S.Account_LimitReached_Demo_Explanation.FormatResource(Model.LimitationMessage, Html.StartActionLink<SignupController>(c => c.Index(null, null, null))));

            
            #line default
            #line hidden
WriteLiteral("\r\n</p>\r\n<div class=\"actions\">\r\n    <a class=\"");


            
            #line 15 "..\..\Views\Account\LimitReached-Demo.cshtml"
         Write(Html.PrimaryButtonClass());

            
            #line default
            #line hidden
WriteLiteral("\" href=\"");


            
            #line 15 "..\..\Views\Account\LimitReached-Demo.cshtml"
                                            Write(Url.Action<SignupController>(c => c.Index(null, null, null)));

            
            #line default
            #line hidden
WriteLiteral("\">Sign Up</a>\r\n</div>\r\n");


        }
    }
}
#pragma warning restore 1591
