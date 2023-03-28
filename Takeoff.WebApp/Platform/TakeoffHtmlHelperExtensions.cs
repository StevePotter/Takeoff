using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Linq.Expressions;
using Takeoff.Resources;

namespace Takeoff
{
    public static class Bootstrap2HtmlHelperExtensions
    {

        public static void DisableNagBanners(this HtmlHelper html)
        {
            html.ViewData["EnableNagBanner"] = false;
        }

        public static void ShowExternalLinksForVisitors(this HtmlHelper html)
        {
            html.ViewData["ShowExternalLinksForVisitors"] = true;
        }

        public static IHtmlString ControlLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return ControlLabelFor(html, expression, null);
        }

        public static IHtmlString SubmitButton(this HtmlHelper html)
        {
            return html.SubmitButton(Strings.Shared_SubmitBtn_Text);
        }

        public static IHtmlString SubmitButton(this HtmlHelper html, string text)
        {
            var tagBuilder = new TagBuilder("input");
            tagBuilder.Attributes.Add("type","submit");
            tagBuilder.Attributes.Add("value", text);
            tagBuilder.Attributes.Add("class", html.PrimaryButtonClass());
            return new HtmlString(tagBuilder.ToString(TagRenderMode.SelfClosing));
        }

        public static IHtmlString ControlTooltip(this HtmlHelper html, string tooltipText)
        {
            //<span id="Logo-Tooltip2" class="tooltip-anchor label" data-placement="right" data-title="@("The logo will be shown alongside the production title on the site and in emails.  You can also set an account-wide logo in Account/Logo.")">?</span>
            var tagBuilder = new TagBuilder("span");
            tagBuilder.Attributes.Add("class", "tooltip-anchor label");
            tagBuilder.Attributes.Add("data-title", tooltipText);
            tagBuilder.SetInnerText("?");
            return new HtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }


