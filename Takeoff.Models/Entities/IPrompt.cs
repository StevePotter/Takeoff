using System;
using System.Collections.Generic;
using System.IO;

namespace Takeoff.Data
{

    public interface IPrompt : ITypicalEntity
    {
        /// <summary>
        /// Indicates a url that the user will be redirected to when they next log in.
        /// </summary>
        string RedirectUrl { get; set; }

        string BannerHtml { get; set; }
        bool HtmlEncodeBanner { get; set; }

        
    }


}