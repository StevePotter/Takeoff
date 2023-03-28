using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Collections;
using Takeoff.Css;

using System.IO;
namespace Takeoff.CssInliner
{
    /// <summary>
    /// This takes a regular html file (must have css in STYLE tags, not an external LINK), and takes hte css rules and puts them into individual elements.  This is intended for use during 
    /// emails.
    /// </summary>
    /// <remarks>
    /// Only tag selectors and class selectors are supported.  Nothing else - no mixtures (don't do td.field), descendants, pseudo selectors, attribute selectors or anything else.
    /// 
    /// It's also not smart about selector order.  And it's not smart about replacing attributes that are already inline.  
    /// 
    /// Also do not use margin style attribut it don't work!
    /// </remarks>
    public static class CssInliner
    {

        /// <summary>
        /// Takes the css styles in STYLE tags and inlines them into html elements.  Intended for use in sending html emails, where css sheets often aren't honored.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="clearCss"></param>
        /// <param name="tagsToReplaceWithTables">Certain tags, such as P, don't render reliably in clients.  However they are annoying to design with.  So we set up our templates with that html and substitute TABLE for each tag type specifed.  Note:  the attribtes are copied to the TABLE, not the TD.  This was done because of width, and since things like text and background worked fine, we left all the attributes on the TABLE and not TD.</param>
        /// <returns></returns>
        public static string InlineHtml(string html, bool clearCss = true)//, string[] tagsToReplaceWithTables = null)
        {
            return InlineHtml(html, null, clearCss);
        }

        public static string InlineHtml(string html, string css, bool clearCss = true)//, string[] tagsToReplaceWithTables = null)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var cssSource = new StringBuilder();
            if (css.HasChars())
            {
                cssSource.Append(css);
            }
            var head = doc.DocumentNode.SelectSingleNode("//head");
            if (head != null)
            {
                var styleElements = head.SelectNodes("//style");
                if (styleElements != null)
                {
                    foreach (var node in styleElements)
                    {
                        cssSource.AppendLine(node.InnerText.Trim());
                        if (clearCss)
                            node.ParentNode.RemoveChild(node);
                    }
                }
            }
            else
                return html;

            if (cssSource.Length == 0)
                return html;

            var rules = CssParser.ParseRuleDictionary(cssSource.ToString());
            var selectors = rules.GetSelectors().ToArray();

            var tags = new HashSet<string>();
            var classes = new HashSet<string>();

            foreach (var selector in rules.GetSelectors().Select(text => CssParser.ParseSelector(text)))
            {
                if (selector is ClassSelector)
                    classes.Add(((ClassSelector)selector).ClassName);
                else if (selector is TagSelector)
                    tags.Add(selector.Text.Trim().ToLowerInvariant());
                //else: warn
            }


            //originally I used xpath selectors but that was slow.  by looping through elements once and gathering the ones i need, it cut avg time from 4ms to 2
            //            var tagsToReplace = tagsToReplaceWithTables == null ? null : new HashSet<string>(tagsToReplaceWithTables);
            List<HtmlNode> elementsWithCssTags = new List<HtmlNode>();
            List<HtmlNode> elementsWithCssClasses = new List<HtmlNode>();
            List<HtmlNode> elementsToTableize = new List<HtmlNode>();
            var allElements = doc.DocumentNode.DescendantNodesAndSelf().Where(d => d.NodeType == HtmlNodeType.Element).ToArray();
            foreach (var tag in allElements)
            {
                var tagName = tag.Name.ToLowerInvariant();
                if (tags.Contains(tagName))
                    elementsWithCssTags.Add(tag);
                if (tag.Attributes["class"] != null)
                    elementsWithCssClasses.Add(tag);
            }

            if (elementsWithCssTags.Count > 0)
            {
                foreach (var element in elementsWithCssTags)
                {
                    var selector = element.Name.ToLowerInvariant();
                    var declarations = rules[selector];
                    if (declarations.Count > 0)
                        InlineDeclarations(element, declarations);
                }
            }


