//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Web.Routing;
//using System.Web.Mvc;
//using System.Reflection;
//using System.Web;
//using System.Text.RegularExpressions;

//namespace Mediascend.Web
//{
//             //    * productions
//             //* produtions/{id}/edit
//             //* productions/Edit/{id}
//             //* productions/Edit (tricky!)
//             //* productions/{productionId}/Comments
//             //* productions/{productionId}/Comments/{id}
//             //* productions/{id}/edit/crazy
//             //* 

//    public static class SuperRouteHelper
//    {

//        const string DefaultAction = "Index";

//        static string[] RestActions = new string[] { "Details", "Edit", "Delete" };

//        static string ActionName(this MethodInfo action)
//        {
//            var customName = (ActionNameAttribute)Attribute.GetCustomAttribute(action, typeof(ActionNameAttribute));
//            return customName == null ? action.Name : customName.Name;
//        }

//        static HttpMethodConstraint ActionHttpMethods(this MethodInfo action)
//        {
//            var post = (HttpPostAttribute)Attribute.GetCustomAttribute(action, typeof(HttpPostAttribute));
//            if ( post != null )
//                return new HttpMethodConstraint("POST");
//            var get = (HttpGetAttribute)Attribute.GetCustomAttribute(action, typeof(HttpGetAttribute));
//            if ( get != null )
//                return new HttpMethodConstraint("GET");
//            var delete = (HttpDeleteAttribute)Attribute.GetCustomAttribute(action, typeof(HttpDeleteAttribute));
//            if ( delete != null )
//                return new HttpMethodConstraint("DELETE");
//            var put = (HttpPutAttribute)Attribute.GetCustomAttribute(action, typeof(HttpPutAttribute));
//            if (put != null)
//                return new HttpMethodConstraint("PUT");
//            var verbs = (AcceptVerbsAttribute)Attribute.GetCustomAttribute(action, typeof(AcceptVerbsAttribute));
//            if ( verbs != null )
//                return new HttpMethodConstraint(verbs.Verbs.ToArray());

//            return null;
//        }


//        static string[] ActionHttpVerbs(this MethodInfo action)
//        {
//            var post = (HttpPostAttribute)Attribute.GetCustomAttribute(action, typeof(HttpPostAttribute));
//            if (post != null)
//                return new string[]{"POST"};
//            var get = (HttpGetAttribute)Attribute.GetCustomAttribute(action, typeof(HttpGetAttribute));
//            if (get != null)
//                return new string[]{"GET"};
//            var delete = (HttpDeleteAttribute)Attribute.GetCustomAttribute(action, typeof(HttpDeleteAttribute));
//            if (delete != null)
//                return new string[]{"DELETE"};
//            var put = (HttpPutAttribute)Attribute.GetCustomAttribute(action, typeof(HttpPutAttribute));
//            if (put != null)
//                return new string[]{"PUT"};
//            var verbs = (AcceptVerbsAttribute)Attribute.GetCustomAttribute(action, typeof(AcceptVerbsAttribute));
//            if (verbs != null)
//                return verbs.Verbs.ToArray();

//            return null;
//        }


//        static bool IsRestAction(this MethodInfo action)
//        {
//            var param = action.GetParameters().FirstOrDefault();
//            return param != null && param.Name.Equals("id", StringComparison.OrdinalIgnoreCase);
//        }

//        static bool IsDefaultAction(this MethodInfo action)
//        {
//            var actionName = action.ActionName();
//            if ( actionName.Equals(DefaultAction, StringComparison.OrdinalIgnoreCase) )
//                return true;
//            if ( actionName.Equals("Create", StringComparison.OrdinalIgnoreCase) )
//            {
//                var methods = action.ActionHttpMethods();
//                if ( methods != null && methods.AllowedMethods.Contains("POST", StringComparer.OrdinalIgnoreCase) )
//                    return true;
//            }
//            return false;
//        }

//        static string ControllerName(this Type controllerType)
//        {
//            return controllerType.Name.EndWithout("Controller");
//        }
//        static string ControllerName(this MethodInfo action)
//        {
//            return action.DeclaringType.ControllerName();
//        }

