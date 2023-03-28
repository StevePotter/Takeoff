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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Email/Layout-Simple.cshtml")]
    public class Layout_Simple : System.Web.Mvc.WebViewPage<dynamic>
    {
        public Layout_Simple()
        {
        }
        public override void Execute()
        {

            
            #line 1 "..\..\Views\Email\Layout-Simple.cshtml"
  
    Layout = "~/Views/Email/Layout-Base.cshtml";
    this.RedefineSection("Subject");


            
            #line default
            #line hidden

DefineSection("HeadContent", () => {

WriteLiteral("\r\n    ");


            
            #line 7 "..\..\Views\Email\Layout-Simple.cshtml"
Write(RenderSection("HeadContent", false));

            
            #line default
            #line hidden
WriteLiteral("\r\n    <style>\r\n    ");


            
            #line 9 "..\..\Views\Email\Layout-Simple.cshtml"
Write(Html.Raw(typeof(Global).GetEmbeddedResourceString(@"Takeoff.Assets.Email.email.css")));

            
            #line default
            #line hidden
WriteLiteral("\r\n    </style>\r\n");


});

WriteLiteral("\r\n");


DefineSection("Plain", () => {

WriteLiteral("\r\n");


            
            #line 14 "..\..\Views\Email\Layout-Simple.cshtml"
Write(RenderSection("Plain", false));

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n");


            
            #line 16 "..\..\Views\Email\Layout-Simple.cshtml"
Write(Html.Raw(ViewData.ValueOrDefault("ExplanationPlain", string.Empty)));

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\nTakeoff - www.takeoffvideo.com - Simple Video Collaboration\r\n© Copyright ");


            
            #line 19 "..\..\Views\Email\Layout-Simple.cshtml"
       Write(DateTime.Now.Year.ToInvariant());

            
            #line default
            #line hidden
WriteLiteral(" Takeoff, Inc. 218 Denison Street, Unit 1 Highland Park, NJ 08904\r\nQuestions, com" +
"ments?  Email info@takeoffvideo.com\r\nWe\'d love to hear from you.  Email info@tak" +
"eoffvideo.com or call 800-561-1785\r\n");


});

WriteLiteral("\r\n");


DefineSection("Html", () => {

WriteLiteral("\r\n");


            
            #line 25 "..\..\Views\Email\Layout-Simple.cshtml"
 if (IsSectionDefined("HtmlHeader"))
{
    
            
            #line default
            #line hidden
            
            #line 27 "..\..\Views\Email\Layout-Simple.cshtml"
Write(RenderSection("HtmlHeader"));

            
            #line default
            #line hidden
            
            #line 27 "..\..\Views\Email\Layout-Simple.cshtml"
                                           
}

            
            #line default
            #line hidden
WriteLiteral("    <div class=\"main\">\r\n        ");


            
            #line 30 "..\..\Views\Email\Layout-Simple.cshtml"
   Write(RenderBody());

            
            #line default
            #line hidden
WriteLiteral("\r\n    </div>\r\n\r\n");


            
            #line 33 "..\..\Views\Email\Layout-Simple.cshtml"
 if (IsSectionDefined("HtmlExplanation"))
{

            
            #line default
            #line hidden
WriteLiteral("     <div class=\"explanation\">\r\n        ");


            
            #line 36 "..\..\Views\Email\Layout-Simple.cshtml"
   Write(RenderSection("HtmlExplanation"));

            
            #line default
            #line hidden
WriteLiteral("       \r\n     </div>\r\n");


            
            #line 38 "..\..\Views\Email\Layout-Simple.cshtml"
}

            
            #line default
            #line hidden
WriteLiteral("\r\n<div class=\"footer\">Takeoff - <a href=\"http://www.takeoffvideo.com\">www.takeoff" +
"video.com</a> - Simple Video Collaboration<br />\r\n&copy; Copyright ");


            
            #line 41 "..\..\Views\Email\Layout-Simple.cshtml"
            Write(DateTime.Now.Year.ToInvariant());

            
            #line default
            #line hidden
WriteLiteral(" Takeoff, Inc. 218 Denison Street, Unit 1 Highland Park, NJ 08904 <br/>\r\nWe\'d lov" +
"e to hear from you.  Email <a href=\"mailto:info@takeoffvideo.com\">info@takeoffvi" +
"deo.com</a> or call 800-561-1785</div>\r\n");


});


        }
    }
}
#pragma warning restore 1591