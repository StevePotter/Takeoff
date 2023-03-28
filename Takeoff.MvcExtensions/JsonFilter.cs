using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Text;
using System.Web.Routing;
using System.Diagnostics;
using Microsoft.CSharp;
using System.Linq;
using Newtonsoft.Json;

namespace Mediascend.Web
{

    //http://www.c-sharpcorner.com/Blogs/BlogDetail.aspx?BlogId=863

    /// <summary>
    /// Fixes a problem with ASP.NET MVC where parameters specified in a JSON post aren't filled.
    /// </summary>
    /// <remarks>
    /// For example, say there is an action Create(string name, int age){}
    /// 
    /// And you post to it with a json object:
    /// { name: 'joe', age: 12 }
    /// 
    /// That won't work in regular asp.net mvc.  Instead it expects something like Create(Person person) where person has the parameters.  That is lame.  So this attribute makes it possible.
    /// 
    /// I used to do a fancy deal where I generated a class and everything using reflection.emot.  But the newtonsoft json.net library made it easy to do it so i just stuck with that.
    /// </remarks>
    public class JsonParametersAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if ((filterContext.HttpContext.Request.ContentType ?? string.Empty).Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                var parameterDefs = filterContext.ActionDescriptor.GetParameters().ToDictionary(p => p.ParameterName);
                if (parameterDefs.Count == 0 || filterContext.HttpContext.Request.InputStream.Length == 0)
                    return;//no params needed and/or provided, no need to continue

                try
                {
                    var originalPosition = filterContext.HttpContext.Request.InputStream.Position;
                    if (originalPosition > 0)
                        filterContext.HttpContext.Request.InputStream.Position = 0;
                    var reader = new JsonTextReader(new StreamReader(filterContext.HttpContext.Request.InputStream));
                    var serializer = Newtonsoft.Json.JsonSerializer.Create(new JsonSerializerSettings { });
                    //read through all the top level (depth 1) properties and deserialize them individually to their proper types then set the parameters
                    while (reader.Read())
                    {
                        if (reader.Depth == 1 && reader.TokenType == JsonToken.PropertyName)
                        {
                            var property = (string)reader.Value;
                            reader.Read();
                            ParameterDescriptor parameter;
                            if (parameterDefs.TryGetValue(property, out parameter))
                            {
                                var value = serializer.Deserialize(reader, parameter.ParameterType);
                                filterContext.ActionParameters[parameter.ParameterName] = value;
                            }
                        }
                    }

                    filterContext.HttpContext.Request.InputStream.Position = originalPosition;

                }
                catch
                {
                    //indicates bad input.  kinda cheap but without the try/catch the server can crash.  you should check for empty input though
                }

            }

        }

    }

    public class LogActionFilter : ActionFilterAttribute
    {
        
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Log("OnActionExecuting", filterContext.RouteData);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            Log("OnActionExecuted", filterContext.RouteData);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            Log("OnResultExecuting", filterContext.RouteData);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            Log("OnResultExecuted", filterContext.RouteData);
        }

        private void Log(string methodName, RouteData routeData)
        {
            var controllerName = routeData.Values["controller"];
            var actionName = routeData.Values["action"];
            var message = String.Format("{0} controller:{1} action:{2}", methodName, controllerName, actionName);
            Debug.WriteLine(message, "Action Filter Log");
        }

    }

}