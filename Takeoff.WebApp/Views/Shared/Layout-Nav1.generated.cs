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

namespace Takeoff.Views.Shared
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Shared/Layout-Nav.cshtml")]
    public class Layout_Nav : System.Web.Mvc.WebViewPage<dynamic>
    {
        public Layout_Nav()
        {
        }
        public override void Execute()
        {

            
            #line 1 "..\..\Views\Shared\Layout-Nav.cshtml"
  
    var controllerName = ViewData["ControllerName"].CastTo<string>();
    var actionName = ViewData["ActionName"].CastTo<string>();
    var user = ViewData["User"].CastTo<IUser>();
    var identity = ViewData["Identity"].CastTo<Identity>();

    var account = user.MapIfNotNull(u => u.Account);
    var isDemo = account != null && account.Status == AccountStatus.Demo;

    var showExternalLinks = identity == null && ViewData["ShowExternalLinksForVisitors"].ValueOr(false);

    var logo = (AppLogo)ViewData[AppLogoHelper.AppLogoKey];
    int customLogoMarginTop = 0;
    if (logo != null)
    {
        customLogoMarginTop = 10 - (logo.Height / 2);
    }
    const string selectedCssClass = "selected";


            
            #line default
            #line hidden
WriteLiteral("    <div class=\"navbar\" id=\"top-nav\">\r\n    <div class=\"navbar-inner\">\r\n      <div" +
" class=\"container\">\r\n");


            
            #line 23 "..\..\Views\Shared\Layout-Nav.cshtml"
             if (logo == null)
            {

            
            #line default
            #line hidden
WriteLiteral("                <a id=\"logo\" class=\"default\" href=\"");


            
            #line 25 "..\..\Views\Shared\Layout-Nav.cshtml"
                                               Write(Url.Action<RootController>(c => c.Index(null)));

            
            #line default
            #line hidden
WriteLiteral("\"><img src=\"");


            
            #line 25 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                           Write(this.Url.Asset("/Assets/img/brand-logo.png"));

            
            #line default
            #line hidden
WriteLiteral("\" alt=\"");


            
            #line 25 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                                                                               Write(S.Header_Logo_AltText);

            
            #line default
            #line hidden
WriteLiteral("\" title=\"");


            
            #line 25 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                                                                                                              Write(S.Header_Logo_TitleText);

            
            #line default
            #line hidden
WriteLiteral("\" /></a>\r\n");


            
            #line 26 "..\..\Views\Shared\Layout-Nav.cshtml"
            }
            else
            {

            
            #line default
            #line hidden
WriteLiteral("                <a id=\"logo\" class=\"custom\" href=\"");


            
            #line 29 "..\..\Views\Shared\Layout-Nav.cshtml"
                                              Write(Url.Action<RootController>(c => c.Index(null)));

            
            #line default
            #line hidden
WriteLiteral("\" style=\"margin-top: ");


            
            #line 29 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                                   Write(customLogoMarginTop.ToInvariant());

            
            #line default
            #line hidden
WriteLiteral("px\"><img src=\"");


            
            #line 29 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                                                                                   Write(logo.Url);

            
            #line default
            #line hidden
WriteLiteral("\" style=\"width:");


            
            #line 29 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                                                                                                           Write(logo.Width.ToInvariant());

            
            #line default
            #line hidden
WriteLiteral("px;height:");


            
            #line 29 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                                                                                                                                              Write(logo.Height.ToInvariant());

            
            #line default
            #line hidden
WriteLiteral("px;\" alt=\"Logo\"/></a>\r\n");


            
            #line 30 "..\..\Views\Shared\Layout-Nav.cshtml"
            }              

            
            #line default
            #line hidden
WriteLiteral("        <div class=\"nav-collapse\">\r\n          <ul class=\"nav\">\r\n");


            
            #line 33 "..\..\Views\Shared\Layout-Nav.cshtml"
                 if (user != null)
                {

            
            #line default
            #line hidden
WriteLiteral("                    <li id=\"nav-dashboard\"><a href=\"");


            
            #line 35 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                Write(Url.Action<DashboardController>(c => c.Index(null, null)));

            
            #line default
            #line hidden
WriteLiteral("\" ");


            
            #line 35 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                              Write(Html.ClassAttIfController<DashboardController>(selectedCssClass));

            
            #line default
            #line hidden
WriteLiteral(">");


            
            #line 35 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                                                                                                 Write(S.Nav_DashboardBtn_Text);

            
            #line default
            #line hidden
WriteLiteral("</a></li>\r\n");



WriteLiteral("                    <li id=\"nav-account\"><a href=\"");


            
            #line 36 "..\..\Views\Shared\Layout-Nav.cshtml"
                                              Write(Url.Action<AccountController>(c => c.Index()));

            
            #line default
            #line hidden
WriteLiteral("\" ");


            
            #line 36 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                Write(Html.ClassAttIfController<AccountController>(selectedCssClass));

            
            #line default
            #line hidden
WriteLiteral(">");


            
            #line 36 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                                                                                 Write(S.Nav_AccountBtn_Text);

            
            #line default
            #line hidden
WriteLiteral("</a></li>\r\n");


            
            #line 37 "..\..\Views\Shared\Layout-Nav.cshtml"
                    if (account != null)
                    {
                        if (account.Status == AccountStatus.Demo || account.Status == AccountStatus.TrialAnonymous)
                        {

            
            #line default
            #line hidden
WriteLiteral("                        <li id=\"nav-signup\"><a href=\"");


            
            #line 41 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                 Write(Url.Action<SignupController>(c => c.Index(null, null, null)));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 41 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                                 Write(S.Nav_SignupBtn_Text);

            
            #line default
            #line hidden
WriteLiteral("</a></li>                                                    \r\n");


            
            #line 42 "..\..\Views\Shared\Layout-Nav.cshtml"
                        }
                        else if (account.CanPurchase())
                        {

            
            #line default
            #line hidden
WriteLiteral("                        <li id=\"nav-buy\"><a href=\"");


            
            #line 45 "..\..\Views\Shared\Layout-Nav.cshtml"
                                              Write(Url.Action<SignupController>(c => c.Index(null, null, null)));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 45 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                              Write(S.Nav_BuyBtn_Text);

            
            #line default
            #line hidden
WriteLiteral("</a></li>                        \r\n");


            
            #line 46 "..\..\Views\Shared\Layout-Nav.cshtml"
                        }
                    }
                }

            
            #line default
            #line hidden
WriteLiteral("          </ul>\r\n          <ul class=\"nav pull-right\">\r\n");


            
            #line 51 "..\..\Views\Shared\Layout-Nav.cshtml"
                 if (identity == null)
                {

            
            #line default
            #line hidden
WriteLiteral("                    <li id=\"nav-login\"><a href=\"");


            
            #line 53 "..\..\Views\Shared\Layout-Nav.cshtml"
                                            Write(Url.Action<AccountController>(c => c.Login(null)));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 53 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                 Write(S.Nav_LoginBtn_Text);

            
            #line default
            #line hidden
WriteLiteral("</a></li>\r\n");


            
            #line 54 "..\..\Views\Shared\Layout-Nav.cshtml"
                }
                else if (isDemo)
                {                     

            
            #line default
            #line hidden
WriteLiteral("                    <li id=\"nav-logout\"><a href=\"");


            
            #line 57 "..\..\Views\Shared\Layout-Nav.cshtml"
                                             Write(Url.Action<AccountController>(c => c.Logout(null)));

            
            #line default
            #line hidden
WriteLiteral("\">Exit Demo</a></li>\r\n");


            
            #line 58 "..\..\Views\Shared\Layout-Nav.cshtml"
                }
                else if (user != null)
                {

            
            #line default
            #line hidden
WriteLiteral("                    <li id=\"nav-user-recognition\">");


            
            #line 61 "..\..\Views\Shared\Layout-Nav.cshtml"
                                             Write(S.Header_UserRecognition.FormatResource(user.FirstName.CharsOr(user.DisplayName).Truncate(20, StringTruncating.EllipsisCharacter)));

            
            #line default
            #line hidden
WriteLiteral("</li>\r\n");



WriteLiteral("                    <li id=\"nav-logout\"><a href=\"");


            
            #line 62 "..\..\Views\Shared\Layout-Nav.cshtml"
                                             Write(Url.Action<AccountController>(c => c.Logout(null)));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 62 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                   Write(S.Header_LogoutBtn_Text);

            
            #line default
            #line hidden
WriteLiteral("</a></li>\r\n");


            
            #line 63 "..\..\Views\Shared\Layout-Nav.cshtml"
                }
                else if (identity != null && identity is SemiAnonymousUserIdentity)
                {

            
            #line default
            #line hidden
WriteLiteral("                    <li id=\"nav-logout\"><a href=\"");


            
            #line 66 "..\..\Views\Shared\Layout-Nav.cshtml"
                                             Write(Url.Action<AccountController>(c => c.Logout(null)));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 66 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                                                   Write(S.Header_LogoutBtn_Text);

            
            #line default
            #line hidden
WriteLiteral("</a></li>                \r\n");


            
            #line 67 "..\..\Views\Shared\Layout-Nav.cshtml"
                }

            
            #line default
            #line hidden
WriteLiteral(@"                <li class=""dropdown"">
                    <a data-toggle=""dropdown"" class=""dropdown-toggle"" href=""#"">Help <b class=""caret""></b></a>
                    <ul class=""dropdown-menu"">
                    <li><a href=""mailto:support@takeoffvideo.com"">");


            
            #line 71 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                              Write("Email Support");

            
            #line default
            #line hidden
WriteLiteral("</a></li>\r\n                    <li><a href=\"");


            
            #line 72 "..\..\Views\Shared\Layout-Nav.cshtml"
                             Write(Url.Action<RootController>(c => c.Contact()));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 72 "..\..\Views\Shared\Layout-Nav.cshtml"
                                                                              Write("Contact Info");

            
            #line default
            #line hidden
WriteLiteral("</a></li>\r\n                    </ul>\r\n               </li>\r\n\r\n          </ul>\r\n  " +
"      </div><!-- /.nav-collapse -->\r\n      </div>\r\n    </div><!-- /navbar-inner " +
"-->\r\n  </div>\r\n");


        }
    }
}
#pragma warning restore 1591