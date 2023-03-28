using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Takeoff.ViewModels
{
    /// <summary>
    /// Defines a success, info, warning, or error message.
    /// </summary>
    /// <remarks>Properties are objects but really, they should be either string or IHtmlString.</remarks>
    public class Message
    {
        /// <summary>
        /// The title of the page (TITLE tag in HEAD).
        /// </summary>
        public string PageTitle { get; set; }

        public string Heading { get; set; }

        public string Text { get; set; }

    }




    public enum StartupMessageType
    {
        NotSet,
        Warning,
        Error,
        Success,
        Info
    }

    /// <summary>
    /// Defines a model that includes a startup message shown to the user. 
    /// </summary>
    /// <remarks>In the future you might want to extract an interface in case this doesn't fit well into the object model.</remarks>
    public class StartupMessage 
    {
        public StartupMessageType StartupMessageType { get; set; }

        public string StartupMessageBody { get; set; }

    }


}