            if (elementsWithCssClasses.Count > 0)
            {
                foreach (var element in elementsWithCssClasses)
                {
                    var classAtt = element.Attributes["class"];
                    foreach (var className in classAtt.Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var selector = "." + className;
                        var declarations = rules[selector];//todo: add warning if no rule found
                        if (declarations != null && declarations.Count > 0)
                            InlineDeclarations(element, declarations);

                    }
                    if (clearCss)
                        element.Attributes.Remove(classAtt);
                }
            }

            foreach (var element in allElements)
            {
                var styleAtt = element.Attributes["style"];
                if (styleAtt != null && styleAtt.Value.Contains("-tableize"))
                {
                    Tableize(doc, element);
                }
            }

            var output = new StringBuilder();
            using (var reader = new StringWriter(output))
            {
                doc.Save(reader);
            }
            return output.ToString();
        }

        /// <summary>
        /// Takes an element and turns it into a table with a single cell.  The width attribute will be at the table level, and everything else will be at td level
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="p"></param>
        private static void Tableize(HtmlDocument doc, HtmlNode p)
        {
            var table = doc.CreateElement("table");
            var tr = doc.CreateElement("tr");
            var td = doc.CreateElement("td");
            table.SetAttributeValue("cellspacing", "0");
            table.SetAttributeValue("cellpadding", "0");
            table.SetAttributeValue("border", "0");
            table.AppendChild(tr).AppendChild(td);
            if (p.HasAttributes)
            {
                foreach (var att in p.Attributes)
                {
                    //width style attribute will go at the table level
                    if (att.Name.Equals("style", StringComparison.OrdinalIgnoreCase))
                    {
                        var existingStyles = CssParser.ParseDeclarations(att.Value);
                        if (existingStyles.Count > 0)
                        {
                            StringBuilder tableStyle = new StringBuilder();
                            StringBuilder tdStyle = new StringBuilder();
                            foreach (var dec in existingStyles)
                            {
                                if (dec.Property.EqualsCaseSensitive("-tableize"))
                                {
                                    continue;
                                }
                                if (dec.Property.EqualsCaseSensitive("width"))
                                {
                                    tableStyle.Append(dec.Property + ":" + dec.Value + ";");
                                    table.SetAttributeValue("width", dec.Value);
                                }
                                else
                                {
                                    tdStyle.Append(dec.Property + ":" + dec.Value + ";");
                                    if (dec.Property.EqualsCaseSensitive(CssProperties.TextAlign))
                                    {
                                        InlineNonStyleAttribute(td, "align", dec.Value);
                                    }
                                    else if (dec.Property.EqualsCaseSensitive(CssProperties.VerticalAlign))
                                    {
                                        InlineNonStyleAttribute(td, "valign", dec.Value);
                                    }
                                }
                            }
                            if (tableStyle.Length > 0)
                                table.SetAttributeValue("style", tableStyle.ToString());
                            if (tdStyle.Length > 0)
                                td.SetAttributeValue("style", tdStyle.ToString());
                        }
                    }
                    else
                    {
                        td.SetAttributeValue(att.Name, att.Value);
                    }
                }
            }


            foreach (var child in p.ChildNodes)
            {
                td.AppendChild(child);
            }
            p.ParentNode.ReplaceChild(table, p);
        }

