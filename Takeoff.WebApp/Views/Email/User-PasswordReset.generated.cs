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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Email/User-PasswordReset.cshtml")]
    public class User_PasswordReset : System.Web.Mvc.WebViewPage<Email_User_PasswordReset>
    {
        public User_PasswordReset()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\Email\User-PasswordReset.cshtml"
  
    Layout = "~/Views/Email/Layout-Simple.cshtml";
    var closeUrl = Url.Action<AccountController>(c => c.Close(), UrlType.AbsoluteHttps);


            
            #line default
            #line hidden

DefineSection("Subject", () => {

WriteLiteral("\r\nReset Your Password\r\n");


});

WriteLiteral("\r\n");


DefineSection("Plain", () => {

WriteLiteral(@"
You are receiving this email because a new password was requested for your Takeoff account. If you did not request a new password, ignore this email and continue to use your current password.

Please go to the following link to change your password: 
");


            
            #line 14 "..\..\Views\Email\User-PasswordReset.cshtml"
Write(Model.ResetUrl.RawHtml());

            
            #line default
            #line hidden
WriteLiteral("\r\n");


});

WriteLiteral("\r\n\r\n");


DefineSection("HtmlExplanation", () => {

WriteLiteral(@"
You are recieving this because you are a member of Takeoff and a request was made to reset this password.  Since password resetting is a critical thing, there is no way to shut off these emails without closing your account.  If 
you'd like to close your account, please go to <a href=""");


            
            #line 20 "..\..\Views\Email\User-PasswordReset.cshtml"
                                                   Write(closeUrl);

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 20 "..\..\Views\Email\User-PasswordReset.cshtml"
                                                              Write(closeUrl);

            
            #line default
            #line hidden
WriteLiteral("</a> or email <a href=\"mailto:support@takeoffvideo.com\">support@takeoffvideo.com<" +
"/a> and we\'ll help you.\r\n");


});

WriteLiteral(@"

<p class=""text"">
Someone requested a password reset for your Takeoff account. If you did not make this request, don't worry and simply ignore this email. If you did make this request just click the link below:
</p>

<p class=""big-link-container"">
<a href=""");


            
            #line 28 "..\..\Views\Email\User-PasswordReset.cshtml"
    Write(Model.ResetUrl);

            
            #line default
            #line hidden
WriteLiteral("\">Reset Your Password</a>\r\n<div class=\"link-to-copy\">If clicking the above link d" +
"idn\'t work, just copy and paste the following into your browser address bar:<br>" +
"<b>");


            
            #line 29 "..\..\Views\Email\User-PasswordReset.cshtml"
                                                                                                                                    Write(Model.ResetUrl);

            
            #line default
            #line hidden
WriteLiteral("</b></div>\r\n</p>\r\n");


        }
    }
}
#pragma warning restore 1591