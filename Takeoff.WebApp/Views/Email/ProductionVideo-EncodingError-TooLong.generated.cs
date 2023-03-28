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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Email/ProductionVideo-EncodingError-TooLong.cshtml")]
    public class ProductionVideo_EncodingError_TooLong : System.Web.Mvc.WebViewPage<Email_ProductionVideo_EncodingError>
    {
        public ProductionVideo_EncodingError_TooLong()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\Email\ProductionVideo-EncodingError-TooLong.cshtml"
  
    Layout = "~/Views/Email/Layout-ProductionBase.cshtml";
    ViewData["IncludeThumbnails"] = false;
    var changeSettingsUrl = Url.Action<AccountNotificationsController>(c => c.Index(), UrlType.AbsoluteHttps);


            
            #line default
            #line hidden

DefineSection("Subject", () => {

WriteLiteral("\r\nVideo Was Too Long\r\n");


});

WriteLiteral("\r\n");


DefineSection("HtmlHeader", () => {

WriteLiteral("\r\nThe video you uploaded to ");


            
            #line 12 "..\..\Views\Email\ProductionVideo-EncodingError-TooLong.cshtml"
                     Write(Model.ProductionTitle);

            
            #line default
            #line hidden
WriteLiteral(" was too long.\r\n");


});

WriteLiteral("\r\n");


DefineSection("Plain", () => {

WriteLiteral("\r\nThe video file you uploaded, ");


            
            #line 15 "..\..\Views\Email\ProductionVideo-EncodingError-TooLong.cshtml"
                        Write(Model.UploadedFileName);

            
            #line default
            #line hidden
WriteLiteral(", which you titled ");


            
            #line 15 "..\..\Views\Email\ProductionVideo-EncodingError-TooLong.cshtml"
                                                                  Write(Model.VideoTitle);

            
            #line default
            #line hidden
WriteLiteral(", was too long.  Currently we allow video durations up to 1 hour.\r\n");


});

WriteLiteral("\r\n\r\n<p class=\"text\">The video file you uploaded, ");


            
            #line 18 "..\..\Views\Email\ProductionVideo-EncodingError-TooLong.cshtml"
                                        Write(Model.UploadedFileName);

            
            #line default
            #line hidden
WriteLiteral(", which you titled ");


            
            #line 18 "..\..\Views\Email\ProductionVideo-EncodingError-TooLong.cshtml"
                                                                                  Write(Model.VideoTitle);

            
            #line default
            #line hidden
WriteLiteral(@", was too long.</p>

<p class=""text"">Currently we allow video durations up to 1 hour.  To fix this, you should break the video up into chunks and upload them separately.  We realize this is probably not what you want to hear, but to keep things running smoothly, we need to have a duration limit.</p>

<p class=""big-link-container"">
<a href=""");


            
            #line 23 "..\..\Views\Email\ProductionVideo-EncodingError-TooLong.cshtml"
    Write(Model.ProductionUrl);

            
            #line default
            #line hidden
WriteLiteral("\">Visit the Production</a>\r\n<div class=\"link-to-copy\">If clicking the above link " +
"didn\'t work, just copy and paste the following into your browser address bar:<br" +
"><b>");


            
            #line 24 "..\..\Views\Email\ProductionVideo-EncodingError-TooLong.cshtml"
                                                                                                                                    Write(Model.ProductionUrl);

            
            #line default
            #line hidden
WriteLiteral("</b></div>\r\n</p>");


        }
    }
}
#pragma warning restore 1591
