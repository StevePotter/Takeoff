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

namespace Takeoff.Views.AccountMembers
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/AccountMembers/Details.cshtml")]
    public class Details : System.Web.Mvc.WebViewPage<AccountMembers_Details>
    {
        public Details()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\AccountMembers\Details.cshtml"
  
    Layout = "~/Views/Account/Layout.cshtml";
    
    ViewData["Heading"] = "Member Settings";
    
    var updateModel = new AccountMembers_Update
                          {
                          Id = Model.Membership.MembershipId,
                          Role = Model.Membership.Role,
                          };


            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n");


DefineSection("JsDocReady", () => {

WriteLiteral(@"
    $(""#deleteMemberForm"").submit(function(e)
    {
        //in future we should put a dialog with three options:  1) no, nevermind 2) Yes but let them request access in the future  3) Yes and forbid them from requesting access to productions
        if (!confirm(""Are you sure you want to delete this person from your account?  They will not be able to access any productions they are on.""))
        {
            return false;
        }
    });
        
    $(""#roleForm"").madValidate({
        rules:
        {
            role: ""required""
        },
        submitSuccess: function(result) {
            $(""body"").madNotification({ api: true }).success(""The role has been updated."");
        }
    });
    
");


});

WriteLiteral("\r\n\r\n<form id=\"roleForm\" action=\"");


            
            #line 38 "..\..\Views\AccountMembers\Details.cshtml"
                        Write(Url.Action<AccountMembersController>(c => c.Update(null)));

            
            #line default
            #line hidden
WriteLiteral("\" class=\"form-horizontal\">\r\n<fieldset>\r\n\r\n<h3>Member Details</h3>\r\n");


            
            #line 42 "..\..\Views\AccountMembers\Details.cshtml"
Write(Html.Hidden("Id", updateModel));

            
            #line default
            #line hidden
WriteLiteral("\r\n");


            
            #line 43 "..\..\Views\AccountMembers\Details.cshtml"
Write(Html.ControlGroup(Html.ControlLabelFor(m => Model.Membership.MemberName, "Name"), Html.TextBoxFor(m => Model.Membership.MemberName, new { disabled = "disabled"})));

            
            #line default
            #line hidden
WriteLiteral("    \r\n");


            
            #line 44 "..\..\Views\AccountMembers\Details.cshtml"
Write(Html.ControlGroup(Html.ControlLabelFor(m => Model.Membership.MemberEmail, "Email"), Html.TextBoxFor(m => Model.Membership.MemberEmail, new { disabled = "disabled" })));

            
            #line default
            #line hidden
WriteLiteral("    \r\n");


            
            #line 45 "..\..\Views\AccountMembers\Details.cshtml"
Write(Html.ControlGroup(Html.ControlLabelFor(m => Model.Membership.CreatedOn, "Added On"), Html.TextBoxFor(m => Model.Membership.CreatedOn, new { disabled = "disabled" })));

            
            #line default
            #line hidden
WriteLiteral("    \r\n    <div class=\"");


            
            #line 46 "..\..\Views\AccountMembers\Details.cshtml"
           Write(Html.ControlGroupClass());

            
            #line default
            #line hidden
WriteLiteral("\">\r\n        ");


            
            #line 47 "..\..\Views\AccountMembers\Details.cshtml"
   Write(Html.ControlLabel("Role"));

            
            #line default
            #line hidden
WriteLiteral("\r\n        ");


            
            #line 48 "..\..\Views\AccountMembers\Details.cshtml"
   Write(Html.ControlsWrapperBeginTag());

            
            #line default
            #line hidden
WriteLiteral("\r\n        <label class=\"radio\">\r\n            <input id=\"role1\" type=\"radio\" name=" +
"\"role\" value=\"Client\" ");


            
            #line 50 "..\..\Views\AccountMembers\Details.cshtml"
                                                                  Write("Client".Equals(Model.Membership.Role) ? "checked='checked'" : String.Empty);

            
            #line default
            #line hidden
WriteLiteral("  />\r\n            ");


            
            #line 51 "..\..\Views\AccountMembers\Details.cshtml"
        Write("Client");

            
            #line default
            #line hidden
WriteLiteral("\r\n            ");


            
            #line 52 "..\..\Views\AccountMembers\Details.cshtml"
       Write(Html.ControlTooltip("Can add comments and upload files, but cannot add new videos or invite people in. Use this for customers and reviewers."));

            
            #line default
            #line hidden
WriteLiteral("\r\n        </label>\r\n        <label class=\"radio\">\r\n            <input id=\"role1\" " +
"type=\"radio\" name=\"role\" value=\"Staff\" ");


            
            #line 55 "..\..\Views\AccountMembers\Details.cshtml"
                                                                 Write("Staff".Equals(Model.Membership.Role) ? "checked='checked'" : String.Empty);

            
            #line default
            #line hidden
WriteLiteral("  />\r\n            ");


            
            #line 56 "..\..\Views\AccountMembers\Details.cshtml"
        Write("Staff");

            
            #line default
            #line hidden
WriteLiteral("\r\n            ");


            
            #line 57 "..\..\Views\AccountMembers\Details.cshtml"
       Write(Html.ControlTooltip("Can add comments, upload files, add new teammates and upload new videos. Use this for editors and contractors."));

            
            #line default
            #line hidden
WriteLiteral("\r\n        </label>\r\n        ");


            
            #line 59 "..\..\Views\AccountMembers\Details.cshtml"
   Write(Html.ControlsWrapperEndTag());

            
            #line default
            #line hidden
WriteLiteral("\r\n    </div>    \r\n    ");


            
            #line 61 "..\..\Views\AccountMembers\Details.cshtml"
Write(Html.FormButtons("Change Role"));

            
            #line default
            #line hidden
WriteLiteral("\r\n</fieldset>\r\n</form>\r\n\r\n");


            
            #line 65 "..\..\Views\AccountMembers\Details.cshtml"
 if (Model.Productions.HasItems())
    {


            
            #line default
            #line hidden
WriteLiteral("            <h3>");


            
            #line 68 "..\..\Views\AccountMembers\Details.cshtml"
            Write("Productions");

            
            #line default
            #line hidden
WriteLiteral("</h3>\r\n");


            
            #line 69 "..\..\Views\AccountMembers\Details.cshtml"


            
            #line default
            #line hidden
WriteLiteral("        <p>");


            
            #line 70 "..\..\Views\AccountMembers\Details.cshtml"
       Write("This person is a member of the following productions:");

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n");



WriteLiteral("        <ul>\r\n");


            
            #line 72 "..\..\Views\AccountMembers\Details.cshtml"
             foreach (var production in Model.Productions)
                {
            

            
            #line default
            #line hidden
WriteLiteral("                <li><a href=\"");


            
            #line 75 "..\..\Views\AccountMembers\Details.cshtml"
                        Write(Url.ActionDetails("Productions", production.Id));

            
            #line default
            #line hidden
WriteLiteral(" \" target=\"_blank\">\r\n                    ");


            
            #line 76 "..\..\Views\AccountMembers\Details.cshtml"
               Write(production.Title);

            
            #line default
            #line hidden
WriteLiteral("</a></li>\r\n");


            
            #line 77 "..\..\Views\AccountMembers\Details.cshtml"
                }

            
            #line default
            #line hidden
WriteLiteral("        </ul>\r\n");


            
            #line 79 "..\..\Views\AccountMembers\Details.cshtml"

     }

            
            #line default
            #line hidden
WriteLiteral("\r\n<div class=\"");


            
            #line 82 "..\..\Views\AccountMembers\Details.cshtml"
       Write(Html.AlertErrorClass(true));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n\t<h3>Remove Member from Account</h3>\r\n\t<p>This person will be removed from al" +
"l productions.</p>\r\n\t<div class=\"alert-actions\">\r\n    <form method=\"post\" id=\"de" +
"leteMemberForm\" action=\"");


            
            #line 86 "..\..\Views\AccountMembers\Details.cshtml"
                                                  Write(Url.Action<AccountMembersController>(c => c.Delete(Model.Membership.MembershipId)));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n        <input type=\"hidden\" name=\"Id\" value=\"");


            
            #line 87 "..\..\Views\AccountMembers\Details.cshtml"
                                         Write(Model.Membership.MembershipId);

            
            #line default
            #line hidden
WriteLiteral("\" />\r\n        <input type=\"submit\" class=\"btn btn-danger\" value=\"Remove Member fr" +
"om Account\" />\r\n    </form>\r\n    </div>\r\n</div>\r\n");


        }
    }
}
#pragma warning restore 1591
