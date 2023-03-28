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

namespace Takeoff.Views.Account
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Account/Close-AccountHolder.cshtml")]
    public class Close_AccountHolder : System.Web.Mvc.WebViewPage<dynamic>
    {
        public Close_AccountHolder()
        {
        }
        public override void Execute()
        {

            
            #line 1 "..\..\Views\Account\Close-AccountHolder.cshtml"
  
    Layout = "~/Views/Account/Layout.cshtml";
    ViewData["Heading"] = "Close My Account";
    


            
            #line default
            #line hidden
WriteLiteral("\r\n<div class=\"well\">\r\n\t<h3>");


            
            #line 8 "..\..\Views\Account\Close-AccountHolder.cshtml"
 Write("Remain as a Guest");

            
            #line default
            #line hidden
WriteLiteral("</h3>\r\n\t<p>");


            
            #line 9 "..\..\Views\Account\Close-AccountHolder.cshtml"
Write("We will permanently delete all productions you've made. You'll remain on as a guest.  If you are a member of other people's productions, you will remain as one.  Warning: there is no undo!");

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n\t<div>\r\n        <form method=\"post\" action=\"");


            
            #line 11 "..\..\Views\Account\Close-AccountHolder.cshtml"
                               Write(Url.Action("Close", "Account", new { deleteUser = false }));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n            <input class=\"btn btn-danger\" value=\"");


            
            #line 12 "..\..\Views\Account\Close-AccountHolder.cshtml"
                                             Write("Remain a Guest");

            
            #line default
            #line hidden
WriteLiteral("\" type=\"submit\" />\r\n        </form>\r\n\t</div>\r\n</div>\r\n\r\n<div class=\"well\">\r\n    <" +
"h3>");


            
            #line 18 "..\..\Views\Account\Close-AccountHolder.cshtml"
    Write("Delete Everything");

            
            #line default
            #line hidden
WriteLiteral("</h3>\r\n    <p class=\"");


            
            #line 19 "..\..\Views\Account\Close-AccountHolder.cshtml"
         Write(Html.AlertErrorClass());

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 19 "..\..\Views\Account\Close-AccountHolder.cshtml"
                                   Write("Permanently delete everything I'm associated with. I no longer wish to be a part of Takeoff.  Warning: there is no undo!");

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n\r\n    <div>\r\n        <form method=\"post\" action=\"");


            
            #line 22 "..\..\Views\Account\Close-AccountHolder.cshtml"
                               Write(Url.Action("Close", "Account", new { deleteUser = true }));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n            <input class=\"btn btn-danger\" value=\"");


            
            #line 23 "..\..\Views\Account\Close-AccountHolder.cshtml"
                                             Write("Delete Everything");

            
            #line default
            #line hidden
WriteLiteral("\" type=\"submit\" />\r\n        </form>\r\n    </div>\r\n</div>\r\n\r\n");


            
            #line 28 "..\..\Views\Account\Close-AccountHolder.cshtml"
 if (ViewData["IsSubscribed"].ValueOr(false))
{

            
            #line default
            #line hidden
WriteLiteral("    <p>* However you close your account, you will no longer be charged for it.  W" +
"e do not prorate your current bill.</p>\r\n");


            
            #line 31 "..\..\Views\Account\Close-AccountHolder.cshtml"
}

            
            #line default
            #line hidden

        }
    }
}
#pragma warning restore 1591
