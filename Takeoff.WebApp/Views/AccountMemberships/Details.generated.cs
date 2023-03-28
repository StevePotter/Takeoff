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

namespace Takeoff.Views.AccountMemberships
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/AccountMemberships/Details.cshtml")]
    public class Details : System.Web.Mvc.WebViewPage<AccountMemberships_Details>
    {
        public Details()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\AccountMemberships\Details.cshtml"
  
    Layout = "~/Views/Account/Layout.cshtml";
    
    ViewData["Heading"] = "Membership Settings";


            
            #line default
            #line hidden

DefineSection("JsDocReady", () => {

WriteLiteral("\r\n    $(\"#deleteMemberForm\").submit(function(e)\r\n    {\r\n        if (!confirm(\"Are" +
" you sure you want to do this?  You will be removed from all productions on this" +
" account.\"))\r\n        {\r\n            return false;\r\n        }\r\n    });\r\n");


});

WriteLiteral("\r\n<h3>");


            
            #line 16 "..\..\Views\AccountMemberships\Details.cshtml"
Write("Membership Details");

            
            #line default
            #line hidden
WriteLiteral("</h3>\r\n\r\n<dl class=\"dl-horizontal\">\r\n    <dt>");


            
            #line 19 "..\..\Views\AccountMemberships\Details.cshtml"
    Write("Account Owner Name");

            
            #line default
            #line hidden
WriteLiteral("</dt>\r\n    <dd>");


            
            #line 20 "..\..\Views\AccountMemberships\Details.cshtml"
   Write(Model.AccountOwner.Name);

            
            #line default
            #line hidden
WriteLiteral("</dd>\r\n    <dt>");


            
            #line 21 "..\..\Views\AccountMemberships\Details.cshtml"
    Write("Account Owner Email");

            
            #line default
            #line hidden
WriteLiteral("</dt>\r\n    <dd><a href=\"");


            
            #line 22 "..\..\Views\AccountMemberships\Details.cshtml"
            Write(Html.Attribute("mailto:" + Model.AccountOwner.Email));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 22 "..\..\Views\AccountMemberships\Details.cshtml"
                                                                   Write(Model.AccountOwner.Email);

            
            #line default
            #line hidden
WriteLiteral("</a></dd>\r\n    <dt>");


            
            #line 23 "..\..\Views\AccountMemberships\Details.cshtml"
    Write("Added to Account");

            
            #line default
            #line hidden
WriteLiteral("</dt>\r\n    <dd>");


            
            #line 24 "..\..\Views\AccountMemberships\Details.cshtml"
   Write(Model.JoinedAccountOn.ToString(DateTimeFormat.ShortDateTime));

            
            #line default
            #line hidden
WriteLiteral("</dd>\r\n</dl>\r\n");


            
            #line 26 "..\..\Views\AccountMemberships\Details.cshtml"
 if (Model.Productions.HasItems())
{

            
            #line default
            #line hidden
WriteLiteral("    <h3>");


            
            #line 28 "..\..\Views\AccountMemberships\Details.cshtml"
    Write("Productions");

            
            #line default
            #line hidden
WriteLiteral("</h3>\r\n");



WriteLiteral("    <p>");


            
            #line 29 "..\..\Views\AccountMemberships\Details.cshtml"
   Write("You are a member of the following productions on this account:");

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n");



WriteLiteral("    <ul>\r\n");


            
            #line 31 "..\..\Views\AccountMemberships\Details.cshtml"
         foreach (var production in Model.Productions)
        {

            
            #line default
            #line hidden
WriteLiteral("            <li><a href=\"");


            
            #line 33 "..\..\Views\AccountMemberships\Details.cshtml"
                    Write(Url.ActionDetails("Productions", production.Id));

            
            #line default
            #line hidden
WriteLiteral("\" target=\"_blank\">");


            
            #line 33 "..\..\Views\AccountMemberships\Details.cshtml"
                                                                                      Write(production.Title);

            
            #line default
            #line hidden
WriteLiteral("</a></li>           \r\n");


            
            #line 34 "..\..\Views\AccountMemberships\Details.cshtml"
        }

            
            #line default
            #line hidden
WriteLiteral("    </ul>\r\n");


            
            #line 36 "..\..\Views\AccountMemberships\Details.cshtml"
}

            
            #line default
            #line hidden
WriteLiteral("\r\n<h3>");


            
            #line 38 "..\..\Views\AccountMemberships\Details.cshtml"
Write("Disassociate");

            
            #line default
            #line hidden
WriteLiteral("</h3>\n<p>");


            
            #line 39 "..\..\Views\AccountMemberships\Details.cshtml"
Write("You will be removed from all productions on this account.");

            
            #line default
            #line hidden
WriteLiteral("</p>\n<form method=\"post\" id=\"deleteMemberForm\" action=\"");


            
            #line 40 "..\..\Views\AccountMemberships\Details.cshtml"
                                              Write(Url.Action<AccountMembershipsController>(c => c.Delete(Model.Id)));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n    ");


            
            #line 41 "..\..\Views\AccountMemberships\Details.cshtml"
Write(Html.HiddenFor(m => m.Id));

            
            #line default
            #line hidden
WriteLiteral("\r\n    <input type=\"submit\" class=\"btn btn-danger\" value=\"");


            
            #line 42 "..\..\Views\AccountMemberships\Details.cshtml"
                                                   Write("Disassociate from Account");

            
            #line default
            #line hidden
WriteLiteral("\" />\r\n</form>\n\n");


        }
    }
}
#pragma warning restore 1591