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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Email/ProductionVideoCommentReply-New.cshtml")]
    public class ProductionVideoCommentReply_New : System.Web.Mvc.WebViewPage<Email_ProductionVideoCommentReply_New>
    {
        public ProductionVideoCommentReply_New()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
  
    Layout = "~/Views/Email/Layout-ProductionBase.cshtml";
    var changeSettingsUrl = Url.Action<AccountNotificationsController>(c => c.Index(), UrlType.AbsoluteHttps);


            
            #line default
            #line hidden

DefineSection("Subject", () => {

WriteLiteral("\r\nNew Reply \'");


            
            #line 8 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
      Write(Model.CommentBody.Truncate(15, StringTruncating.EllipsisCharacter));

            
            #line default
            #line hidden
WriteLiteral("\' in ");


            
            #line 8 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
                                                                              Write(Model.ProductionTitle);

            
            #line default
            #line hidden
WriteLiteral("\r\n");


});

WriteLiteral("\r\n");


DefineSection("HtmlHeader", () => {

WriteLiteral("\r\nNew Reply \'");


            
            #line 11 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
      Write(Model.CommentBody.Truncate(15, StringTruncating.EllipsisCharacter));

            
            #line default
            #line hidden
WriteLiteral("\' in ");


            
            #line 11 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
                                                                              Write(Model.ProductionTitle);

            
            #line default
            #line hidden
WriteLiteral("\r\n");


});

WriteLiteral("\r\n");


DefineSection("HtmlExplanation", () => {

WriteLiteral("\r\nYou are recieving this because you are a member of Takeoff and your settings in" +
"dicate you wish to recieve an email when comments replies are added to productio" +
"ns you belog to.  Visit <a href=\"");


            
            #line 15 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
                                                                                                                                                                                           Write(changeSettingsUrl);

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 15 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
                                                                                                                                                                                                               Write(changeSettingsUrl);

            
            #line default
            #line hidden
WriteLiteral("</a> to change this setting.\r\n");


});

WriteLiteral("\r\n\r\n");


            
            #line 18 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
 if (Model.IsCommentAndCommentReplySameCreator)
{

            
            #line default
            #line hidden
WriteLiteral("    <p class=\"text\">\r\n    ");


            
            #line 21 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
Write(Model.ReplyCreatedBy);

            
            #line default
            #line hidden
WriteLiteral(" replied to a comment he/she wrote in the ");


            
            #line 21 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
                                                              Write(Model.VideoTitle.Surround("\""));

            
            #line default
            #line hidden
WriteLiteral(" video in the ");


            
            #line 21 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
                                                                                                            Write(Model.ProductionTitle.Surround("\""));

            
            #line default
            #line hidden
WriteLiteral(" production.\r\n    </p>\r\n");



WriteLiteral("    <p class=\"video-comment-header\">The original comment:</p>\r\n");


            
            #line 24 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
}
else if (Model.IsRecipientCommentCreator)
{

            
            #line default
            #line hidden
WriteLiteral("    <p class=\"text\">\r\n    ");


            
            #line 28 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
Write(Model.ReplyCreatedBy);

            
            #line default
            #line hidden
WriteLiteral(" replied to a comment you wrote in the ");


            
            #line 28 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
                                                           Write(Model.VideoTitle.Surround("\""));

            
            #line default
            #line hidden
WriteLiteral(" video in the ");


            
            #line 28 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
                                                                                                         Write(Model.ProductionTitle.Surround("\""));

            
            #line default
            #line hidden
WriteLiteral(" production.\r\n    </p>\r\n");



WriteLiteral("    <p class=\"video-comment-header\">Your original comment:</p>\r\n");


            
            #line 31 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
}
else
{

            
            #line default
            #line hidden
WriteLiteral("    <p class=\"text\">\r\n    ");


            
            #line 35 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
Write(Model.ReplyCreatedBy);

            
            #line default
            #line hidden
WriteLiteral(" replied to a comment ");


            
            #line 35 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
                                          Write(Model.CommentCreatedBy);

            
            #line default
            #line hidden
WriteLiteral(" wrote in the ");


            
            #line 35 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
                                                                               Write(Model.VideoTitle.Surround("\""));

            
            #line default
            #line hidden
WriteLiteral(" video in the ");


            
            #line 35 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
                                                                                                                             Write(Model.ProductionTitle.Surround("\""));

            
            #line default
            #line hidden
WriteLiteral(" production.\r\n    </p>\r\n");



WriteLiteral("    <p class=\"video-comment-header\">");


            
            #line 37 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
                               Write(Model.CommentCreatedBy.EndWith("'s"));

            
            #line default
            #line hidden
WriteLiteral(" original comment:</p>\r\n");


            
            #line 38 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
}

            
            #line default
            #line hidden
WriteLiteral("<p class=\"video-comment\">");


            
            #line 39 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
                    Write(Model.CommentBody.Surround("\""));

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n<p class=\"video-comment-header\">The reply:</p>\r\n<p class=\"video-comment-rep" +
"ly\">");


            
            #line 41 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
                          Write(Model.ReplyBody.Surround("\""));

            
            #line default
            #line hidden
WriteLiteral("</p>\r\n\r\n<p class=\"big-link-container\">\r\n<a href=\"");


            
            #line 44 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
    Write(Model.ViewUrl);

            
            #line default
            #line hidden
WriteLiteral("\">See It</a>\r\n<div class=\"link-to-copy\">If clicking the above link didn\'t work, j" +
"ust copy and paste the following into your browser address bar:<br><b>");


            
            #line 45 "..\..\Views\Email\ProductionVideoCommentReply-New.cshtml"
                                                                                                                                    Write(Model.ViewUrl);

            
            #line default
            #line hidden
WriteLiteral("</b></div>\r\n</p>\r\n");


        }
    }
}
#pragma warning restore 1591