//        /// <summary>
//        /// Gets the relative url for the given controller.  This can include url parameters like {productionId}.  This will never end or start with "/".
//        /// </summary>
//        /// <param name="controllerType"></param>
//        /// <returns></returns>
//        static string ControllerUrl(this Type controllerType)
//        {
//            var name = controllerType.ControllerName();
//            var subController = (SubControllerAttribute)Attribute.GetCustomAttribute(controllerType, typeof(SubControllerAttribute));
//            if (subController == null)
//                return name;
//            var urlRoot = subController.BaseUrl.StartWithout("/").EndWithout("/");
//            if (!subController.UrlContainsControllerName)
//            {
//                urlRoot += "/" + name;//added actual controller name instead of"{controller}" because Url.Action wasn't working
//            }
//            return urlRoot;
//        }

//        //rules:  any action whose first (or only) parameter is called "id", will be restful
//        //staff/accounts/3424234/users/3333/videos/333

//        private class RouteInfo
//        {
//            public string Url { get; set; }

//            public object Defaults { get; set; }

//            public object Constraints { get; set; }

//            public RouteInfo(string url, string actionName, string controller, HttpMethodConstraint verbsToAccept)
//            {
//                this.Url = url;
//                Defaults = new { controller = controller, action = actionName };
//                if ( verbsToAccept != null )
//                {
//                    Constraints = new { httpMethod = verbsToAccept };
//                }
//            }
//        }


//        class ActionInfo
//        {
//            public ActionInfo()
//            {

//            }

//            //public List<string> RestHttpVerbs = new List<string>();
//            //public List<string> RootHttpVerbs = new List<string>();
//            //public List<string> DefaultHttpVerbs = new List<string>();
//            //public List<string> NormalHttpVerbs = new List<string>();
//            //public List<string> OldUrls = new List<string>();

//            public bool IsRestAction { get; set; }
//            public bool IsRootUrl { get; set; }
//            public bool IsDefaultAction { get; set; }
//            public string[] OldUrls { get; set; }


//            public Type Controller { get; set; }

//            public string ControllerName()
//            {
//                return Controller.ControllerName();
//            }

//            public HttpMethodConstraint VerbsToAccept { get; set; }
//            public string Name { get; set; }
            
//            public ActionType Type { get; set; }

//            public string Url { get; set; }

//            public void AddVerbs(string[] verbs)
//            {

//            }
//        }

//        /// <summary>
//        /// The type of action specified, ordered by priority.
//        /// </summary>
//        enum ActionType
//        {
//            Root,
//            Default,
//            Rest,
//            Normal,
//            Old,
//        }

//        /* Actions fall into the following categories:
//         * 
//         * 1. A root action, like /demo.
//         * 2. A restful action, where the ID comes before the action name
//         * 3. A normal action, where it goes /controller/action.  
//         * 
//         * Special cases:
//         * - index
//         * - Create (post to root)
//         * - details (get from root with id)
//         * 
//         * 
//         * Sometimes there is a mixture so you gotta figure taht shit out.
//         * 
//         */


//        public static void RegisterRoutes(RouteCollection routes, Assembly[] assemblies)
//        {
//            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

//            string baseUrl = string.Empty;//eventually if you add areas, this will make it easier to add that feature

//            var controllers = assemblies.SelectMany(a => a.GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(Controller)) && x.Name.EndsWith("Controller")));

//            var actions = new List<ActionInfo>();
//            foreach (var controller in controllers)
//            {
//                //bool isRootController = controller.HasAttribute<RootUrlAttribute>();
//                var actionsInController = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m => m.ReturnType.Equals(typeof(ActionResult)) && Attribute.GetCustomAttribute(m, typeof(NonActionAttribute)) == null);
//                //foreach (var action in actionsInController)
//                //{
//                //    var actionName = action.ActionName();
//                //    var controllerUrl = controller.ControllerUrl();
//                //    if (isRootController || action.HasAttribute<RootUrlAttribute>())
//                //    {
//                //        var existingAction = actions.Where(a => a.Type == ActionType.Root && a.Name == actionName && a.Controller == controller).FirstOrDefault();
//                //        if (existingAction == null)
//                //        {
//                //            actions.Add(new ActionInfo
//                //            {
//                //                Controller = controller,
//                //                Name = actionName,
                                
