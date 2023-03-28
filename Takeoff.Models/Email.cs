using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Takeoff.Models
{
    /// <summary>
    /// A simple mail message class.  This is needed because we have 3 apis to send mail to: postmark, amazon, and smtp.  This is the base format that can convert between all others.
    /// </summary>
    /// <remarks>
    /// Addresses can be simple email addresses or can include a displayname.  Displayname is like this {displayName} <{email}>.
    /// </remarks>
    public class Email
    {

        /// <summary>
        /// The sender's email address.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Any recipients. Separate multiple recipients with a comma.
        /// </summary>
        public string To { get; set; }

        /// <summary>
        ///   Any CC recipients. Separate multiple recipients with a comma.
        /// </summary>
        public string Cc { get; set; }

        /// <summary>
        ///   Any BCC recipients. Separate multiple recipients with a comma.
        /// </summary>
        public string Bcc { get; set; }

        /// <summary>
        ///   The email address to reply to. This is optional.
        /// </summary>
        public string ReplyTo { get; set; }

        /// <summary>
        ///   The message subject line.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        ///   The message body, if the message contains HTML.
        /// </summary>
        public string HtmlBody { get; set; }

        /// <summary>
        ///   The message body, if the message is plain text.
        /// </summary>
        public string TextBody { get; set; }

        /// <summary>
        /// UTC time when the email was submitted for sending.  Useful for logging.
        /// </summary>
        public DateTime? SendAttemptedOn { get; set; }

        /// <summary>
        /// UTC time when the email was sent.  Useful for logging.
        /// </summary>
        public DateTime? SentOn { get; set; }

        /// <summary>
        /// The name of the type of email that was sent, such as "PasswordReset".  Useful for logging.
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// The ID of the user that the email was sent to.  Useful for logging.
        /// </summary>
        public int? ToUserId { get; set; }

        /// <summary>
        /// An error type/message given when an attempt to send was made.
        /// </summary>
        public string SendError { get; set; }
    }

}
