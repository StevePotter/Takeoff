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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Account/Billing-Success.cshtml")]
    public class Billing_Success : System.Web.Mvc.WebViewPage<dynamic>
    {
        public Billing_Success()
        {
        }
        public override void Execute()
        {

            
            #line 1 "..\..\Views\Account\Billing-Success.cshtml"
  
    Layout = "~/Views/Account/Layout.cshtml";
    
    ViewData["Heading"] = S.Account_Billing_PageTitle;


            
            #line default
            #line hidden
WriteLiteral("<p class=\"");


            
            #line 6 "..\..\Views\Account\Billing-Success.cshtml"
     Write(Html.AlertSuccessClass());

            
            #line default
            #line hidden
WriteLiteral("\">Your billing information has been updated.  Thank you!</p>\r\n\r\n");


        }
    }
}
#pragma warning restore 1591
