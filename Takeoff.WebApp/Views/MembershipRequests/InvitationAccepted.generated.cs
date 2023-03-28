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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/MembershipRequests/InvitationAccepted.cshtml")]
    public class InvitationAccepted : System.Web.Mvc.WebViewPage<MembershipRequests_InvitationAccepted>
    {
        public InvitationAccepted()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Views\MembershipRequests\InvitationAccepted.cshtml"
  
    Layout = "~/Views/Shared/Layout-App-Slim.cshtml"; 


            
            #line default
            #line hidden
WriteLiteral("\r\n");


DefineSection("Header", () => {

WriteLiteral("\r\n    <h1>");


            
            #line 8 "..\..\Views\MembershipRequests\InvitationAccepted.cshtml"
    Write("Auto Accept Future Invites?");

            
            #line default
            #line hidden
WriteLiteral("</h1>\r\n");


});

WriteLiteral("\r\n\r\n<p>The invitation was accepted.  Would you like us to automatically accept fu" +
"ture invitations from ");


            
            #line 11 "..\..\Views\MembershipRequests\InvitationAccepted.cshtml"
                                                                                              Write(Model.InvitedByDisplayName);

            
            #line default
            #line hidden
WriteLiteral("?  Otherwise, you\'ll have to accept each invitation from them.</p>\r\n<div class=\"a" +
"ctions\">\r\n    <form method=\"post\" action=\"");


            
            #line 13 "..\..\Views\MembershipRequests\InvitationAccepted.cshtml"
                           Write(Url.Action("SetFutureInvitationsAutoResponse", "MembershipRequests"));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n        <input type=\"hidden\" name=\"userId\" value=\"");


            
            #line 14 "..\..\Views\MembershipRequests\InvitationAccepted.cshtml"
                                             Write(Model.InvitedById);

            
            #line default
            #line hidden
WriteLiteral("\" />\r\n        <input type=\"hidden\" name=\"accept\" value=\"true\" />\r\n        <input " +
"type=\"hidden\" name=\"redirectTo\" value=\"");


            
            #line 16 "..\..\Views\MembershipRequests\InvitationAccepted.cshtml"
                                                 Write(Url.ActionDetails("Productions", Model.ProductionId));

            
            #line default
            #line hidden
WriteLiteral("\" />\r\n        <input type=\"submit\" value=\"Yes\" class=\"");


            
            #line 17 "..\..\Views\MembershipRequests\InvitationAccepted.cshtml"
                                           Write(Html.PrimaryButtonClass());

            
            #line default
            #line hidden
WriteLiteral("\" />\r\n    </form>\r\n    <a href=\"");


            
            #line 19 "..\..\Views\MembershipRequests\InvitationAccepted.cshtml"
        Write(Url.ActionDetails("Productions", Model.ProductionId));

            
            #line default
            #line hidden
WriteLiteral("\" class=\"btn\">No</a>\r\n</div>\r\n");


        }
    }
}
#pragma warning restore 1591
