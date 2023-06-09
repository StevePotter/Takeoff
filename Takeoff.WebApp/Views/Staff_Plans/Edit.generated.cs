﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Takeoff.Views.Staff_Plans
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
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "1.5.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Staff_Plans/Edit.cshtml")]
    public class Edit : System.Web.Mvc.WebViewPage<Staff_Plans_Plan>
    {
        public Edit()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\Staff_Plans\Edit.cshtml"
  
    Layout = "~/Views/Staff/staff-layout.cshtml";
    
    var intervals = System.Enum.GetValues(typeof(PlanInterval)).Cast<PlanInterval>().Select(value => new SelectListItem
                {
                    Text = value.ToString(),
                    Value = value.ToString()                
                });    


            
            #line default
            #line hidden

DefineSection("Header", () => {

WriteLiteral("\r\n    <h1>");


            
            #line 13 "..\..\Views\Staff_Plans\Edit.cshtml"
    Write("Plans");

            
            #line default
            #line hidden
WriteLiteral("</h1>\r\n");


});

WriteLiteral("\r\n\r\n");


DefineSection("JsDocReady", () => {

WriteLiteral("\r\n    $(\"#mainForm\").madValidate({\r\n        rules:\r\n        {\r\n            \"Id\": " +
"\"required\"\r\n        },\r\n        useAjax: false\r\n    });\r\n");


});

WriteLiteral("\r\n    ");


            
            #line 26 "..\..\Views\Staff_Plans\Edit.cshtml"
Write(Html.ValidationSummary());

            
            #line default
            #line hidden
WriteLiteral("\r\n    <form id=\"mainForm\" method=\"post\" action=\"");


            
            #line 27 "..\..\Views\Staff_Plans\Edit.cshtml"
                                         Write(Url.Action("Edit", "Staff_Plans"));

            
            #line default
            #line hidden
WriteLiteral("\" class=\"form-horizontal\">\r\n        ");


            
            #line 28 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.AntiForgeryToken());

            
            #line default
            #line hidden
WriteLiteral("\r\n        ");


            
            #line 29 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.HiddenFor(m=>m.Id));

            
            #line default
            #line hidden
WriteLiteral("\r\n        ");


            
            #line 30 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.TextBoxControlGroupFor(m => m.Id));

            
            #line default
            #line hidden
WriteLiteral("\r\n        ");


            
            #line 31 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.TextBoxControlGroupFor(m => m.Title));

            
            #line default
            #line hidden
WriteLiteral("\r\n        ");


            
            #line 32 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.TextBoxControlGroupFor(m => m.Notes));

            
            #line default
            #line hidden
WriteLiteral("\r\n        ");


            
            #line 33 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.TextBoxControlGroupFor(m => m.Price));

            
            #line default
            #line hidden
WriteLiteral("\r\n        ");


            
            #line 34 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.ControlGroup(Html.ControlLabelFor(m=>m.AllowSignups), Html.CheckBoxFor(m =>m.AllowSignups)));

            
            #line default
            #line hidden
WriteLiteral("\r\n        <div class=\"");


            
            #line 35 "..\..\Views\Staff_Plans\Edit.cshtml"
               Write(Html.ControlGroupClass());

            
            #line default
            #line hidden
WriteLiteral("\">\r\n            ");


            
            #line 36 "..\..\Views\Staff_Plans\Edit.cshtml"
       Write(Html.ControlLabelFor(m => m.BillingIntervalLength));

            
            #line default
            #line hidden
WriteLiteral("\r\n            ");


            
            #line 37 "..\..\Views\Staff_Plans\Edit.cshtml"
       Write(Html.ControlsWrapperBeginTag());

            
            #line default
            #line hidden
WriteLiteral("\r\n            ");


            
            #line 38 "..\..\Views\Staff_Plans\Edit.cshtml"
       Write(Html.TextBoxFor(m=> m.BillingIntervalLength));

            
            #line default
            #line hidden
WriteLiteral("\r\n            ");


            
            #line 39 "..\..\Views\Staff_Plans\Edit.cshtml"
       Write(Html.DropDownListFor(m => m.BillingIntervalUnit, intervals));

            
            #line default
            #line hidden
WriteLiteral("\r\n            ");


            
            #line 40 "..\..\Views\Staff_Plans\Edit.cshtml"
       Write(Html.ControlsWrapperEndTag());

            
            #line default
            #line hidden
WriteLiteral("\r\n        </div>\r\n        <div class=\"");


            
            #line 42 "..\..\Views\Staff_Plans\Edit.cshtml"
               Write(Html.ControlGroupClass());

            
            #line default
            #line hidden
WriteLiteral("\">\r\n            ");


            
            #line 43 "..\..\Views\Staff_Plans\Edit.cshtml"
       Write(Html.ControlLabelFor(m => m.TrialIntervalLength));

            
            #line default
            #line hidden
WriteLiteral("\r\n            ");


            
            #line 44 "..\..\Views\Staff_Plans\Edit.cshtml"
       Write(Html.ControlsWrapperBeginTag());

            
            #line default
            #line hidden
WriteLiteral("\r\n            ");


            
            #line 45 "..\..\Views\Staff_Plans\Edit.cshtml"
       Write(Html.TextBoxFor(m=> m.TrialIntervalLength));

            
            #line default
            #line hidden
WriteLiteral("\r\n            ");


            
            #line 46 "..\..\Views\Staff_Plans\Edit.cshtml"
       Write(Html.DropDownListFor(m => m.TrialIntervalUnit, intervals));

            
            #line default
            #line hidden
WriteLiteral("\r\n            ");


            
            #line 47 "..\..\Views\Staff_Plans\Edit.cshtml"
       Write(Html.ControlsWrapperEndTag());

            
            #line default
            #line hidden
WriteLiteral("\r\n        </div>\r\n        ");


            
            #line 49 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.TextBoxControlGroupFor(m => m.VideosTotalMaxCount));

            
            #line default
            #line hidden
WriteLiteral("\r\n        ");


            
            #line 50 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.TextBoxControlGroupFor(m => m.VideosPerBillingCycleMax));

            
            #line default
            #line hidden
WriteLiteral("\r\n        ");


            
            #line 51 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.TextBoxControlGroupFor(m => m.VideoFileSizeMax));

            
            #line default
            #line hidden
WriteLiteral("\r\n        ");


            
            #line 52 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.TextBoxControlGroupFor(m => m.VideoDurationLimit, "Max Video Duration (sec)"));

            
            #line default
            #line hidden
WriteLiteral("\r\n        ");


            
            #line 53 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.TextBoxControlGroupFor(m => m.ProductionLimit));

            
            #line default
            #line hidden
WriteLiteral("\r\n        ");


            
            #line 54 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.TextBoxControlGroupFor(m => m.AssetFileSizeMax));

            
            #line default
            #line hidden
WriteLiteral("\r\n        ");


            
            #line 55 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.TextBoxControlGroupFor(m => m.AssetsAllTimeMaxCount));

            
            #line default
            #line hidden
WriteLiteral("\r\n        ");


            
            #line 56 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.TextBoxControlGroupFor(m => m.AssetsTotalMaxCount));

            
            #line default
            #line hidden
WriteLiteral("\r\n        ");


            
            #line 57 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.TextBoxControlGroupFor(m => m.AssetsTotalSizeMax));

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n        ");


            
            #line 59 "..\..\Views\Staff_Plans\Edit.cshtml"
   Write(Html.FormButtons("Update"));

            
            #line default
            #line hidden
WriteLiteral("\r\n    </form>\r\n");


        }
    }
}
#pragma warning restore 1591
