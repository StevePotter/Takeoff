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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/AccountPlan/Edit-Downgrade.cshtml")]
    public class Edit_Downgrade : System.Web.Mvc.WebViewPage<AccountPlan_Edit>
    {
        public Edit_Downgrade()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
  
    Layout = "~/Views/Account/Layout.cshtml";
    
    ViewData["Heading"] = "Downgrade Your Plan";
    var user = (IUser)ViewData["User"];
    var account = user.Account;
    var currentPlan = Model.CurrentPlan;
    var newPlan = (IPlan)Model.NewPlan;

    //we don't allow downgrades unless they are subscribed


            
            #line default
            #line hidden
WriteLiteral("\r\n");


            
            #line 14 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
 if (account.InTrialPeriod())
{

            
            #line default
            #line hidden
WriteLiteral("    <p>Since you\'re still in the trial period, you won\'t pay anything until your " +
"trial expires, which is on ");


            
            #line 16 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
                                                                                                       Write(account.TrialPeriodEndsOn.Value.ToString(DateTimeFormat.ShortDate));

            
            #line default
            #line hidden
WriteLiteral(".</p>\r\n");


            
            #line 17 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
}
else
{

            
            #line default
            #line hidden
WriteLiteral("    <p>Are you sure you\'d like to downgrade? Since you have already paid for this" +
" month, downgrades will take place at the beginning of the next month.</p>\r\n");


            
            #line 21 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
}

            
            #line default
            #line hidden
WriteLiteral("<p>You are currently on the ");


            
            #line 22 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
                       Write(currentPlan.Title);

            
            #line default
            #line hidden
WriteLiteral(" plan, which charges ");


            
            #line 22 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
                                                              Write(currentPlan.PriceInDollars().ToString(NumberFormat.Currency));

            
            #line default
            #line hidden
WriteLiteral("\r\n    every ");


            
            #line 23 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
     Write(currentPlan.BillingIntervalLength);

            
            #line default
            #line hidden
WriteLiteral(" ");


            
            #line 23 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
                                        Write(currentPlan.BillingIntervalType.ToString().ToLowerInvariant().EndWith("{s}"));

            
            #line default
            #line hidden
WriteLiteral(".</p>\r\n<p>You are downgrading to the ");


            
            #line 24 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
                         Write(newPlan.Title);

            
            #line default
            #line hidden
WriteLiteral(" plan, which charges ");


            
            #line 24 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
                                                            Write(newPlan.PriceInDollars().ToString(NumberFormat.Currency));

            
            #line default
            #line hidden
WriteLiteral("\r\n    every ");


            
            #line 25 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
     Write(newPlan.BillingIntervalLength);

            
            #line default
            #line hidden
WriteLiteral(" ");


            
            #line 25 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
                                    Write(newPlan.BillingIntervalType.ToString().ToLowerInvariant().EndWith("{s}"));

            
            #line default
            #line hidden
WriteLiteral(".</p>\r\n\r\n<form  method=\"post\" action=\"");


            
            #line 27 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
                         Write(Url.Action<AccountPlanController>(c => c.EditPost(null)));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n<div class=\"well\">\r\n");


            
            #line 29 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
Write(Html.AntiForgeryToken());

            
            #line default
            #line hidden
WriteLiteral("\r\n");


            
            #line 30 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
Write(Html.Hidden("plan", newPlan.Id));

            
            #line default
            #line hidden
WriteLiteral("\r\n    <input type=\"submit\" class=\"");


            
            #line 31 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
                           Write(Html.PrimaryButtonClass());

            
            #line default
            #line hidden
WriteLiteral("\" value=\"Downgrade my Account\" />\r\n    <a class=\"btn\" href=\"");


            
            #line 32 "..\..\Views\AccountPlan\Edit-Downgrade.cshtml"
                     Write(Url.Action<AccountPlanController>(c => c.Index()));

            
            #line default
            #line hidden
WriteLiteral("\">Cancel</a>\r\n</div>\r\n</form>\r\n");


        }
    }
}
#pragma warning restore 1591