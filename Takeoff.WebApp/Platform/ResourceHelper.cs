using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.ComponentModel;

namespace Takeoff.Mvc
{
    public static class ResourceHelper
    {

        /// <summary>
        /// Formats a resource string using an enhanced string.Format syntax.  Supports html substitutions.
        /// </summary>
        /// <param name="formatString"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <remarks>
        /// Example format strings:   
        /// "You are great, {0}"
        /// "Not a member?  {0}Click here{/0} to join."
        /// "Not a member, {username}?  {joinlink}Click here{/joinlink} to join."
        /// "Not a member, {username}?  {joinlink}Click here{/joinlink} to join.  Offer ends on {1}."
        /// 
        /// If you use named parameters, they must be taken from an anonymous object in the first argument.
        /// Also, it is recommended that you pass in FluentTagBuilder objects, which can be created by html helpers like Element and StartActionLink.
        /// </remarks>
        public static IHtmlString FormatResource(this string formatString, params object[] args)
        {
            if (args == null)
                return new HtmlString(HttpUtility.HtmlEncode(formatString));

            var htmlString = formatString.Format((argName) =>
            {
                if (!argName.StartsWith(@"/"))
                {
                    return GetArgValue(args, argName).ToString(); //this will apply to html and IHtmlString 
                }
                argName = argName.Substring(1);
                var argValue = GetArgValue(args, argName);
                var asTagBuilder = argValue as FluentTagBuilder;
                if (asTagBuilder != null)
                    return asTagBuilder.ToString(TagRenderMode.EndTag);

                string tagName = null;
                string html = argValue.ToString(); //IHtmlString and regular string will do the same thing
                if (html.StartsWith("<"))
                    tagName = html.After("<").Before(" ");
                if (tagName.HasChars())
                    return "</" + tagName + ">";
                return string.Empty;
            }, HttpUtility.HtmlEncode);
            return new HtmlString(htmlString);
        }

        /// <summary>
        /// Returns either an http encoded string, a FluentTagBuilder, or an IHtmlString.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="argName"></param>
        /// <returns></returns>
        private static object GetArgValue(object[] args, string argName)
        {
            object value;
            if ( char.IsDigit(argName[0]) )
            {
                value = args[argName.ToInt()];
            }
            else
            {
                value = TypeDescriptor.GetProperties(args[0])[argName].GetValue(args[0]);
            }
            if (value == null)
                return string.Empty;

            var asString = value as string;
            if (asString != null)
                return HttpUtility.HtmlEncode(asString);
            var asIHtmlString = value as IHtmlString;//FluentTagBuilder implements this interface but we cast to it explicitly in the FormatResource method.
            if (asIHtmlString != null)
                return asIHtmlString;
            return HttpUtility.HtmlEncode(Convert.ToString(value));
        }

    }
}