        public static HtmlDocument InlineHtml3(string html, string css)
        {
            var clearCss = true;
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var cssSource = new StringBuilder();
            if (css.HasChars())
            {
                cssSource.Append(css);
            }
            var head = doc.DocumentNode.SelectSingleNode("//head");
            if (head != null)
            {
                var styleElements = head.SelectNodes("//style");
                if (styleElements != null)
                {
                    foreach (var node in styleElements)
                    {
                        cssSource.AppendLine(node.InnerText.Trim());
                        if (clearCss)
                            node.ParentNode.RemoveChild(node);
                    }
                }
            }
            else
                return doc;

            if (cssSource.Length == 0)
                return doc;

            var rules = CssParser.ParseRuleDictionary(cssSource.ToString());
            var selectors = rules.GetSelectors().ToArray();

            var tags = new HashSet<string>();
            var classes = new HashSet<string>();

            foreach (var selector in rules.GetSelectors().Select(text => CssParser.ParseSelector(text)))
            {
                if (selector is ClassSelector)
                    classes.Add(((ClassSelector)selector).ClassName);
                else if (selector is TagSelector)
                    tags.Add(selector.Text.Trim().ToLowerInvariant());
                //else: warn
            }


            //originally I used xpath selectors but that was slow.  by looping through elements once and gathering the ones i need, it cut avg time from 4ms to 2
            //            var tagsToReplace = tagsToReplaceWithTables == null ? null : new HashSet<string>(tagsToReplaceWithTables);
            List<HtmlNode> elementsWithCssTags = new List<HtmlNode>();
            List<HtmlNode> elementsWithCssClasses = new List<HtmlNode>();
            List<HtmlNode> elementsToTableize = new List<HtmlNode>();
            var allElements = doc.DocumentNode.DescendantNodesAndSelf().Where(d => d.NodeType == HtmlNodeType.Element).ToArray();
            foreach (var tag in allElements)
            {
                var tagName = tag.Name.ToLowerInvariant();
                if (tags.Contains(tagName))
                    elementsWithCssTags.Add(tag);
                if (tag.Attributes["class"] != null)
                    elementsWithCssClasses.Add(tag);
            }

            if (elementsWithCssTags.Count > 0)
            {
                foreach (var element in elementsWithCssTags)
                {
                    var selector = element.Name.ToLowerInvariant();
                    var declarations = rules[selector];
                    if (declarations.Count > 0)
                        InlineDeclarations(element, declarations);
                }
            }


            if (elementsWithCssClasses.Count > 0)
            {
                foreach (var element in elementsWithCssClasses)
                {
                    var classAtt = element.Attributes["class"];
                    foreach (var className in classAtt.Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var selector = "." + className;
                        var declarations = rules[selector];//todo: add warning if no rule found
                        if (declarations != null && declarations.Count > 0)
                            InlineDeclarations(element, declarations);

                    }
                    if (clearCss)
                        element.Attributes.Remove(classAtt);
                }
            }

            foreach (var element in allElements)
            {
                var styleAtt = element.Attributes["style"];
                if (styleAtt != null && styleAtt.Value.Contains("-tableize"))
                {
                    Tableize2(doc, element);
                }
            }

            return doc;
        }



        public static string GetHtml(this HtmlDocument doc)
        {
            var output = new StringBuilder();
            using (var reader = new StringWriter(output))
            {
                doc.Save(reader);
            }
            return output.ToString();
        }


        /// <summary>
        /// Takes an element and turns it into a table with a single cell.  The width attribute will be at the table level, and everything else will be at td level
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="p"></param>
        private static void Tableize2(HtmlDocument doc, HtmlNode p)
        {
            var table = doc.CreateElement("table");
            var tr = doc.CreateElement("tr");
            var td = doc.CreateElement("td");
            table.SetAttributeValue("cellspacing", "0");
            table.SetAttributeValue("border", "0");
            table.AppendChild(tr).AppendChild(td);
            if (p.HasAttributes)
            {
                foreach (var att in p.Attributes)
                {
                    //width style attribute will go at the table level
                    if (att.Name.Equals("style", StringComparison.OrdinalIgnoreCase))
                    {
                        var existingStyles = CssParser.ParseDeclarations(att.Value);
                        if (existingStyles.Count > 0)
                        {
                            StringBuilder tableStyle = new StringBuilder();
                            StringBuilder tdStyle = new StringBuilder();
                            foreach (var dec in existingStyles)
                            {
                                if (dec.Property.EqualsCaseSensitive("-tableize"))
                                {
                                    continue;
                                }
                                if (dec.Property.EqualsCaseSensitive("width"))
                                {
                                    //tableStyle.Append(dec.Property + ":" + dec.Value + ";");
                                    table.SetAttributeValue("width", dec.Value);
                                    td.SetAttributeValue("width", "100%");//it's a single table with a single td, so you can just make the cell take up the table's width
                                    //tdStyle.Append(dec.Property + ":" + dec.Value + ";");
                                    //InlineNonStyleAttribute(table, "width", dec.Value);
                                }
                                else if (dec.Property.EqualsCaseSensitive("padding"))
                                {
                                    table.SetAttributeValue("cellpadding", dec.Value.FilterNumbers(false));
                                }
                                else if (dec.Property.EqualsCaseSensitive(CssProperties.TextAlign))
                                {
                                    //putting text-align in the style attribte along with the align attribute caused it to be always left aligned, at least in FF.  no clue why but this works fine
                                    InlineNonStyleAttribute(td, "align", dec.Value);
                                }
                                else if (dec.Property.EqualsCaseSensitive(CssProperties.VerticalAlign))
                                {
                                    InlineNonStyleAttribute(td, "valign", dec.Value);
                                }
                                else if (dec.Property.EqualsCaseSensitive(CssProperties.BackgroundColor))
                                {
                                    InlineNonStyleAttribute(td, "bgcolor", dec.Value);
                                }
                                else
                                {
                                    tdStyle.Append(dec.Property + ":" + dec.Value + ";");
                                }
                            }
                            if (tableStyle.Length > 0)
                                table.SetAttributeValue("style", tableStyle.ToString());
                            if (tdStyle.Length > 0)
                                td.SetAttributeValue("style", tdStyle.ToString());
                        }
                    }
                    else
                    {
                        td.SetAttributeValue(att.Name, att.Value);
                    }
                }
            }
            foreach (var child in p.ChildNodes)
            {
                td.AppendChild(child);
            }
            p.ParentNode.ReplaceChild(table, p);
        }



