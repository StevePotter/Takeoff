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

namespace Takeoff.Views.Referrals
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Referrals/Index.cshtml")]
    public class Index : System.Web.Mvc.WebViewPage<Account_Notifications>
    {
        public Index()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\Referrals\Index.cshtml"
  
    Layout = "~/Views/Account/Layout.cshtml";

    ViewData["Heading"] = "Referrals";   


            
            #line default
            #line hidden

DefineSection("JsDocReady", () => {

WriteLiteral("\r\n    // window.views.Account_Notifications();\r\n//    $(\"#invite-email\").watermar" +
"k(\"Email Address\");\r\n");


});

WriteLiteral("\r\n<h3>");


            
            #line 11 "..\..\Views\Referrals\Index.cshtml"
Write("What is This?");

            
            #line default
            #line hidden
WriteLiteral("</h3>\r\n<p>");


            
            #line 12 "..\..\Views\Referrals\Index.cshtml"
Write("Referrals are an easy way to get Takeoff for free. Refer someone, and if they sign up, you get a Takeoff credit each time they are billed.");

            
            #line default
            #line hidden
WriteLiteral("\r\n <a>Discounts and More Info</a>\r\n</p>\r\n<h3>Refer Someone</h3>\r\n<form id=\"invite" +
"-form\" method=\"post\" action=\"");


            
            #line 16 "..\..\Views\Referrals\Index.cshtml"
                                         Write(Url.Action<ReferralsController>(c => c.Index()));

            
            #line default
            #line hidden
WriteLiteral("\" class=\"form-horizontal\">\r\n    ");


            
            #line 17 "..\..\Views\Referrals\Index.cshtml"
Write(Html.AntiForgeryToken());

            
            #line default
            #line hidden
WriteLiteral("\r\n    <fieldset>\r\n        <div class=\"");


            
            #line 19 "..\..\Views\Referrals\Index.cshtml"
               Write(Html.ControlGroupClass());

            
            #line default
            #line hidden
WriteLiteral(@""">
            <input id=""invite-email"" name=""email"" type=""text"" placeholder=""Email Address"" style=""width: 300px; height: 30px; font-size: 18px;"" />
            <input type=""submit"" class=""btn btn-primary"" value=""Update"" />
        </div>
    </fieldset>
</form>
<h3>My Referrals</h3>
<table cellspacing=""0"" class=""data table table-striped"" id=""referrals"">
    <thead>
        <tr>
            <th>
                ");


            
            #line 30 "..\..\Views\Referrals\Index.cshtml"
            Write("Person");

            
            #line default
            #line hidden
WriteLiteral("\r\n            </th>\r\n            <th>\r\n                ");


            
            #line 33 "..\..\Views\Referrals\Index.cshtml"
            Write("Status");

            
            #line default
            #line hidden
WriteLiteral(@"
            </th>
        </tr>
    </thead>
    <tbody>
        <tr class=""odd"">
            <td class="" sorting_1"">
                Anderson Roseman
            </td>
            <td>
                In Trial
            </td>
        </tr>
        <tr class=""even"">
            <td class="" sorting_1"">
                Bill Frank
            </td>
            <td>
                Active
            </td>
        </tr>
    </tbody>
</table>


<h3>Credits</h3>
<table cellspacing=""0"" class=""data table table-striped"" id=""credits"">
    <thead>
        <tr>
            <th>
                ");


            
            #line 63 "..\..\Views\Referrals\Index.cshtml"
            Write("Person");

            
            #line default
            #line hidden
WriteLiteral("\r\n            </th>\r\n            <th>\r\n                ");


            
            #line 66 "..\..\Views\Referrals\Index.cshtml"
            Write("Date");

            
            #line default
            #line hidden
WriteLiteral("\r\n            </th>\r\n            <th>\r\n                ");


            
            #line 69 "..\..\Views\Referrals\Index.cshtml"
            Write("My Credit");

            
            #line default
            #line hidden
WriteLiteral(@"
            </th>
        </tr>
    </thead>
    <tbody>
        <tr class=""odd"">
            <td>
                Bill Frank
            </td>
            <td>
                6/4/2012
            </td>
            <td>
                $4.99
            </td>
        </tr>
        <tr class=""even"">
            <td>
                Bill Frank
            </td>
            <td>
                5/4/2012
            </td>
            <td>
                $4.99
            </td>
        </tr>
        <tr class=""odd"">
            <td>
                Bill Frank
            </td>
            <td>
                3/4/2012
            </td>
            <td>
                $4.99
            </td>
        </tr>
    </tbody>
</table>
");


        }
    }
}
#pragma warning restore 1591
