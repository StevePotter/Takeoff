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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Shared/Production-MembershipRequests-Disabled.cshtml")]
    public class Production_MembershipRequests_Disabled : System.Web.Mvc.WebViewPage<dynamic>
    {
        public Production_MembershipRequests_Disabled()
        {
        }
        public override void Execute()
        {

            
            #line 1 "..\..\Views\Shared\Production-MembershipRequests-Disabled.cshtml"
  
    Layout = "~/Views/Shared/Layout-App-Slim.cshtml"; 
    ViewData["PageTitle"] = S.Production_MembershipRequestsDisabled_PageTitle;


            
            #line default
            #line hidden

DefineSection("Header", () => {

WriteLiteral("\r\n    <h1>");


            
            #line 7 "..\..\Views\Shared\Production-MembershipRequests-Disabled.cshtml"
   Write(S.Production_MembershipRequestsDisabled_PageHeading);

            
            #line default
            #line hidden
WriteLiteral("</h1>\r\n");


});

WriteLiteral("\r\n<p class=\"");


            
            #line 9 "..\..\Views\Shared\Production-MembershipRequests-Disabled.cshtml"
     Write(Html.AlertWarningClass(true));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n");


            
            #line 10 "..\..\Views\Shared\Production-MembershipRequests-Disabled.cshtml"
Write(S.Production_MembershipRequestsDisabled_Explanation);

            
            #line default
            #line hidden
WriteLiteral("\r\n</p>");


        }
    }
}
#pragma warning restore 1591
