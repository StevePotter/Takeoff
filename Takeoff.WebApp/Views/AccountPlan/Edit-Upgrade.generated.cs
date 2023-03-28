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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/AccountPlan/Edit-Upgrade.cshtml")]
    public class Edit_Upgrade : System.Web.Mvc.WebViewPage<AccountPlan_Edit>
    {
        public Edit_Upgrade()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
  
    Layout = "~/Views/Account/Layout.cshtml";
    
    ViewData["Heading"] = "Upgrade Your Plan";
    var user = (IUser)ViewData["User"];
    var account = user.Account;
    var currentPlan = Model.CurrentPlan;
    var newPlan = (IPlan) Model.NewPlan;
    var newPlanCharge = newPlan.PriceInDollars().ToString(NumberFormat.Currency);


            
            #line default
            #line hidden

            
            #line 12 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
 if (account.IsSubscribed() && !account.InTrialPeriod())
{

            
            #line default
            #line hidden
WriteLiteral("    <p>Awesome! Here\'s how it works: We will issue you a prorated credit for the " +
"remainder of the month for your current plan. Then we start your billing cycle f" +
"resh today with the new plan. So from now on, you will be billed on this day eac" +
"h month.</p>\r\n");


            
            #line 15 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
}
else if (account.IsSubscribed() && account.TrialPeriodEndsOn.HasValue)
{

            
            #line default
            #line hidden
WriteLiteral("    <p>Awesome! Since you\'re still in the trial period, you won\'t pay anything un" +
"til your trial expires, which is on ");


            
            #line 18 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
                                                                                                                Write(account.TrialPeriodEndsOn.Value.ToString(DateTimeFormat.ShortDate));

            
            #line default
            #line hidden
WriteLiteral(".</p>\r\n");


            
            #line 19 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
}
else
{

            
            #line default
            #line hidden
WriteLiteral("    <p>Awesome. We hope you enjoy this plan!</p>\r\n");


            
            #line 23 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
}

            
            #line default
            #line hidden
WriteLiteral("<p>You are currently on the ");


            
            #line 24 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
                       Write(currentPlan.Title);

            
            #line default
            #line hidden
WriteLiteral(" plan, which charges ");


            
            #line 24 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
                                                              Write(currentPlan.PriceInDollars().ToString(NumberFormat.Currency));

            
            #line default
            #line hidden
WriteLiteral("\r\n    every ");


            
            #line 25 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
     Write(currentPlan.BillingIntervalLength);

            
            #line default
            #line hidden
WriteLiteral(" ");


            
            #line 25 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
                                        Write(currentPlan.BillingIntervalType.ToString().ToLowerInvariant().EndWith("(s)"));

            
            #line default
            #line hidden
WriteLiteral(".</p>\r\n<p>You are upgrading to the ");


            
            #line 26 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
                       Write(newPlan.Title);

            
            #line default
            #line hidden
WriteLiteral(" plan, which charges ");


            
            #line 26 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
                                                          Write(newPlan.PriceInDollars().ToString(NumberFormat.Currency));

            
            #line default
            #line hidden
WriteLiteral("\r\n    every ");


            
            #line 27 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
     Write(newPlan.BillingIntervalLength);

            
            #line default
            #line hidden
WriteLiteral(" ");


            
            #line 27 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
                                    Write(newPlan.BillingIntervalType.ToString().ToLowerInvariant().EndWith("(s)"));

            
            #line default
            #line hidden
WriteLiteral(".</p>\r\n<form  method=\"post\" action=\"");


            
            #line 28 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
                         Write(Url.Action<AccountPlanController>(c => c.EditPost(null)));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n<div class=\"well\">\r\n");


            
            #line 30 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
Write(Html.AntiForgeryToken());

            
            #line default
            #line hidden
WriteLiteral("\r\n");


            
            #line 31 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
Write(Html.Hidden("plan", newPlan.Id));

            
            #line default
            #line hidden
WriteLiteral("\r\n    <input class=\"");


            
            #line 32 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
             Write(Html.PrimaryButtonClass());

            
            #line default
            #line hidden
WriteLiteral("\" type=\"submit\" value=\"Upgrade my Account\" />\r\n    <a class=\"btn\" href=\"");


            
            #line 33 "..\..\Views\AccountPlan\Edit-Upgrade.cshtml"
                     Write(Url.Action<AccountPlanController>(c => c.Index()));

            
            #line default
            #line hidden
WriteLiteral("\">Cancel</a>\r\n</div></form>\r\n");


        }
    }
}
#pragma warning restore 1591