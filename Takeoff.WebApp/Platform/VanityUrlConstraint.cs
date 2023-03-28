using System;
using System.Web;
using System.Web.Routing;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.App_Start
{
    public class VanityUrlConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            object objUrl = values["vanityUrl"];
            if (objUrl != null)
            {
                var url = Convert.ToString(objUrl);
                if ( !url.HasChars() || !VantiyUrlHelper.IsValid(url) || ApplicationSettings.ForbiddenVanityUrls.Contains(url))
                {
                    return false;
                }

                var target = VantiyUrlHelper.GetObjectForUrl(url);
                if (target != null)
                {
                    if (target is IProduction)
                    {
                        values["controller"] = "Productions";
                        values["action"] = "details";
                        values["id"] = target.CastTo<ProjectThing>().Id;
                        return true;
                    }
                    else if (target is IVideo)
                    {
                        values["controller"] = "Videos";
                        values["action"] = "details";
                        values["id"] = target.CastTo<IVideo>().Id;
                        return true;
                    }
                }

            }
            return false;
        }

    }
}