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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Account/PasswordReset-InvalidLink.cshtml")]
    public class PasswordReset_InvalidLink : System.Web.Mvc.WebViewPage<dynamic>
    {
        public PasswordReset_InvalidLink()
        {
        }
        public override void Execute()
        {

            
            #line 1 "..\..\Views\Account\PasswordReset-InvalidLink.cshtml"
  
    Layout = "~/Views/Shared/Layout-App-Slim.cshtml";
    
    ViewData["PageTitle"] = S.PasswordReset_PageTitle;


            
            #line default
            #line hidden

DefineSection("Header", () => {

WriteLiteral("\r\n\t<h1>");


            
            #line 8 "..\..\Views\Account\PasswordReset-InvalidLink.cshtml"
Write(S.PasswordReset_InvalidLinkPageHeading);

            
            #line default
            #line hidden
WriteLiteral("</h1>\r\n");


});

WriteLiteral("\r\n\r\n<p class=\"");


            
            #line 11 "..\..\Views\Account\PasswordReset-InvalidLink.cshtml"
     Write(Html.AlertErrorClass(true));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 11 "..\..\Views\Account\PasswordReset-InvalidLink.cshtml"
                                  Write(S.PasswordReset_InvalidLinkDetails.FormatResource(Html.StartTag("a").Href((Url.Action<AccountController>(c => c.PasswordReset(null))))));

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n\r\n<div class=\"actions\">\r\n<a class=\"");


            
            #line 14 "..\..\Views\Account\PasswordReset-InvalidLink.cshtml"
     Write(Html.PrimaryButtonClass());

            
            #line default
            #line hidden
WriteLiteral("\" href=\"");


            
            #line 14 "..\..\Views\Account\PasswordReset-InvalidLink.cshtml"
                                        Write(Url.Action<AccountController>(c => c.PasswordReset(null)));

            
            #line default
            #line hidden
WriteLiteral("\">Send a New Email</a>\r\n</div>");


        }
    }
}
#pragma warning restore 1591