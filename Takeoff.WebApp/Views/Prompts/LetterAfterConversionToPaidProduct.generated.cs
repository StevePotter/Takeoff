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

namespace Takeoff.Views.Prompts
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Prompts/LetterAfterConversionToPaidProduct.cshtml")]
    public class LetterAfterConversionToPaidProduct : System.Web.Mvc.WebViewPage<ViewPromptViewModel>
    {
        public LetterAfterConversionToPaidProduct()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\Prompts\LetterAfterConversionToPaidProduct.cshtml"
  
    Layout = "~/Views/Shared/Layout-App-Slim.cshtml"; 
    ViewData["PageTitle"] = "Takeoff is Now a Premium Product";
    ViewData.AddBodyCss("prompt");


            
            #line default
            #line hidden
WriteLiteral("\r\n");


DefineSection("Header", () => {

WriteLiteral("\r\n    <h1>Welcome to the New Takeoff</h1>\r\n");


});

WriteLiteral("\r\n\r\n<p>In case you haven\'t heard, Takeoff has come out of its Beta\r\nperiod and is" +
" now a paid product.  We\'ve converted your account to the least\r\nexpensive plan," +
" $9.99/month, which can be changed at any time.  And...</p>\r\n\r\n<h2>You get 20% o" +
"ff for life!</h2>\r\n\r\n<p>Usage until now won\'t count, so don\'t worry about all th" +
"e videos or productions you already have in the system.  You have 30 days\r\n to d" +
"ecide if you want to be a Takeoff customer.  With a paid product, we can invest " +
"more resources into making Takeoff better. Here\'s our plans:</p>\r\n<ul>\r\n<li>Abil" +
"ity to password-protect your productions so clients don\'t need to create an acco" +
"unt.</li>\r\n<li>We are working hard on an iPhone app for\r\nTakeoff.  Expect it in " +
"the app store within a few months.</li>\r\n<li>Visual customizations.  You can rep" +
"lace the Takeoff logo with your own.  Eventually you\'ll be able to change much m" +
"ore.</li>\r\n<li>Other nifty features: data export, login with Facebook or Google," +
"\r\nasset folders, better and faster upload, and simple ways to move videos\r\nthrou" +
"ghout productions.</li>\r\n<li>For you video producers, we are building a Mac desk" +
"top app.  It\'ll\r\nsport rock solid uploads, a slick interface, and possibly drop " +
"folders. Most\r\nimportantly, it\'ll provide integrations with editing programs, st" +
"arting with\r\nFinal Cut Pro.</li>\r\n</ul>\r\n\r\n<p>Keep an eye on our blog, because w" +
"e\'ll be sharing our\r\nprogress as we go.  It is our goal to build the easiest, be" +
"st video collaboration\r\nsystem on the planet.  It can only happen with your supp" +
"ort.  I hope you\'ll\r\njoin us.  Also, I\'d love to hear from you personally, so fe" +
"el free to write me\r\nat any time.</p>\r\n\r\n<p>Thank you from the bottom of my hear" +
"t,</p>\r\n\r\n<p>Chris Kilayko<br>\r\nPresident, Takeoff Inc.<br>\r\n<a href=\"mailto:chr" +
"isk@takeoffvideo.com\">chrisk@takeoffvideo.com</a><br>\r\n800-561-1785 ext 3</p>\r\n\r" +
"\n<div class=\"actions\">\r\n    <a class=\"");


            
            #line 49 "..\..\Views\Prompts\LetterAfterConversionToPaidProduct.cshtml"
         Write(Html.PrimaryButtonClass());

            
            #line default
            #line hidden
WriteLiteral("\" href=\"");


            
            #line 49 "..\..\Views\Prompts\LetterAfterConversionToPaidProduct.cshtml"
                                           Write(Model.MapIfNotNull(m => m.OriginalUrl));

            
            #line default
            #line hidden
WriteLiteral("\">Continue</a>\r\n</div>\r\n\r\n");


        }
    }
}
#pragma warning restore 1591
