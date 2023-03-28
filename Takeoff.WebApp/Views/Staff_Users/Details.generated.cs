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

namespace Takeoff.Views.Staff_Users
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Staff_Users/Details.cshtml")]
    public class Details : System.Web.Mvc.WebViewPage<Staff_Users_Details>
    {
        public Details()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\Staff_Users\Details.cshtml"
  
    Layout = "~/Views/Staff/staff-layout.cshtml";


            
            #line default
            #line hidden
WriteLiteral("\r\n");


DefineSection("Header", () => {

WriteLiteral("\r\n    <h1>");


            
            #line 8 "..\..\Views\Staff_Users\Details.cshtml"
    Write("User #");

            
            #line default
            #line hidden
WriteLiteral(" ");


            
            #line 8 "..\..\Views\Staff_Users\Details.cshtml"
               Write(Model.Id);

            
            #line default
            #line hidden
WriteLiteral("</h1>\r\n");


});

WriteLiteral("\r\n\r\n");


DefineSection("JsDocReady", () => {

WriteLiteral(@"
$(""#impersonate"").click(function (e) {
    var message = ""Do you want to log in as this account owner?  You will be able to do everything they can do.  If you want to come back to the staff section you'll have to log out then back in under your normal account.  Remember impersonating is not an excuse for spying."";
    if (!confirm(message)) {
        e.preventDefault();
        return false;
    }
});
");


});

WriteLiteral("\r\n");


DefineSection("CssInline", () => {

WriteLiteral("\r\n");


});

WriteLiteral("\r\n<div>\r\n<a href=\"");


            
            #line 25 "..\..\Views\Staff_Users\Details.cshtml"
    Write(Url.Action("Edit", "Staff_Users", Model.Id));

            
            #line default
            #line hidden
WriteLiteral("\">Edit User</a> &nbsp; <a href=\"");


            
            #line 25 "..\..\Views\Staff_Users\Details.cshtml"
                                                                                Write(Url.Action("Delete", "Staff_Users", Model.Id));

            
            #line default
            #line hidden
WriteLiteral("\">Delete User</a>&nbsp; <a href=\"");


            
            #line 25 "..\..\Views\Staff_Users\Details.cshtml"
                                                                                                                                                               Write(Url.Action("Impersonate", "Staff_Users", new { id = (int)Model.Id }));

            
            #line default
            #line hidden
WriteLiteral("\">Impersonate</a>\r\n</div>\r\n<div>\r\n    <label>Name:</label> ");


            
            #line 28 "..\..\Views\Staff_Users\Details.cshtml"
                    Write(Model.Name);

            
            #line default
            #line hidden
WriteLiteral("\r\n</div>\r\n<div>\r\n    <label>Email:</label> <a href=\"mailto:");


            
            #line 31 "..\..\Views\Staff_Users\Details.cshtml"
                                     Write(Model.Email);

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 31 "..\..\Views\Staff_Users\Details.cshtml"
                                                   Write(Model.Email);

            
            #line default
            #line hidden
WriteLiteral("</a>\r\n");


            
            #line 32 "..\..\Views\Staff_Users\Details.cshtml"
 if (!@Model.IsVerified)
{

            
            #line default
            #line hidden
WriteLiteral("    <span>Not verified.  Verification link: ");


            
            #line 34 "..\..\Views\Staff_Users\Details.cshtml"
                                       Write(Html.Raw(Url.Action2("Verify", "Account", new { email = Model.Email, verificationKey = Model.VerificationKey }, UrlType.AbsoluteHttps)));

            
            #line default
            #line hidden
WriteLiteral("</span>\r\n");


            
            #line 35 "..\..\Views\Staff_Users\Details.cshtml"
}

            
            #line default
            #line hidden
WriteLiteral("</div>\r\n\r\n");


            
            #line 38 "..\..\Views\Staff_Users\Details.cshtml"
 if (@Model.AccountId.HasValue)
{

            
            #line default
            #line hidden
WriteLiteral("<div>\r\n    <label>Account:</label> <a href=\"");


            
            #line 41 "..\..\Views\Staff_Users\Details.cshtml"
                                Write(Url.ActionDetails("Staff_Accounts", Model.AccountId.Value));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 41 "..\..\Views\Staff_Users\Details.cshtml"
                                                                                             Write(Model.AccountId.Value);

            
            #line default
            #line hidden
WriteLiteral("</a>\r\n</div>\r\n");


            
            #line 43 "..\..\Views\Staff_Users\Details.cshtml"

}
            
            #line default
            #line hidden

        }
    }
}
#pragma warning restore 1591