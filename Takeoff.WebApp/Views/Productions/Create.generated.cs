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

namespace Takeoff.Views.Productions
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Productions/Create.cshtml")]
    public class Create : System.Web.Mvc.WebViewPage<Productions_Create>
    {
        public Create()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\Productions\Create.cshtml"
  
    Layout = "~/Views/Shared/Layout-App-Slim.cshtml";
    

    ViewData["PageTitle"] = S.Productions_Create_PageTitle;


            
            #line default
            #line hidden

DefineSection("JsExternal", () => {

WriteLiteral("\r\n    ");


            
            #line 10 "..\..\Views\Productions\Create.cshtml"
Write(Html.JsLib("app-productions-create.js"));

            
            #line default
            #line hidden
WriteLiteral("\r\n");


});

WriteLiteral("\r\n\r\n");


DefineSection("JsDocReady", () => {

WriteLiteral("\r\n    window.views.Productions_Create();\r\n");


});

WriteLiteral("\r\n\r\n");


DefineSection("Header", () => {

WriteLiteral("\r\n    <h1>");


            
            #line 20 "..\..\Views\Productions\Create.cshtml"
   Write(S.Productions_Create_PageHeading);

            
            #line default
            #line hidden
WriteLiteral("</h1>\r\n");


});

WriteLiteral("\r\n\r\n\r\n");


            
            #line 24 "..\..\Views\Productions\Create.cshtml"
Write(Html.ValidationSummary(@S.Validation_SummaryHeading));

            
            #line default
            #line hidden
WriteLiteral("\r\n<form method=\"post\" class=\"form-horizontal\" action=\"");


            
            #line 25 "..\..\Views\Productions\Create.cshtml"
                                                Write(Url.Action<ProductionsController>(c => c.Create()));

            
            #line default
            #line hidden
WriteLiteral("\"  enctype=\"multipart/form-data\">\r\n    ");


            
            #line 26 "..\..\Views\Productions\Create.cshtml"
Write(Html.AntiForgeryToken());

            
            #line default
            #line hidden
WriteLiteral("\r\n    <fieldset>\r\n        ");


            
            #line 28 "..\..\Views\Productions\Create.cshtml"
   Write(Html.TextBoxControlGroupFor(m => m.Title, S.Productions_Create_Form_Title_Label));

            
            #line default
            #line hidden
WriteLiteral("\r\n        <div class=\"control-group\">\r\n            ");


            
            #line 30 "..\..\Views\Productions\Create.cshtml"
       Write(Html.ControlLabel(S.Productions_Create_Form_Logo_Label, "Logo", "The logo will be shown alongside the production title on the site and in emails.  You can also set an account-wide logo in Account/Logo."));

            
            #line default
            #line hidden
WriteLiteral("\r\n            <div class=\"controls\">\r\n                <input type=\"file\" id=\"Logo" +
"\" name=\"Logo\" size=\"30\" />\r\n            </div>\r\n        </div>\r\n        <div id=" +
"\"CustomUrl-group\" class=\"control-group\">\r\n            ");


            
            #line 36 "..\..\Views\Productions\Create.cshtml"
       Write(Html.ControlLabel("Custom Url", "CustomUrl", "A custom web address for this production makes it easier to find and share.  For example, enter 'mymovie' and this production can be found at http://takeoffvideo.com/mymovie."));

            
            #line default
            #line hidden
WriteLiteral("\r\n            <div class=\"controls\">\r\n                <div class=\"input-prepend\">" +
"\r\n                    <span class=\"add-on\">");


            
            #line 39 "..\..\Views\Productions\Create.cshtml"
                                     Write("http://takeoffvideo.com/");

            
            #line default
            #line hidden
WriteLiteral("</span><input type=\"text\" size=\"16\" id=\"CustomUrl\" name=\"CustomUrl\" class=\"span2\"" +
" />\r\n                </div>\r\n            </div>\r\n        </div>\r\n        <div id" +
"=\"SemiAnonymousLogin-Container\" class=\"control-group\">\r\n            ");


            
            #line 44 "..\..\Views\Productions\Create.cshtml"
       Write(Html.ControlLabel("Guest Password", "SemiAnonymousDecryptedPassword", "Setting this password lets people enter the production without creating a Takeoff account.  You can optionally allow those people to add comments."));

            
            #line default
            #line hidden
WriteLiteral("\r\n            <div class=\"controls\">\r\n                ");


            
            #line 46 "..\..\Views\Productions\Create.cshtml"
           Write(Html.TextBoxFor(m => m.SemiAnonymousDecryptedPassword));

            
            #line default
            #line hidden
WriteLiteral("\r\n                <div id=\"SemiAnonymousUsersCanComment-Container\" style=\"visibil" +
"ity:hidden\">");


            
            #line 47 "..\..\Views\Productions\Create.cshtml"
                                                                                      Write(Html.CheckBoxFor(m => m.SemiAnonymousUsersCanComment));

            
            #line default
            #line hidden
WriteLiteral("Guests can Comment</div>\r\n            </div>\r\n        </div>\r\n        <div class=" +
"\"form-actions\">\r\n            <input type=\"submit\" value=\"Create Production\"  cla" +
"ss=\"btn btn-primary\" /><a href=\"#\" class=\"btn\">");


            
            #line 51 "..\..\Views\Productions\Create.cshtml"
                                                                                                         Write(S.Shared_CancelBtn_Text);

            
            #line default
            #line hidden
WriteLiteral("</a>\r\n        </div>\r\n    </fieldset>\r\n</form>\r\n");


        }
    }
}
#pragma warning restore 1591
