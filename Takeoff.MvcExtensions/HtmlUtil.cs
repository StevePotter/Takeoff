using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.ComponentModel;
using System.Reflection;
using System.Linq.Expressions;
using System.Web.Mvc.Html;
using System.Configuration;

namespace System.Web.Mvc.Html
{
    public static class HtmlUtil
    {

        /// <summary>
        /// Overload for HtmlHelper.Encode that adds line break tags for new lines and html spaces for double spacing and crap.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="value"></param>
        /// <param name="addLineBreaks"></param>
        /// <returns></returns>
        public static string Encode(this HtmlHelper html, string value, bool addLineBreaks)
        {
            var encoded = html.Encode(value);
            if (addLineBreaks)
            {
                var lines = encoded.Trim().SplitLines();
                if (lines.Length > 0)
                    encoded = string.Join("<br />", lines);
            }
            return encoded.Replace("  ", "&nbsp; ");
        }


        public static string HtmlEncode(this string text)
        {
            if (text == null)
                return text;
            return HttpUtility.HtmlEncode(text);
        }

        public static string AppSetting(this HtmlHelper html, string key)
        {
            return ConfigurationManager.AppSettings[key];
        }


        #region Enum Dropdown

        //from http://stackoverflow.com/questions/388483/how-do-you-create-a-dropdownlist-from-an-enum-in-asp-net-mvc

        private static Type GetNonNullableModelType(ModelMetadata modelMetadata)
        {
            Type realModelType = modelMetadata.ModelType;

            Type underlyingType = Nullable.GetUnderlyingType(realModelType);
            if (underlyingType != null)
            {
                realModelType = underlyingType;
            }
            return realModelType;
        }

        private static readonly SelectListItem[] SingleEmptyItem = new[] { new SelectListItem { Text = "", Value = "" } };

        public static string GetEnumDescription<TEnum>(TEnum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if ((attributes != null) && (attributes.Length > 0))
                return attributes[0].Description;
            else
                return value.ToString();
        }

        //public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression)
        //{
        //    return EnumDropDownListFor(htmlHelper, expression, null);
        //}

        //public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        //{
        //    ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
        //    Type enumType = GetNonNullableModelType(metadata);
        //    IEnumerable<TEnum> values = Enum.GetValues(enumType).Cast<TEnum>();

        //    IEnumerable<SelectListItem> items = from value in values
        //                                        select new SelectListItem
        //                                        {
        //                                            Text = GetEnumDescription(value),
        //                                            Value = value.ToString(),
        //                                            Selected = value.Equals(metadata.Model)
        //                                        };

        //    // If the enum is nullable, add an 'empty' item to the collection
        //    if (metadata.IsNullableValueType)
        //        items = SingleEmptyItem.Concat(items);

        //    return htmlHelper.DropDownListFor(expression, items, htmlAttributes);
        //}


        #endregion 

    }
}