//                //            });
//                //        }
//                //    }
//                //}
//                foreach (var actionByName in actionsInController.ToLookup(action => action.ActionName()))
//                {
//                    List<string> verbs = new List<string>();
//                    List<string> oldUrls = new List<string>();
//                    bool acceptAllVerbs = false;
//                    var actionInfo = new ActionInfo
//                    {
//                        Controller = controller,
//                        Name = actionByName.Key,
//                    };
//                    if (controller.HasAttribute<RootUrlAttribute>() )
//                        actionInfo.IsRootUrl = true;
//                    foreach (var actionMethod in actionByName)
//                    {
//                        var actionHttpVerbs = actionMethod.ActionHttpVerbs();
//                        if (!actionHttpVerbs.HasItems())
//                        {
//                            acceptAllVerbs = true;
//                        }
//                        actionHttpVerbs.IfNotNull(vs => verbs.AddRange(vs));
//                        if (actionMethod.IsRestAction())
//                            actionInfo.IsRestAction = true;
//                        if (actionMethod.HasAttribute<RootUrlAttribute>())
//                            actionInfo.IsRootUrl = true;
//                        if (actionMethod.IsDefaultAction())
//                            actionInfo.IsDefaultAction = true;
//                        foreach (OldUrlAttribute oldUrl in Attribute.GetCustomAttributes(actionMethod, typeof(OldUrlAttribute)))
//                        {
//                            oldUrls.Add(oldUrl.Url.NotSurroundedBy("/"));
//                        }
//                    }
//                    if (oldUrls.Count > 0)
//                        actionInfo.OldUrls = oldUrls.ToArray();
//                    if (!acceptAllVerbs && verbs.Count > 0)
//                        actionInfo.VerbsToAccept = new HttpMethodConstraint(verbs.ToArray());
//                    actions.Add(actionInfo);
//                }                
//            }
            
//            var actionsPerController = actions.ToLookup(m => m.Controller);

//            var routeInfos = new List<RouteInfo>();
//            //root actions have no controller url in their route and they are first in line
//            var rootActions = actions.Where(m => m.IsRootUrl);
//            foreach (var action in rootActions)
//            {
//                var actionName = action.Name;
//                var url = baseUrl + (action.IsDefaultAction ? "" : actionName);//Index and Create (POST) is the root of the whole app
//                routeInfos.Add(new RouteInfo(url, actionName, action.ControllerName(), action.VerbsToAccept));
//                if (action.IsRestAction)
//                {
//                    routeInfos.Add(new RouteInfo(url + "/{id}", actionName, action.ControllerName(), action.VerbsToAccept));
//                }
//            }

//            //next come sub controllers.  these go in front of top level controllers 
//            var controllerList = controllers.ToList();
//            //controllerList.Sort((x,y) => {
//            //    var xUrl = x.ControllerUrl();
//            //    var yUrl = y.ControllerUrl();
//            //    var xLevels = xUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
//            //    var yLevels = yUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
//            //    var xTokens = Regex.Matches(x.ControllerUrl(), Regex.Escape(@"{")).Count;
//            //    var yTokens = Regex.Matches(y.ControllerUrl(), Regex.Escape(@"{")).Count;
//            //    if (xTokens == 0 && yTokens == 0)
//            //        return xLevels.Length.CompareTo(yLevels.Length);//the one with the fewest levels will come first
//            //    else if (xTokens == 0 || yTokens == 0)//no tokens in the first one, so it comes first
//            //        return xTokens == 0 ? -1 : 1;
//            //    else
//            //    {
//            //        return xLevels.Length.CompareTo(yLevels.Length);
//            //    }
//            //});

//            foreach (var controller in controllerList)
//            {
//                var controllerActions = actionsPerController[controller];
//                var controllerName = controller.ControllerName();
//                var controllerUrl = baseUrl + controller.ControllerUrl();

