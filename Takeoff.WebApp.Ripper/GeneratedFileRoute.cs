using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;

namespace Takeoff.WebApp.Ripper
{
    public class GeneratedFileRoute: Route
    {
        public GeneratedFileRoute(string url, IRouteHandler routeHandler)
            : base(url, routeHandler) { }
        public GeneratedFileRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
            : base(url, defaults, routeHandler) { }
        public GeneratedFileRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler)
            : base(url, defaults, constraints, routeHandler) { }
        public GeneratedFileRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler) : base(url, defaults, constraints, dataTokens, routeHandler) { }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            string url = string.Empty;
            if (values.ContainsKey("Controller"))
                url += values["Controller"].ToString();
            if (values.ContainsKey("Action"))
            {
                if (url.HasChars())
                    url += "_";
                url += values["Action"].ToString();
            }
            return new VirtualPathData(this, url + ".html");
        }
    }
}
