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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Shared/Account-Trial-SignupRequired.cshtml")]
    public class Account_Trial_SignupRequired : System.Web.Mvc.WebViewPage<Account_LimitReached>
    {
        public Account_Trial_SignupRequired()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\Shared\Account-Trial-SignupRequired.cshtml"
  
    Layout = "~/Views/Shared/Layout-App-Slim.cshtml";
    ViewData["PageTitle"] = "Signup Required";
    
    Html.DisableNagBanners();


            
            #line default
            #line hidden

DefineSection("Header", () => {

WriteLiteral("\r\n    <h1>");


            
            #line 10 "..\..\Views\Shared\Account-Trial-SignupRequired.cshtml"
    Write("Signup Required");

            
            #line default
            #line hidden
WriteLiteral("</h1>\r\n");


});

WriteLiteral("\r\n<p>\r\n");


            
            #line 13 "..\..\Views\Shared\Account-Trial-SignupRequired.cshtml"
Write("Sorry but you must be signed up in order to do this.  Don't worry, it's free and takes only seconds.");

            
            #line default
            #line hidden
WriteLiteral("\r\n</p>\r\n<div class=\"actions\">\r\n    <a class=\"");


            
            #line 16 "..\..\Views\Shared\Account-Trial-SignupRequired.cshtml"
         Write(Html.PrimaryButtonClass());

            
            #line default
            #line hidden
WriteLiteral("\" href=\"");


            
            #line 16 "..\..\Views\Shared\Account-Trial-SignupRequired.cshtml"
                                            Write(Url.Action<SignupController>(c => c.Index(null, null, null)));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 16 "..\..\Views\Shared\Account-Trial-SignupRequired.cshtml"
                                                                                                             Write("Sign Up");

            
            #line default
            #line hidden
WriteLiteral("</a>\r\n</div>\r\n");


        }
    }
}
#pragma warning restore 1591