//                foreach (var action in controllerActions)
//                {
//                    //first handle gets (map to Index action) and posts (map to Create action)
//                    var actionName = action.Name;
//                    var isDefaultAction = action.IsDefaultAction;
//                    if (isDefaultAction)
//                    {
//                        routeInfos.Add(new RouteInfo(controllerUrl, actionName, action.ControllerName(), action.VerbsToAccept));
//                    }
//                    if (action.IsRestAction)
//                    {
//                        //details action gets the root url + the id parameter, so hte url would be something like accounts/{id}
//                        if (!isDefaultAction && actionName.Equals("Details", StringComparison.OrdinalIgnoreCase))
//                        {
//                            routeInfos.Add(new RouteInfo(controllerUrl + "/{id}", actionName, action.ControllerName(), action.VerbsToAccept));
//                        }
//                        var idBefureUrl = controllerUrl + "/{id}/" + actionName;//cases like foo/3234234/bar
//                        routeInfos.Add(new RouteInfo(idBefureUrl, actionName, action.ControllerName(), action.VerbsToAccept));
//                        var idAfterUrl = controllerUrl + "/" + actionName + "/{id}";//cases like foo/bar/3234234
//                        routeInfos.Add(new RouteInfo(idAfterUrl, actionName, action.ControllerName(), action.VerbsToAccept));
//                    }
//                    routeInfos.Add(new RouteInfo(controllerUrl + "/" + actionName, actionName, action.ControllerName(), action.VerbsToAccept));
//                }
//            }

//            /*now we take all the route infos and sort them so everything falls back nicely.  an example ordering would be 
//             * "" - maps to app root
//             * about
//             * productions
//             * produtions/{id}/edit
//             * productions/Edit/{id}
//             * productions/Edit (tricky!)
//             * productions/{productionId}/Comments
//             * productions/{productionId}/Comments/{id}
//             * productions/{id}/edit/crazy
//             * 
//             * 
//             */

//            //routeInfos.Sort((x, y) =>
//            //{
//            //    //1 means x > y, -1 means x < Y
//            //    var xTokens = Regex.Matches(x.Url, Regex.Escape("{")).Count;
//            //    var yTokens = Regex.Matches(y.Url, Regex.Escape("{")).Count;

//            //    var xLevels = x.Url.Split(new char[]{'/'}, StringSplitOptions.RemoveEmptyEntries);
//            //    //Regex.Matches(x.Url, Regex.Escape(@"/")).Count;
//            //    var yLevels = y.Url.Split(new char[]{'/'}, StringSplitOptions.RemoveEmptyEntries);

//            //    if (xTokens == 0 && yTokens == 0)
//            //    {
//            //        return xLevels.Length.CompareTo(yLevels.Length);//the one with the fewest levels will come first
//            //    }
//            //    else if (xTokens == 0 || yTokens == 0)//no tokens in the first one, so it comes first
//            //    {
//            //        return xTokens == 0 ? -1 : 1;
//            //    }
//            //    else
//            //    {
//            //        //first come the ones whose tokens appear at the farthest level
//            //        List<int> xTokenLevels = new List<int>();
//            //        for (var i = 0; i < xLevels.Length; i++)
//            //        {
//            //            if (xLevels[i].Contains('{'))
//            //            {
//            //                xTokenLevels.Add(i);
//            //            }
//            //        }
//            //        List<int> yTokenLevels = new List<int>();
//            //        for (var i = 0; i < yLevels.Length; i++)
//            //        {
//            //            if (yLevels[i].Contains('{'))
//            //            {
//            //                yTokenLevels.Add(i);
//            //            }
//            //        }
//            //        while (xTokenLevels.Count > 0 && yTokenLevels.Count > 0)
//            //        {
//            //            if (xTokenLevels.First() != yTokenLevels.First())
//            //            {
//            //                return -1 * xTokenLevels.First().CompareTo(yTokenLevels.First());
//            //            }
//            //            xTokenLevels.RemoveAt(0);
//            //            yTokenLevels.RemoveAt(0);
//            //        }
//            //        //this means one had more tokens than the other.  in this case the one with the fewest tokens will go first
//            //        if (xTokenLevels.Count != yTokenLevels.Count)
//            //        {
//            //            return xTokens.CompareTo(yTokens);
//            //        }

