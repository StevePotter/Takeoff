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
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "1.3.2.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Shared/RequiresVerification.cshtml")]
    public class RequiresVerification : System.Web.Mvc.WebViewPage<dynamic>
    {
        public RequiresVerification()
        {
        }
        public override void Execute()
        {

            
            #line 1 "..\..\Views\Shared\RequiresVerification.cshtml"
  
    Layout = "~/Views/Shared/Layout-App-Slim.cshtml"; 
    ViewData["PageTitle"] = S.MustVerify_Heading;


            
            #line default
            #line hidden

DefineSection("Header", () => {

WriteLiteral("\r\n    <h1>");


            
            #line 7 "..\..\Views\Shared\RequiresVerification.cshtml"
   Write(S.MustVerify_Heading);

            
            #line default
            #line hidden
WriteLiteral("</h1>\r\n");


});

WriteLiteral("\r\n<p class=\"");


            
            #line 9 "..\..\Views\Shared\RequiresVerification.cshtml"
     Write(Html.AlertErrorClass(true));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 9 "..\..\Views\Shared\RequiresVerification.cshtml"
                                  Write(S.MustVerify_Message);

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n\r\n<div class=\"actions\">\r\n<a class=\"");


            
            #line 12 "..\..\Views\Shared\RequiresVerification.cshtml"
     Write(Html.PrimaryButtonClass());

            
            #line default
            #line hidden
WriteLiteral("\" href=\"");


            
            #line 12 "..\..\Views\Shared\RequiresVerification.cshtml"
                                        Write(Url.Action<AccountController>(c => c.Verify(null)));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 12 "..\..\Views\Shared\RequiresVerification.cshtml"
                                                                                               Write("Email me with a link to verify");

            
            #line default
            #line hidden
WriteLiteral("</a>\r\n</div>\r\n");


        }
    }
}
#pragma warning restore 1591
