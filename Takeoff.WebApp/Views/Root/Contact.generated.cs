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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Root/Contact.cshtml")]
    public class Contact : System.Web.Mvc.WebViewPage<dynamic>
    {
        public Contact()
        {
        }
        public override void Execute()
        {

            
            #line 1 "..\..\Views\Root\Contact.cshtml"
  
    Layout = "~/Views/Shared/Layout-External-Slim.cshtml";
    
    ViewData["PageTitle"] = "Contact Us";


            
            #line default
            #line hidden

DefineSection("JsDocReady", () => {

WriteLiteral("\r\n    $(\"#mainForm\").madValidate({ useAjax: false }); \r\n    $(\"#timezoneOffset\")." +
"val(new Date().getTimezoneOffset());\r\n");


});

WriteLiteral("\r\n");


DefineSection("Header", () => {

WriteLiteral("\r\n\t<h1>");


            
            #line 13 "..\..\Views\Root\Contact.cshtml"
 Write("Contact Us");

            
            #line default
            #line hidden
WriteLiteral("</h1>\r\n");


});

WriteLiteral("\r\n<address>\r\n    30 Spear St<br/>\r\n    Metuchen, NJ 08840</br>\r\n    (888) 551-246" +
"4\r\n</address>\r\n\r\n<dl class=\"dl-horizontal\">\r\n    <dt>");


            
            #line 22 "..\..\Views\Root\Contact.cshtml"
    Write("Support");

            
            #line default
            #line hidden
WriteLiteral("</dt>\r\n    <dd><a href=\"mailto:support@takeoffvideo.com\">support@takeoffvideo.com" +
"</a></dd>\r\n    <dt>");


            
            #line 24 "..\..\Views\Root\Contact.cshtml"
    Write("Sales");

            
            #line default
            #line hidden
WriteLiteral("</dt>\r\n    <dd><a href=\"mailto:sales@takeoffvideo.com\">sales@takeoffvideo.com</a>" +
"</dd>\r\n    <dt>");


            
            #line 26 "..\..\Views\Root\Contact.cshtml"
    Write("Stephen Potter, CEO");

            
            #line default
            #line hidden
WriteLiteral("</dt>\r\n    <dd><a href=\"mailto:stephenp@takeoffvideo.com\">stephenp@takeoffvideo.c" +
"om</a></dd>\r\n    <dt>");


            
            #line 28 "..\..\Views\Root\Contact.cshtml"
    Write("Chris Kilayko, President");

            
            #line default
            #line hidden
WriteLiteral("</dt>\r\n    <dd><a href=\"mailto:chrisk@takeoffvideo.com\">chrisk@takeoffvideo.com</" +
"a></dd>\r\n</dl>\r\n");


        }
    }
}
#pragma warning restore 1591