        /*special cases:
         * img: width & height attributes
         * td: align & valign
         * table: cellspacing, border, and padding
         */
        public static void InlineDeclarations(HtmlNode element, List<CssDeclaration> declarations)
        {
            var style = element.Attributes["style"];
            StringBuilder styleVal = null;
            HashSet<string> existingStyles = null;//so we don't overwrite any preexisting inline styles
            if (style != null)
            {
                existingStyles = new HashSet<string>(CssParser.ParseDeclarations(style.Value).Select(dec => dec.Property));
                if (existingStyles.Count > 0)
                {
                    styleVal = new StringBuilder(style.Value.Trim());
                    if (styleVal[styleVal.Length - 1] != ';')
                        styleVal.Append(';');
                }
            }
            var tagName = element.Name.ToLowerInvariant();

            foreach (var declaration in declarations)
            {
                if (existingStyles != null && existingStyles.Contains(declaration.Property))
                    continue;//ignore the duplicate

                if (tagName.EqualsCaseSensitive("td"))
                {
                    if (declaration.Property.EqualsCaseSensitive(CssProperties.TextAlign))
                    {
                        if (!InlineNonStyleAttribute(element, "align", declaration.Value))
                            continue;
                    }
                    else if (declaration.Property.EqualsCaseSensitive(CssProperties.VerticalAlign))
                    {
                        if (!InlineNonStyleAttribute(element, "valign", declaration.Value))
                            continue;
                    }
                }

                if (tagName.EqualsAny("table", "td", "img") && declaration.Property.EqualsAny(CssProperties.Width, CssProperties.Height))
                {
                    if (!InlineNonStyleAttribute(element, declaration.Property, declaration.Value))
                        continue;
                }
                if (tagName.EqualsAny("table") && declaration.Property.EqualsAny(CssProperties.BackgroundColor))
                {
                    if (!InlineNonStyleAttribute(element, "bgcolor", declaration.Value))
                        continue;
                }

                //a nice way to auto insert cellpadding and cellspacing
                if (declaration.Property.StartsWith("-attr-"))
                {
                    InlineNonStyleAttribute(element, declaration.Property.Strip("-attr-"), declaration.Value);
                    continue;
                }
                if (styleVal == null)
                    styleVal = new StringBuilder();

                styleVal.Append(declaration.Property + ":" + declaration.Value + ";");
            }

            if (styleVal != null)
                element.SetAttributeValue("style", styleVal.ToString());
        }

        private static bool InlineNonStyleAttribute(HtmlNode element, string attributeName, string value)
        {
            var att = element.Attributes[attributeName];
            if (att != null && att.Value.HasChars())
                return false;

            element.SetAttributeValue(attributeName, value);
            return true;
        }


        private static IEnumerable<HtmlNode> DescendantElements(HtmlNode element)
        {
            yield return element;
            if (element.HasChildNodes)
            {
                foreach (var child in element.ChildNodes)
                {
                    if (child.NodeType == HtmlNodeType.Element)
                    {
                        foreach (var descendant in DescendantElements(child))
                            yield return descendant;
                    }
                }
            }
        }


    }

}
