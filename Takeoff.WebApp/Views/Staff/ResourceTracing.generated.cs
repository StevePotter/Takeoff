﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASP
{
    using System;
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
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Staff/ResourceTracing.cshtml")]
    public partial class _Views_Staff_ResourceTracing_cshtml : System.Web.Mvc.WebViewPage<Staff_ResourceTracing>
    {
        public _Views_Staff_ResourceTracing_cshtml()
        {
        }
        public override void Execute()
        {
            
            #line 2 "..\..\Views\Staff\ResourceTracing.cshtml"
  
    ViewBag.Title = "Resource Tracing";
    Layout = "~/Views/Staff/staff-layout.cshtml";

            
            #line default
            #line hidden
WriteLiteral("\r\n<p>This will change a setting that will turn on a level of resource tracing.</p" +
">\r\n<form");

WriteLiteral(" id=\"mainForm\"");

WriteLiteral(" method=\"post\"");

WriteAttribute("action", Tuple.Create(" action=\"", 244), Tuple.Create("\"", 292)
            
            #line 7 "..\..\Views\Staff\ResourceTracing.cshtml"
, Tuple.Create(Tuple.Create("", 253), Tuple.Create<System.Object, System.Int32>(Url.Action("ResourceTracing", "Staff")
            
            #line default
            #line hidden
, 253), false)
);

WriteLiteral(">\r\n<div");

WriteLiteral(" class=\"form-field\"");

WriteLiteral(">\r\n    <label>Level</label>\r\n");

WriteLiteral("    ");

            
            #line 10 "..\..\Views\Staff\ResourceTracing.cshtml"
Write(Html.EnumDropDownListFor(m => m.ResourceTraceLevel));

            
            #line default
            #line hidden
WriteLiteral("\r\n</div>\r\n<div");

WriteLiteral(" class=\"form-field\"");

WriteLiteral(">\r\n    <input");

WriteAttribute("value", Tuple.Create(" value=\"", 450), Tuple.Create("\"", 482)
            
            #line 13 "..\..\Views\Staff\ResourceTracing.cshtml"
, Tuple.Create(Tuple.Create("", 458), Tuple.Create<System.Object, System.Int32>(S.Shared_UpdateBtn_Text
            
            #line default
            #line hidden
, 458), false)
);

WriteLiteral(" type=\"submit\"");

WriteLiteral(" />\r\n</div>\r\n</form>\r\n");

        }
    }
}
#pragma warning restore 1591
