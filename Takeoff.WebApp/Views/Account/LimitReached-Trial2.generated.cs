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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Account/LimitReached-Trial2.cshtml")]
    public class LimitReached_Trial2 : System.Web.Mvc.WebViewPage<Account_LimitReached>
    {
        public LimitReached_Trial2()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\Account\LimitReached-Trial2.cshtml"
  
    Layout = "~/Views/Shared/Layout-App-Slim.cshtml";
    
    ViewData["PageTitle"] = "Limit Reached";


            
            #line default
            #line hidden

DefineSection("Header", () => {

WriteLiteral("\r\n    <h1>You Hit a Limit</h1>\r\n");


});

WriteLiteral("\r\n<p>\r\nSorry but ");


            
            #line 12 "..\..\Views\Account\LimitReached-Trial2.cshtml"
     Write(Model.LimitationMessage);

            
            #line default
            #line hidden
WriteLiteral(".  To continue, you\'ll need to purchase a Takeoff plan.  We hope you do!\r\n</p>\r\n<" +
"div class=\"actions\">\r\n    <a class=\"");


            
            #line 15 "..\..\Views\Account\LimitReached-Trial2.cshtml"
         Write(Html.PrimaryButtonClass());

            
            #line default
            #line hidden
WriteLiteral("\" href=\"");


            
            #line 15 "..\..\Views\Account\LimitReached-Trial2.cshtml"
                                            Write(Url.Action<SignupController>(c => c.Subscribe(null, null)));

            
            #line default
            #line hidden
WriteLiteral("\">Purchase Takeoff</a>\r\n</div>\r\n");


        }
    }
}
#pragma warning restore 1591
