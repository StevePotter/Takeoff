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

namespace Takeoff.Views.MembershipRequests
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/MembershipRequests/ChooseRoleForApproval.cshtml")]
    public class ChooseRoleForApproval : System.Web.Mvc.WebViewPage<dynamic>
    {
        public ChooseRoleForApproval()
        {
        }
        public override void Execute()
        {

            
            #line 1 "..\..\Views\MembershipRequests\ChooseRoleForApproval.cshtml"
  
    Layout = "~/Views/Shared/Layout-App-Slim.cshtml";
    int RequestId = (int)ViewData["RequestId"];
    this.ViewData.AddBodyCss("chooserole");


            
            #line default
            #line hidden
WriteLiteral("\r\n");


DefineSection("Header", () => {

WriteLiteral("\r\n    <h1>");


            
            #line 9 "..\..\Views\MembershipRequests\ChooseRoleForApproval.cshtml"
    Write("Please Choose a Role for Your New Member");

            
            #line default
            #line hidden
WriteLiteral("</h1>\r\n");


});

WriteLiteral(@"

<div id=""roles"">
<div>
	          		<h3>Client</h3>
	          	    <p>Can add comments and upload files, but cannot add new videos or invite people in. Use this for customers and reviewers.</p>
	          	    <div class=""alert-actions"">
                        <form method=""post"" action=""");


            
            #line 17 "..\..\Views\MembershipRequests\ChooseRoleForApproval.cshtml"
                                               Write(Url.Action("Approve", "MembershipRequests", new { id = RequestId }));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n                        <input type=\"hidden\" name=\"role\" value=\"Client\" />\r\n " +
"                       <input type=\"submit\" value=\"Approve as Client\" class=\"");


            
            #line 19 "..\..\Views\MembershipRequests\ChooseRoleForApproval.cshtml"
                                                                         Write(Html.PrimaryButtonClass());

            
            #line default
            #line hidden
WriteLiteral(@""" />
                        </form>
	          	    </div>
	          	</div>
<div>
	      			<h3>Staff</h3>
	      		    <p>Can add comments, upload files, add new teammates and upload new videos. Use this for editors and contractors.</p>
	      		    <div>
                        <form method=""post"" action=""");


            
            #line 27 "..\..\Views\MembershipRequests\ChooseRoleForApproval.cshtml"
                                               Write(Url.Action("Approve", "MembershipRequests", new { id = RequestId }));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n                        <input type=\"hidden\" name=\"role\" value=\"Staff\" />\r\n  " +
"                      <input type=\"submit\" value=\"Approve as Staff\" class=\"");


            
            #line 29 "..\..\Views\MembershipRequests\ChooseRoleForApproval.cshtml"
                                                                        Write(Html.PrimaryButtonClass());

            
            #line default
            #line hidden
WriteLiteral("\" />\r\n                        </form>\r\n\t      \t\t    </div>\r\n\t      \t\t</div>\r\n</di" +
"v>\r\n");


        }
    }
}
#pragma warning restore 1591
