using System;
using System.Collections.Generic;
using System.IO;

namespace Takeoff.Data
{

    /// <summary>
    /// A screen shown to users when they log into the app.  A prompt is shown once, then automatically deleted.
    /// </summary>
    /// <remarks>
    /// 
    /// In the future, you can build in prompts like:
    /// - redirect.  The user is redirected to some specified url.
    /// - html.  Html, potentially wtih a master page specified, can be provided.  No changes to app deployment are necessary.
    /// - banner.  A banner at the top, potentially closable, is provided.
    /// </remarks>
    public interface IViewPrompt : ITypicalEntity
    {
        /// <summary>
        /// The name of the view.  Must be in the Views/Prompts folder.
        /// </summary>
        string View { get; set; }

        /// <summary>
        /// When set, the prompt will be ignored and auto deleted if the user never logs in before this date.
        /// </summary>
        DateTime? ExpiresOn { get; set; }

        /// <summary>
        /// When set, the prompt will be ignored but not deleted if the user logs in before this date.  This is useful for product announcements that haven't taken effect yet.
        /// </summary>
        DateTime? StartsOn { get; set; }


    }

}