using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace Takeoff.Css
{

	public static class CssSerializer
	{
        /// <summary>
        /// Strips out comments and whitespace from the given css markup.
        /// </summary>
        /// <param name="cssSource"></param>
        /// <returns></returns>
        public static string Compact(string cssSource)
        {
            return CssSerializer.Serialize(CssParser.ParseRules(cssSource), false);
        }

		public static string SerializeDeclarations (IList<CssDeclaration> declarations) {
			bool useWhitespace = true;
			StringBuilder sb = new StringBuilder();

			foreach (CssDeclaration currDeclaration in declarations) {
			    string declaration = currDeclaration.Property + ":" + currDeclaration.Value + ";";
			    if (useWhitespace)
			        sb.AppendLine("\t" + declaration);
			    else
			        sb.Append(declaration);
			}
			return sb.ToString();
		}

		public static string SerializeRuleDictionary (CssRuleDictionary rules) {
			bool useWhitespace = true;
			StringBuilder sb = new StringBuilder();

			foreach (string currSelector in rules.GetSelectors()) {
				sb.Append(currSelector);
				if (useWhitespace) {
					sb.AppendLine();
					sb.AppendLine("{");
				}
				else
					sb.Append(" {");

				foreach (CssDeclaration currDeclaration in rules[currSelector]) {

					string declaration = currDeclaration.Property + ":" + currDeclaration.Value + ";";
					if (useWhitespace)
						sb.AppendLine("\t" + declaration);
					else
						sb.Append(declaration);
				}

				if (useWhitespace)
					sb.AppendLine("}");
				else
					sb.Append("} ");
			}

			return sb.ToString();
		}



		public static string Serialize (IList<CssRule> rules) {
			return Serialize(rules,true);
		}
	    public static string Serialize (IList<CssRule> rules, bool useWhitespace) {

			StringBuilder sb = new StringBuilder();

			foreach (CssRule currRule in rules) {
				for (int i = 0; i < currRule.Selectors.Count; i++) {
					if (i > 0)
						sb.Append(", ");
					sb.Append(currRule.Selectors[i].Text);
				}
				if (useWhitespace) {
					sb.AppendLine();
					sb.AppendLine("{");
				}
				else
					sb.Append(" {");

				foreach (CssDeclaration currDeclaration in currRule.Declarations) {

					string declaration = currDeclaration.Property + ":" + currDeclaration.Value + ";";
					if (useWhitespace)
						sb.AppendLine("\t" + declaration);
					else
						sb.Append(declaration);
				}

				if (useWhitespace)
					sb.AppendLine("}");
				else
					sb.Append("} ");
			}

			return sb.ToString();
	    }
	}

}
