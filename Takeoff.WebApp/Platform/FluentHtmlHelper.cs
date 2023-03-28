using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.Web.Mvc.Html;
using Microsoft.Web.Mvc;

namespace Takeoff.Mvc
{

    /// <summary>
    /// Provides a nice way to create html elements using a fluent API.  This works especially well when formatting resources.
    /// </summary>
    public static class FluentHtmlHelper
    {
        /// <summary>
        /// Returns a fluent builder for the element with the given name.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static FluentTagBuilder Element(this HtmlHelper helper, string element)
        {
            return new FluentTagBuilder(element);
        }

        /// <summary>
        /// Creates a fluent tag builder 
        /// </summary>
        /// <param name="html"></param>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public static FluentTagBuilder StartTag(this HtmlHelper html, string tagName)
        {
            return new FluentTagBuilder(tagName).RenderStart();
        }


        /// <summary>
        /// Returns an opening tag for a link to a controller action.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public static FluentTagBuilder StartActionLink(this HtmlHelper htmlHelper, string actionName, string controllerName)
        {
            return StartActionLink(htmlHelper, actionName, controllerName, null);
        }

        /// <summary>
        /// Returns an opening tag for a link to a controller action.
        /// </summary>
        public static FluentTagBuilder StartActionLink(this HtmlHelper htmlHelper, string actionName, string controllerName, object routeValues)
        {
            return StartActionLink(htmlHelper, actionName, controllerName, routeValues, UrlType.Relative);
        }

        /// <summary>
        /// Returns an opening tag for a link to a controller action.
        /// </summary>
        public static FluentTagBuilder StartActionLink(this HtmlHelper htmlHelper, string actionName, string controllerName, object routeValues, UrlType urlType)
        {
            var urlHelper = htmlHelper.ViewContext.Controller.CastTo<Controller>().Url;
            return new FluentTagBuilder("a").RenderMode(TagRenderMode.StartTag).Href(urlHelper.Action(actionName, controllerName, routeValues, urlType));
        }

        /// <summary>
        /// Returns an opening tag for a link to a controller action.
        /// </summary>
        public static FluentTagBuilder StartActionLink<TController>(this HtmlHelper htmlHelper, Expression<Action<TController>> expression) where TController : Controller
        {
            var url = LinkBuilder.BuildUrlFromExpression<TController>(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, expression);
            return new FluentTagBuilder("a").RenderMode(TagRenderMode.StartTag).Href(url);
        }

    }


    /// <summary>
    /// Extends the TagBuilder so it can be rendered by mvc (by implementing IHtmlString) and provides fluent methods for adding attributes and whatnot.
    /// </summary>
    /// <remarks>This is a lightweight version of mvccontrib's fluent html helpers - http://mvccontrib.codeplex.com.  I tried theirs out and felt it was overkill and </remarks>
    public class FluentTagBuilder : TagBuilder, IHtmlString
    {
        public FluentTagBuilder(string tagName) : base(tagName) { }

        /// <summary>
        /// Sets the 'id' attribute, overwriting any existing value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FluentTagBuilder Id(string value)
        {
            return Attr("id", value);
        }

        /// <summary>
        /// Sets the 'class' attribute, overwriting any existing value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FluentTagBuilder Class(string value)
        {
            return Attr("class", value);
        }


        /// <summary>
        /// Adds a new css class, keeping any existing class(es) already set.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FluentTagBuilder AddClass(string value)
        {
            this.AddCssClass(value);
            return this;
        }

        /// <summary>
        /// Sets the 'href' attribute, overwriting any existing value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FluentTagBuilder Href(string value)
        {
            return Attr("href", value);
        }

        /// <summary>
        /// Sets the 'alt' attribute, overwriting any existing value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FluentTagBuilder Alt(string value)
        {
            return Attr("alt", value);
        }

        /// <summary>
        /// Sets the 'title' attribute, overwriting any existing value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FluentTagBuilder Title(string value)
        {
            return Attr("title", value);
        }

        /// <summary>
        /// Sets the inner html of the element.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public FluentTagBuilder Html(string innerHtml)
        {
            this.InnerHtml = innerHtml;
            return this;
        }

        /// <summary>
        /// Sets the inner text of the element.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public FluentTagBuilder Text(string text)
        {
            this.SetInnerText(text);
            return this;
        }

        /// <summary>
        /// Sets the given attribute, overwriting the current value if it already exists.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public FluentTagBuilder Attr(string name, object value)
        {
            return this.Attr(name, Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Sets the given attribute, overwriting the current value if it already exists.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public FluentTagBuilder Attr(string name, string value)
        {
            this.MergeAttribute(name, value, true);
            return this;
        }


        /// <summary>
        /// Applies the given html attributes (from an anonymous object).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public FluentTagBuilder Atts(object htmlAttributes)
        {
            this.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes), true);
            return this;
        }


        /// <summary>
        /// Indicates that only the start (opening) tag will be rendered once ToHtmlString() is called.
        /// </summary>
        /// <param name="renderMode"></param>
        /// <returns></returns>
        public FluentTagBuilder RenderStart()
        {
            return RenderMode(TagRenderMode.StartTag);
        }

        /// <summary>
        /// Indicates that only the end (closing) tag will be rendered once ToHtmlString() is called.
        /// </summary>
        /// <param name="renderMode"></param>
        /// <returns></returns>
        public FluentTagBuilder RenderEnd()
        {
            return RenderMode(TagRenderMode.EndTag);
        }

        /// <summary>
        /// Sets the mode in which the element's tag will be rendered.
        /// </summary>
        /// <param name="renderMode"></param>
        /// <returns></returns>
        public FluentTagBuilder RenderMode(TagRenderMode renderMode)
        {
            _renderMode = renderMode;
            return this;
        }
        TagRenderMode _renderMode = TagRenderMode.Normal;

        /// <summary>
        /// Called by framework during render.
        /// </summary>
        /// <returns></returns>
        public string ToHtmlString()
        {
            return this.ToString(_renderMode);
        }

        public override string ToString()
        {
            return ToHtmlString();
        }
    }

}