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

namespace Takeoff.Views.Email
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Email/ProductionMember-RequestByNonMember.cshtml")]
    public class ProductionMember_RequestByNonMember : System.Web.Mvc.WebViewPage<Email_MembershipRequestToProduction>
    {
        public ProductionMember_RequestByNonMember()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
  
    Layout = "~/Views/Email/Layout-ProductionBase.cshtml";
    var changeSettingsUrl = Url.Action<AccountController>(c => c.Privacy(), UrlType.AbsoluteHttps);


            
            #line default
            #line hidden

DefineSection("Subject", () => {

WriteLiteral("\r\nI Want to Join ");


            
            #line 8 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
          Write(Model.ProductionTitle);

            
            #line default
            #line hidden
WriteLiteral("\r\n");


});

WriteLiteral("\r\n");


DefineSection("HtmlHeader", () => {

WriteLiteral("\r\n");


            
            #line 11 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
Write(Model.RequestFromName);

            
            #line default
            #line hidden
WriteLiteral(" would like to join ");


            
            #line 11 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
                                     Write(Model.ProductionTitle);

            
            #line default
            #line hidden
WriteLiteral("\r\n");


});

WriteLiteral("\r\n");


DefineSection("HtmlExplanation", () => {

WriteLiteral("\r\nYou are recieving this because you are a member of Takeoff and your settings al" +
"low other users to request membership to your productions.  Visit <a href=\"");


            
            #line 15 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
                                                                                                                                                     Write(changeSettingsUrl);

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 15 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
                                                                                                                                                                         Write(changeSettingsUrl);

            
            #line default
            #line hidden
WriteLiteral("</a> to change this setting.\r\n");


});

WriteLiteral("\r\n");


DefineSection("Plain", () => {

WriteLiteral("\r\n");


            
            #line 18 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
Write(Model.RequestFromName);

            
            #line default
            #line hidden
WriteLiteral(" would like you to join ");


            
            #line 18 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
                                         Write(Model.ProductionTitle);

            
            #line default
            #line hidden
WriteLiteral(".  To view it, please go to the following link:\r\n");


            
            #line 19 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
Write(Html.Raw(Model.ProductionUrl));

            
            #line default
            #line hidden
WriteLiteral("\r\n");


            
            #line 20 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
 if (Model.Note.HasChars())
{
            
            #line default
            #line hidden
WriteLiteral("    \r\nHere\'s a ");


            
            #line 22 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
    Write(Html.Raw(Model.RequestFromName));

            
            #line default
            #line hidden
WriteLiteral(" wrote to you:\r\n");


            
            #line 23 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
Write(Html.Raw(Model.Note.Indent()));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

WriteLiteral("\r\n");


            
            #line 25 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
}

            
            #line default
            #line hidden

});

WriteLiteral("\r\n\r\n");


            
            #line 28 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
 if (Model.Note.HasChars())
{

            
            #line default
            #line hidden
WriteLiteral("<p class=\"text\">Here\'s a note ");


            
            #line 30 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
                         Write(Model.RequestFromName);

            
            #line default
            #line hidden
WriteLiteral(" wrote for you:<br/><span class=\"video-notes\">");


            
            #line 30 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
                                                                                             Write(Model.Note);

            
            #line default
            #line hidden
WriteLiteral("</span></p>\r\n");


            
            #line 31 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
}

            
            #line default
            #line hidden
WriteLiteral("\r\n<p class=\"big-link-container\">\r\n<a href=\"");


            
            #line 34 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
    Write(Model.RequestUrl);

            
            #line default
            #line hidden
WriteLiteral("\">Accept or Decline the Request</a>\r\n<div class=\"link-to-copy\">If clicking the ab" +
"ove link didn\'t work, just copy and paste the following into your browser addres" +
"s bar:<br><b>");


            
            #line 35 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
                                                                                                                                    Write(Model.ProductionUrl);

            
            #line default
            #line hidden
WriteLiteral("</b></div>\r\n</p>\r\n\r\n<p class=\"text\">You can reach ");


            
            #line 38 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
                         Write(Model.RequestFromName);

            
            #line default
            #line hidden
WriteLiteral(" via email at <a href=\"mailto:");


            
            #line 38 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
                                                                             Write(Model.RequestFromEmail);

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 38 "..\..\Views\Email\ProductionMember-RequestByNonMember.cshtml"
                                                                                                      Write(Model.RequestFromEmail);

            
            #line default
            #line hidden
WriteLiteral("</a>, or by replying to this message.</p>\r\n");


        }
    }
}
#pragma warning restore 1591
