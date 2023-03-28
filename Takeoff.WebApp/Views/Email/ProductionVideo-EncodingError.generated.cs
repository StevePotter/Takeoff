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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Email/ProductionVideo-EncodingError.cshtml")]
    public class ProductionVideo_EncodingError : System.Web.Mvc.WebViewPage<Email_ProductionVideo_EncodingError>
    {
        public ProductionVideo_EncodingError()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\Email\ProductionVideo-EncodingError.cshtml"
  
    Layout = "~/Views/Email/Layout-ProductionBase.cshtml";
    ViewData["IncludeThumbnails"] = false;
    var changeSettingsUrl = Url.Action<AccountNotificationsController>(c => c.Index(), UrlType.AbsoluteHttps);


            
            #line default
            #line hidden

DefineSection("Subject", () => {

WriteLiteral("\r\nProblem With Your Video\r\n");


});

WriteLiteral("\r\n");


DefineSection("HtmlHeader", () => {

WriteLiteral("\r\nProblem with the video you uploaded to ");


            
            #line 12 "..\..\Views\Email\ProductionVideo-EncodingError.cshtml"
                                  Write(Model.ProductionTitle);

            
            #line default
            #line hidden
WriteLiteral("\r\n");


});

WriteLiteral("\r\n");


DefineSection("Plain", () => {

WriteLiteral("\r\nThe video file you uploaded, ");


            
            #line 15 "..\..\Views\Email\ProductionVideo-EncodingError.cshtml"
                        Write(Model.UploadedFileName);

            
            #line default
            #line hidden
WriteLiteral(", which you titled ");


            
            #line 15 "..\..\Views\Email\ProductionVideo-EncodingError.cshtml"
                                                                  Write(Model.VideoTitle);

            
            #line default
            #line hidden
WriteLiteral(", couldn\'t be processed.\r\n\r\n");


            
            #line 17 "..\..\Views\Email\ProductionVideo-EncodingError.cshtml"
 if (Model.Error == Takeoff.Transcoder.TranscodeJobErrorCodes.NotAVideo)
{

            
            #line default
            #line hidden
WriteLiteral("\r\nIt appears as if the file wasn\'t a video.  If you wanted to upload something th" +
"at\'s not a video, please do it in the assets section.\r\n");

WriteLiteral("\r\n");


            
            #line 22 "..\..\Views\Email\ProductionVideo-EncodingError.cshtml"
}
else if (Model.Error == Takeoff.Transcoder.TranscodeJobErrorCodes.NotCompatible)
{

            
            #line default
            #line hidden
WriteLiteral("\r\nOur encoders couldn\'t handle your video format.  We recommend encoding in h.264" +
" or as a MOV.\r\n");

WriteLiteral("\r\n");


            
            #line 28 "..\..\Views\Email\ProductionVideo-EncodingError.cshtml"
}

            
            #line default
            #line hidden

});

WriteLiteral("\r\n<p class=\"text\">The video file you uploaded, ");


            
            #line 30 "..\..\Views\Email\ProductionVideo-EncodingError.cshtml"
                                        Write(Model.UploadedFileName);

            
            #line default
            #line hidden
WriteLiteral(", which you titled ");


            
            #line 30 "..\..\Views\Email\ProductionVideo-EncodingError.cshtml"
                                                                                  Write(Model.VideoTitle);

            
            #line default
            #line hidden
WriteLiteral(", couldn\'t be processed.</p>\r\n\r\n");


            
            #line 32 "..\..\Views\Email\ProductionVideo-EncodingError.cshtml"
 if (Model.Error == Takeoff.Transcoder.TranscodeJobErrorCodes.NotAVideo)
{

            
            #line default
            #line hidden
WriteLiteral("<p class=\"text\">It appears as if the file wasn\'t a video.  If you wanted to uploa" +
"d something that\'s not a video, please do it in the assets section.</p>    \r\n");


            
            #line 35 "..\..\Views\Email\ProductionVideo-EncodingError.cshtml"
}
else if (Model.Error == Takeoff.Transcoder.TranscodeJobErrorCodes.NotCompatible)
{

            
            #line default
            #line hidden
WriteLiteral("<p class=\"text\">Our encoders couldn\'t handle your video format.  We recommend enc" +
"oding in h.264 or as a MOV.</p>    \r\n");


            
            #line 39 "..\..\Views\Email\ProductionVideo-EncodingError.cshtml"
}

            
            #line default
            #line hidden
WriteLiteral("\r\n<p class=\"big-link-container\">\r\n<a href=\"");


            
            #line 42 "..\..\Views\Email\ProductionVideo-EncodingError.cshtml"
    Write(Model.ProductionUrl);

            
            #line default
            #line hidden
WriteLiteral("\">Visit the Production</a>\r\n<div class=\"link-to-copy\">If clicking the above link " +
"didn\'t work, just copy and paste the following into your browser address bar:<br" +
"><b>");


            
            #line 43 "..\..\Views\Email\ProductionVideo-EncodingError.cshtml"
                                                                                                                                    Write(Model.ProductionUrl);

            
            #line default
            #line hidden
WriteLiteral("</b></div>\r\n</p>\r\n");


        }
    }
}
#pragma warning restore 1591
