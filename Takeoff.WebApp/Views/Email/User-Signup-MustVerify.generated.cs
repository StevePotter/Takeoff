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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Email/User-Signup-MustVerify.cshtml")]
    public class User_Signup_MustVerify : System.Web.Mvc.WebViewPage<Email_User_Signup_MustVerify>
    {
        public User_Signup_MustVerify()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\Email\User-Signup-MustVerify.cshtml"
  
    Layout = "~/Views/Email/Layout-Simple.cshtml";


            
            #line default
            #line hidden

DefineSection("Subject", () => {

WriteLiteral("\r\nWelcome To Takeoff\r\n");


});

WriteLiteral("\r\n");


DefineSection("Plain", () => {

WriteLiteral(@"
Congratulations, you're signed up for Takeoff.  Takeoff is a private, centralized place for you to share videos, assets, and gather feedback.  We're glad to have you!

Please go to the following link to verify your email so we can unlock certain features.
");


            
            #line 13 "..\..\Views\Email\User-Signup-MustVerify.cshtml"
Write(Model.VerifyUrl.RawHtml());

            
            #line default
            #line hidden
WriteLiteral("\r\n");


});

WriteLiteral("\r\n\r\n");


DefineSection("HtmlExplanation", () => {

WriteLiteral("\r\nYou are receiving this email because someone signed up for Takeoff using this e" +
"mail address.   If you did not sign up for Takeoff, please contact <a href=\"mail" +
"to:support@takeoffvideo.com\">support@takeoffvideo.com</a> and let us know.\r\n");


});

WriteLiteral(@"

<p class=""text"">
Congratulations, you're signed up for Takeoff.  Takeoff is a private, centralized place for you to share videos, assets, and gather feedback.  We're glad to have you!
</p>

<p class=""text"">
In order to unlock certain features, we need to verify this is indeed your email address.  So please click the link below. 
</p>

<div class=""big-link-container"">
<a href=""");


            
            #line 30 "..\..\Views\Email\User-Signup-MustVerify.cshtml"
    Write(Model.VerifyUrl);

            
            #line default
            #line hidden
WriteLiteral("\">Verify My Email</a>\r\n<div class=\"link-to-copy\">If clicking the above link didn\'" +
"t work, just copy and paste the following into your browser address bar:<br><b>");


            
            #line 31 "..\..\Views\Email\User-Signup-MustVerify.cshtml"
                                                                                                                                    Write(Model.VerifyUrl);

            
            #line default
            #line hidden
WriteLiteral("</b></div>\r\n</div>\r\n");


        }
    }
}
#pragma warning restore 1591
