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

namespace Takeoff.Views.Root
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Root/NoCookies.cshtml")]
    public class NoCookies : System.Web.Mvc.WebViewPage<dynamic>
    {
        public NoCookies()
        {
        }
        public override void Execute()
        {

            
            #line 1 "..\..\Views\Root\NoCookies.cshtml"
  
    Layout = "~/Views/Shared/Layout-App-Slim.cshtml";
    


            
            #line default
            #line hidden

DefineSection("Header", () => {

WriteLiteral("\r\n    <h1>Please Enable Cookies</h1>\r\n");


});

WriteLiteral("\r\n");


DefineSection("JsDocReady", () => {

WriteLiteral("\r\n   $(\"#back-button\").click(function () {\r\n        history.go(-1);\r\n        retu" +
"rn false;\r\n    });\r\n");


});

WriteLiteral("\r\n<p class=\"");


            
            #line 16 "..\..\Views\Root\NoCookies.cshtml"
     Write(Html.AlertErrorClass());

            
            #line default
            #line hidden
WriteLiteral(@""">We need you to enable cookies in your browser in order to use Takeoff.</p>
<p>Cookies are small files on your computer that store settings like login information for various web sites.  Takeoff uses cookies and can't work without them.  Right now you don't have cookies enabled.  Enabling them is easy.  Check out <a href=""http://www.google.com/support/accounts/bin/answer.py?&answer=61416"" target=""_blank"">these instructions</a> from Google (who also uses cookies). </p>
<p>Once you have enabled cookies, please <a href=""#"" id=""back-button"">click here</a> to start using Takeoff.</p>
   ");


        }
    }
}
#pragma warning restore 1591
