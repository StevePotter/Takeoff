using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Takeoff.Views.Helpers
{
    public static class ExtensionMethodsForViews
    {
        public static string PercentString(this int value, int outOf)
        {
            return ((double)value / (double)outOf).ToString("p0");
        }

        public static string PercentString(this long value, long outOf)
        {
            return ((double)value / (double)outOf).ToString("p0");
        }

        public static int Percent(this int value, int outOf)
        {
            return (int)Math.Round((double) value/(double) outOf*100.0);
        }

    
        public static IHtmlString RawHtml(this string html)
        {
            return new HtmlString(html);
        }
    }
}