//            //        //ones with the most levels go first to avoid a rest action liek productions/{id} from overriding productions/{productionId}/comments/{id}
//            //        return -1 * xLevels.Length.CompareTo(yLevels.Length);
//            //    }
//            //});


//            //old urls contain the full url, including controller.  they are only defined on actions.  they go at the end of the list so Url.Action doesn't give the wrong link
//            foreach (var action in actions.Where(m => m.OldUrls.HasItems()))
//            {
//                foreach (string oldUrl in action.OldUrls )
//                {
//                    routeInfos.Add(new RouteInfo(oldUrl, action.Name, action.ControllerName(), action.VerbsToAccept));
//                }
//            }

//            var urls = routeInfos.Select(i => i.Url).ToArray();

//            var j = 0;
//            foreach (var r in routeInfos)
//            {
//                routes.MapRouteLowerCase("mad" + j.ToInvariant(), r.Url, r.Defaults, r.Constraints);
//                j++;
//            }
//        }



//        public static bool HasAttribute<T>(this MemberInfo member) where T:Attribute
//        {
//            return Attribute.GetCustomAttributes(member, typeof(T)).HasItems();
//        }


//        public static Route MapRouteLowerCase(this RouteCollection routes, string name, string url, object defaults)
//        {
//            return routes.MapRouteLowerCase(name, url, defaults, null);
//        }

//        public static Route MapRouteLowerCase(this RouteCollection routes, string name, string url, object defaults, object constraints)
//        {
//            Route route = new LowercaseRoute(url, new MvcRouteHandler())
//            {
//                Defaults = new RouteValueDictionary(defaults),
//                Constraints = new RouteValueDictionary(constraints)
//            };
//            routes.Add(name, route);
//            return route;
//        }

//        //turns urls into lowercase, which mvc doesn't do by default
//        //http://stackoverflow.com/questions/878578/how-can-i-have-lowercase-routes-in-asp-net-mvc
//        public class LowercaseRoute : Route
//        {
//            public LowercaseRoute(string url, IRouteHandler routeHandler)
//                : base(url, routeHandler) { }
//            public LowercaseRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
//                : base(url, defaults, routeHandler) { }
//            public LowercaseRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler)
//                : base(url, defaults, constraints, routeHandler) { }
//            public LowercaseRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler) : base(url, defaults, constraints, dataTokens, routeHandler) { }
//            public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
//            {
//                VirtualPathData path = base.GetVirtualPath(requestContext, values);

//                if (path != null)
//                {
//                    //don't lowercase anything in the query string
//                    var questionMark = path.VirtualPath.IndexOf('?');
//                    if (questionMark >= 0 && questionMark < path.VirtualPath.Length - 1)
//                    {
//                        path.VirtualPath = path.VirtualPath.Substring(0, questionMark + 1).ToLowerInvariant() + path.VirtualPath.Substring(questionMark + 1);
//                    }
//                    else
//                    {
//                        path.VirtualPath = path.VirtualPath.ToLowerInvariant();
//                    }
//                }
//                return path;
//            }

//        }



//        public class IdConstraint : IRouteConstraint
//        {

//            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
//            {
//                object objVal;
//                if ( values.TryGetValue(parameterName, out objVal) )
//                {
//                    string stringValue = objVal as string;
//                    if (stringValue == null)
//                    {
//                        return objVal is int || objVal is Guid;//for testing
//                    }
//                    else if (stringValue.HasChars())
//                    {
//                        int intValue;
//                        if ( int.TryParse(stringValue, out intValue) )
//                            return true;
//                        Guid guidValue;
//                        if (Guid.TryParse(stringValue, out guidValue) && (guidValue != Guid.Empty))
//                            return true;
//                    }
//                }
//                return false;
//            }
//        }


//    }
//}