        public static IHtmlString FormButtons(this HtmlHelper html, string submitButtonText)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.InnerHtml = SubmitButton(html, submitButtonText).ToHtmlString();
            tagBuilder.Attributes.Add("class", html.FormActionsClass());
            return new HtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }

        public static string FormActionsClass(this HtmlHelper html)
        {
            return "form-actions";
        }

        public static string AlertClass(this HtmlHelper html, bool isBlockMessage = false)
        {
            return "alert" + (isBlockMessage ? " alert-block" : string.Empty);
        }

        public static string AlertWarningClass(this HtmlHelper html, bool isBlockMessage = false)
        {
            return "alert" + (isBlockMessage ? " alert-block" : string.Empty);
        }

        public static string AlertSuccessClass(this HtmlHelper html, bool isBlockMessage = false)
        {
            return "alert alert-success" + (isBlockMessage ? " alert-block" : string.Empty);
        }

        public static string AlertErrorClass(this HtmlHelper html, bool isBlockMessage = false)
        {
            return "alert alert-error" + (isBlockMessage ? " alert-block" : string.Empty);
        }

        public static string AlertInfoClass(this HtmlHelper html, bool isBlockMessage = false)
        {
            return "alert alert-info" + (isBlockMessage ? " alert-block" : string.Empty);
        }

  
        

        public static IHtmlString PasswordControlGroupFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return ControlGroup(html, html.ControlLabelFor(expression), html.PasswordFor(expression));
        }

        public static IHtmlString PasswordControlGroup(this HtmlHelper html, string labelText, string inputName, string inputId)
        {
            return ControlGroup(html, html.ControlLabel(labelText, inputId), html.Password(inputName, null, new { @id = inputId }));
        }

        public static IHtmlString TextBoxControlGroupFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return ControlGroup(html, html.ControlLabelFor(expression), html.TextBoxFor(expression));
        }

        public static IHtmlString TextBoxControlGroupFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText)
        {
            return ControlGroup(html, html.ControlLabelFor(expression, labelText), html.TextBoxFor(expression));
        }

        /// <summary>
        /// Typical class for form instructions.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string FormInstructionsClass(this HtmlHelper html)
        {
            return "alert-message block-message instructions";
        }


        public static string ControlGroupClass(this HtmlHelper html)
        {
            return "control-group";
        }

        public static IHtmlString ControlsWrapperBeginTag(this HtmlHelper html)
        {
            return new HtmlString("<div class=\"controls\">");
        }

        public static IHtmlString ControlsWrapperEndTag(this HtmlHelper html)
        {
            return new HtmlString("</div>");
        }

        public static string PrimaryButtonClass(this HtmlHelper html)
        {
            return "btn btn-primary";
        }

        public static IHtmlString ControlGroup(this HtmlHelper html, IHtmlString label, IHtmlString control, string additionalGroupCssClass = null, string groupId = null)
        {
            additionalGroupCssClass = additionalGroupCssClass.HasChars() ? "control-group " + additionalGroupCssClass : "control-group";
            string groupDivOpen = groupId.HasChars() ? "<div class=\"" + additionalGroupCssClass + "\" id=\"" + groupId + "\">" : "<div class=\"" + additionalGroupCssClass + "\">";
            string output = groupDivOpen + label.ToHtmlString() +
                            "<div class=\"controls\">" + control.ToHtmlString() +
                            "</div></div>";
            return new HtmlString(output);
        }

        public static IHtmlString ControlLabel(this HtmlHelper html, string labelText)
        {
            return ControlLabel(html, labelText, null);
        }

        public static IHtmlString ControlLabel(this HtmlHelper html, string labelText, string forId)
        {
            return ControlLabel(html, labelText, forId, null);
        }


        public static IHtmlString ControlLabel(this HtmlHelper html, string labelText, string forId, string tooltip)
        {
            var tagBuilder = new TagBuilder("label");
            if (forId.HasChars())
                tagBuilder.Attributes.Add("for", forId);
            tagBuilder.AddCssClass("control-label");
            if ( tooltip.HasChars())
            {
                tagBuilder.InnerHtml = html.Encode(labelText) + ControlTooltip(html, tooltip).ToHtmlString();
            }
            else
            {
                tagBuilder.SetInnerText(labelText);
            }
            return new HtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }


        /// <summary>
        /// Renders google analytics init script, unless it's disabled.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static IHtmlString GoogleAnalytics(this HtmlHelper html)
        {
            if ( !ApplicationSettings.EnableGoogleAnalytics )
                return new HtmlString(string.Empty);

            return new HtmlString(@"var _gaq = _gaq || []; _gaq.push(['_setAccount', 'UA-17892575-1']); _gaq.push(['_trackPageview']); (function() { var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true; ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js'; var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s); })();");
        }
        public static IHtmlString ControlLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText, string tooltip = null)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var htmlFieldName = ExpressionHelper.GetExpressionText(expression);

            string str = labelText ?? (metadata.DisplayName ?? (metadata.PropertyName ?? htmlFieldName.Split(new char[] { '.' }).Last<string>()));
            if (String.IsNullOrEmpty(str))
            {
                return MvcHtmlString.Empty;
            }
            return html.ControlLabel(str,  TagBuilder.CreateSanitizedId(html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)), tooltip);
        }
    }

    public static class TakeoffHtmlHelperExtensions
    {


        /// <summary>
        /// Writes a class='foo' if the condition evals to true.
        /// </summary>
        /// <param name="className"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static IHtmlString ClassAttIf(this HtmlHelper helper, string className, Func<bool> condition )
        {
            return condition() ? new HtmlString("class=\"" + className + "\"") : null;
        }

        public static IHtmlString ClassAttIfAction<TController>(this HtmlHelper helper, string className, Expression<Action<TController>> action)
        {
            if (IsAction(helper, action))
                return new HtmlString("class=\"" + className + "\"");
            return null;
        }


        public static IHtmlString ClassAttIfController<TController>(this HtmlHelper helper, string className)
        {
            if (IsController(helper, typeof(TController)))
                return new HtmlString("class=\"" + className + "\"");
            return null;
        }


        /// <summary>
        /// Indicates whether the current controller action matches that supplied.
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool IsAction<TController>(this HtmlHelper helper, Expression<Action<TController>> action)
        {
            if ( !IsController(helper, typeof(TController)) )
            {
                return false;
            }
            var body = action.Body as MethodCallExpression;
            if (body == null)
                throw new ArgumentException("Bad expression.");

            return helper.ViewData["ActionName"].CastTo<string>().EqualsCaseInsensitive(GetTargetActionName(body.Method));
        }


        public static bool IsController(this HtmlHelper helper, Type controllerType)
        {
            return helper.ViewData["ControllerName"].CastTo<string>().EqualsCaseInsensitive(controllerType.Name.EndWithout("Controller"));
        }


        //ripped from mvccontrib
        private static string GetTargetActionName(MethodInfo methodInfo)
        {
            string name = methodInfo.Name;
            ActionNameAttribute attribute = methodInfo.GetCustomAttributes(typeof(ActionNameAttribute), true).OfType<ActionNameAttribute>().FirstOrDefault<ActionNameAttribute>();
            if (attribute != null)
            {
                return attribute.Name;
            }
            if (methodInfo.DeclaringType.IsSubclassOf(typeof(AsyncController)))
            {
                if (name.EndsWith("Async", StringComparison.OrdinalIgnoreCase))
                {
                    return name.Substring(0, name.Length - "Async".Length);
                }
                if (name.EndsWith("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException();
                }
            }
            return name;
        }
    }
}