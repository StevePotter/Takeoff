using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Net.Mail;
using System.Text;
using System.Threading;
using Amazon.S3.Model;
using Amazon.SimpleEmail.Model;
using Newtonsoft.Json;
using Takeoff.Models.Data;
using Takeoff.CssInliner;
using Amazon.S3;

namespace Takeoff.Models
{
    /// <summary>
    /// A simple mail message class.  This is needed because we have 3 apis to send mail to: postmark, amazon, and smtp.  This is the base format that can convert between all others.
    /// </summary>
    /// <remarks>
    /// Addresses can be simple email addresses or can include a displayname.  Displayname is like this {displayName} <{email}>.
    /// </remarks>
    public class OutgoingMessage
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

        /// <summary>
        /// When true, a small gif was added to track email opens.
        /// </summary>
        public bool IncludedTrackingImage { get; set; }

        /// <summary>
        /// The vendor used to send this email. OutgoingEmail.AmazonVendorName is an example.
        /// </summary>
        public OutgoingMailProvider Provider { get; set; }

        /// <summary>
        /// If this email was sent as part of a job, this is the job's ID.
        /// </summary>
        public Guid? JobId { get; set; }

        /// <summary>
        /// Normally set during LogMail, but this can be set before calling Send, this way a job can link to an email before the email was actually logged.
        /// </summary>
        public Guid? Id { get; set; }
    }

    /// <summary>
    /// Manages all of our outbound email.
    /// </summary>
    /// <remarks>
    /// We support multiple email providers:  amazon simple email service API, postmarkapp.com API, and smtp.  Amazon is the provider of choice because they are cheap and their mail rarely bounces.  
    /// But they have rate limiting.  To avoid breaking rate limits, all email is sent from a single place.  So, when you call OutgoingMail.Send, it will, by default, 
    /// send a message via MSMQ that contains the message.  Then a service picks up that message, sends the email, logs the email, and pauses a bit to honor rate limiting.
    /// 
    /// Funneling through a single service also speeds things up, since sending a MSMQ message is quick and sending an email might not be.  It also allows us to wait if there is a problem, without blocking http requests or system jobs.
    /// 
    /// Outgoing messages are each given a guid and logged into the database.  A copy of the html, plain text, as well as a json serialization of the entire message record, is uploaded to S3 for later retrieval.
    /// </remarks>
    public static class OutgoingMail
    {
        public const string LinkClickQueryParameter = @"_messageId";

        /// <summary>
        /// Indicates whether any actual email will be set.  This is usef for testing.
        /// </summary>
        public static bool EnableOutgoingMail
        {
            get
            {
                if (!_EnableOutgoingMail.HasValue)
                {
                    _EnableOutgoingMail = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableOutgoingMail"].CharsOr("false"));
                }
                return _EnableOutgoingMail.GetValueOrDefault(true);
            }
            set
            {
                _EnableOutgoingMail = value;
            }
        }
        private static bool? _EnableOutgoingMail;


        /// <summary>
        /// When true, calls to Send will put each email in MSMQ to be picked up for actual sending by a background service.  
        /// All mail should be funneled through MSMQ, primarily to avoid hitting rate limits.
        /// </summary>
        public static bool FunnelThroughQueue
        {
            get
            {
                if (!_FunnelThroughQueue.HasValue)
                {
                    _FunnelThroughQueue = Convert.ToBoolean(ConfigurationManager.AppSettings["OutgoingMailFunnelsThroughQueue"].CharsOr("true"));
                }
                return _FunnelThroughQueue.GetValueOrDefault(true);
            }
            set
            {
                _FunnelThroughQueue = value;
            }
        }
        private static bool? _FunnelThroughQueue;

        

        /// <summary>
        /// Intended for testing, this will only send mail to email addresses specified.
        /// </summary>
        public static HashSet<string> LimitOutgoingMailToAddresses
        {
            get
            {
                if (_LimitOutgoingMailToAddresses == null)
                {
                    _LimitOutgoingMailToAddresses = new HashSet<string>();
                    var addresses = ConfigurationManager.AppSettings["LimitOutgoingMailToAddresses"];
                    if ( addresses.HasChars())
                    {
                        addresses.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => s.HasChars()).Each(s => _LimitOutgoingMailToAddresses.Add(s));
                    }
                }
                return _LimitOutgoingMailToAddresses;
            }
            set
            {
                _LimitOutgoingMailToAddresses = value;
            }
        }
        private static HashSet<string> _LimitOutgoingMailToAddresses;

        /// <summary>
        /// Default outgoing email provider.
        /// </summary>
        public static OutgoingMailProvider OutgoingMailProvider
        {
            get
            {
                if (!_outgoingMailProvider.HasValue)
                {
                    _outgoingMailProvider = Enum<OutgoingMailProvider>.Parse(ConfigurationManager.AppSettings["OutgoingMailProvider"].CharsOr("Smtp"));
                }
                return _outgoingMailProvider.Value;
            }
            set
            {
                _outgoingMailProvider = value;
            }
        }
        private static OutgoingMailProvider? _outgoingMailProvider;

        public static MessageQueue MessageQueue
        {
            get
            {
                if ( _MessageQueue == null)
                    _MessageQueue = new MessageQueue(ConfigUtil.GetRequiredAppSetting("OutgoingMailQueue"));
                return _MessageQueue;
            }
        }
        private static MessageQueue _MessageQueue;

        /// <summary>
        /// Sends an email, possibly going through an intermediate queue.
        /// </summary>
        /// <param name="message"></param>
        public static void Send(OutgoingMessage message)
        {
            if (FunnelThroughQueue)
            {
                OutgoingMail.QueueSend(message);
            }
            else
            {
                OutgoingMail.SendNow(message);
                OutgoingMail.LogMail(message);
            }
        }

        /// <summary>
        /// Puts the message into MSMQ, where is it picked up by a service and actually sent.  This should always be done because 
        /// it funnels all email to a single place, which will prevent busting through rate limits.  It's an intermediate step but a safe one.
        /// </summary>
        /// <param name="queueName"></param>
        public static void QueueSend(OutgoingMessage mail)
        {
            //as per https://connect.microsoft.com/VisualStudio/feedback/details/94943/messagequeue-send-crashes-if-called-by-multiple-threads, you gotta use a message object
            MessageQueue.Send(new System.Messaging.Message(JsonConvert.SerializeObject(mail)));
        }


        /// <summary>
        /// Logs an email that was sent or not.  This adds a record to the database as well as in S3.
        /// </summary>
        public static void LogMail(OutgoingMessage message)
        {
            if (!message.Id.HasValue)
                message.Id = Guid.NewGuid();

            using (var db = new DataModel())
            {
                var emailLog = new OutgoingEmailLog()
                {
                    SendAttemptedOn = message.SendAttemptedOn,
                    SentOn = message.SentOn,
                    Template = message.Template,
                    ToUserId = message.ToUserId,
                    Error = message.SendError,
                    ToAddress = message.To,
                    Provider = message.Provider.ToString(),
                    IncludedTrackingImage = message.IncludedTrackingImage,
                    OpenCount = message.IncludedTrackingImage ? 0 : new int?(),
                    JobId = message.JobId,
                    Id = message.Id.Value,

                };
                db.OutgoingEmailLogs.InsertOnSubmit(emailLog);
                db.SubmitChanges();
                var id = emailLog.Id;
                var path = message.Template.HasChars()
                               ? string.Format(@"{0}/{1}.", message.Template, id.ToString())
                               : string.Format(@"{0}.", id.ToString());
                var uploadBucket = ConfigurationManager.AppSettings["OutgoingMailLogBucket"];
                using (var s3 = new Amazon.S3.AmazonS3Client(ConfigurationManager.AppSettings["AmazonAccessKey"], ConfigurationManager.AppSettings["AmazonSecretKey"]))
                {
                    s3.PutObject(new PutObjectRequest
                    {
                        BucketName = uploadBucket,
                        CannedACL = S3CannedACL.AuthenticatedRead,
                        Key = path + "json",
                        ContentType = @"application/json",
                        ContentBody = JsonConvert.SerializeObject(message, Formatting.Indented),
                    });
                    if (message.HtmlBody.HasChars())
                    {
                        s3.PutObject(new PutObjectRequest
                        {
                            BucketName = uploadBucket,
                            CannedACL = S3CannedACL.AuthenticatedRead,
                            Key = path + "html",
                            ContentBody = message.HtmlBody,
                            ContentType = @"text/html"
                        });
                    }
                    if (message.TextBody.HasChars())
                    {
                        s3.PutObject(new PutObjectRequest
                        {
                            BucketName = uploadBucket,
                            CannedACL = S3CannedACL.AuthenticatedRead,
                            Key = path + "txt",
                            ContentBody = message.TextBody,
                            ContentType = @"text/plain"
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Gets the S3 url to the html,txt,or json source for a given message.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string MessageLogUrl(Guid id, string extension)
        {
            using (var db = DataModel.ReadOnly)
            {
                var message = db.OutgoingEmailLogs.Where(e => e.Id == id).FirstOrDefault();
                if (message == null)
                    return null;
                var uploadBucket = ConfigurationManager.AppSettings["OutgoingMailLogBucket"];
                var path = message.Template.HasChars()
                               ? string.Format(@"{0}/{1}.{2}", message.Template, id, extension)
                               : string.Format(@"{0}.{1}", id, extension);
                return new S3FileLocation
                             { 
                                 Bucket = uploadBucket,
                                 Key = path,
                             }.GetAuthorizedUrl(TimeSpan.FromDays(1));
            }
        }

        /// <summary>
        /// Increments the number of times a message has been viewed (opened).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool LogOpen(Guid id, DateTime? date)
        {
            using (var db = new DataModel())
            {
                var message = db.OutgoingEmailLogs.Where(e => e.Id == id).FirstOrDefault();
                if (message == null)
                    return false;
                message.OpenCount = message.OpenCount.GetValueOrDefault() + 1;
                db.OutgoingEmailOpenLogs.InsertOnSubmit(new OutgoingEmailOpenLog
                                                            {
                                                                Date = date,
                                                                MessageId = id
                                                            });
                db.SubmitChanges();
                return true;
            }
        }


        /// <summary>
        /// Logs a click within a mail message.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool LogLinkClick(Guid id, string url, DateTime? date)
        {
            using (var db = new DataModel())
            {
                var message = db.OutgoingEmailLogs.Where(e => e.Id == id).FirstOrDefault();
                if (message == null)
                    return false;
                if (message.OpenCount.GetValueOrDefault() == 0)
                {
                    //happens if they click a link but don't download images.  happens all the time in outlook
                    LogOpen(id, date);
                }
                message.LinkClickCount = message.LinkClickCount.GetValueOrDefault() + 1;
                db.OutgoingEmailLinkClickLogs.InsertOnSubmit(new OutgoingEmailLinkClickLog
                {
                    Date = date,
                    MessageId = id,
                    Url = url,
                });

                db.SubmitChanges();
                return true;
            }
        }

        /// <summary>
        /// Sends the given message using the provider specified in settings/.  Does not log it.  That's a separate call.
        /// </summary>
        /// <param name="message"></param>
        public static bool SendNow(OutgoingMessage message)
        {
            return SendNow(message, OutgoingMailProvider);
        }

        /// <summary>
        /// Sends the given message.  Does not log it.  That's a separate call.
        /// </summary>
        public static bool SendNow(OutgoingMessage message, OutgoingMailProvider provider)
        {
            ValidateMessage(message);
            message.SendAttemptedOn = DateTime.UtcNow;
            message.Provider = provider;
            if (!EnableOutgoingMail)
                return false;//useful when debugging or dev without internet (avoids exceptions when sending mail).

            var limitToAddresses = LimitOutgoingMailToAddresses;
            if ( limitToAddresses.Count > 0 && !limitToAddresses.Contains(message.To))
            {
                return false;
            }

            //todo: would be nice to catch exceptions and retry later, notify us, or use a different provider
            switch (provider)
            {
                case OutgoingMailProvider.AwsSes:
                    SendMailAws(message);
                    break;
                case OutgoingMailProvider.PostmarkApi:
                    throw new NotImplementedException("nuget package screwed up dependencies so fix that and you can put this postmark shit back");
//                    SendMailPostmarkAPI(message);
                    break;
                case OutgoingMailProvider.Smtp:
                    SendMailSmtp(message);
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
            message.SentOn = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Takes the results of a fully rendered email view and gets teh subject, plain body, and html
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static OutgoingMessage ParseViewOutput(Guid messageId, string source)
        {
            var parts = source.Split(new string[] {"-----------EMAIL_PART-----------"}, StringSplitOptions.None);
            var subject = parts[0].Trim();
            var plain = parts[1].Trim();
            var html = parts[2].Trim();
            if ( html.HasChars(CharsThatMatter.Letters))
            {
                var doc = CssInliner.CssInliner.InlineHtml3(html, null);
                foreach (var a in doc.DocumentNode.SelectNodes("//a"))
                {
                    var href = a.GetAttributeValue("href", null);
                    if ( href.HasChars() && href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        var formatString = href.Contains("?") ? "{0}&{1}={2}" : "{0}?{1}={2}";
                        a.SetAttributeValue("href", formatString.FormatString(href,LinkClickQueryParameter, messageId));
                    }
                }
                html = doc.GetHtml();

            }
            return new OutgoingMessage
                       {
                           Id = messageId,
                           Subject = subject.CharsOrNull(),
                           HtmlBody = html.CharsOrNull(),
                           TextBody = plain.CharsOrNull(),
                       };
        }

        private static void ValidateMessage(OutgoingMessage mail)
        {
            if (!mail.From.HasChars())
            {
                var fromAddress = ConfigUtil.GetRequiredAppSetting("SendMailFromAddress");
                mail.From = string.Format("{0} <{1}>", ConfigUtil.GetRequiredAppSetting("SendMailFromDefaultDisplayName"), fromAddress);
            }
        }

        //private static void SendMailPostmarkAPI(OutgoingMessage mail)
        //{
        //    PostmarkDotNet.PostmarkMessage postmarkMessage = new PostmarkMessage
        //    {
        //        From = mail.From,
        //        To = mail.To,
        //        Cc = mail.Cc,
        //        Bcc = mail.Bcc,
        //        ReplyTo = mail.ReplyTo,
        //        Subject = mail.Subject,
        //        HtmlBody = mail.HtmlBody,
        //        TextBody = mail.TextBody,
        //    };
        //    var postmarkClient =
        //        new PostmarkDotNet.PostmarkClient(ConfigurationManager.AppSettings["PostmarkAPIKey"]);
        //    postmarkClient.SendMessage(postmarkMessage);
        //}

        private static void SendMailAws(OutgoingMessage mail)
        {
            var request = new Amazon.SimpleEmail.Model.SendEmailRequest
            {
                Message = new Amazon.SimpleEmail.Model.Message
                {

                    Body = new Amazon.SimpleEmail.Model.Body(),
                },
                Destination = new Destination(),
            };

            if (mail.Subject.HasChars())
                request.Message.Subject = new Content(mail.Subject);
            if (mail.From.HasChars())
                request.Source = mail.From;
            if (mail.ReplyTo.HasChars())
                request.ReplyToAddresses = mail.ReplyTo.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (mail.To.HasChars())
                request.Destination.ToAddresses = mail.To.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (mail.Cc.HasChars())
                request.Destination.CcAddresses = mail.Cc.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (mail.Bcc.HasChars())
                request.Destination.BccAddresses = mail.Bcc.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            request.ReturnPath = "notifications@takeoffvideo.com";
            if (mail.HtmlBody.HasChars())
            {
                request.Message.Body.Html = new Content(mail.HtmlBody);
            }
            if (mail.TextBody.HasChars())
            {
                request.Message.Body.Text = new Content(mail.TextBody);
            }


            using (var client = new Amazon.SimpleEmail.AmazonSimpleEmailServiceClient(ConfigurationManager.AppSettings["AmazonAccessKey"], ConfigurationManager.AppSettings["AmazonSecretKey"]))
            {
                try
                {
                    var response = client.SendEmail(request);
                }
                catch (Exception ex)
                {
                    Thread.Sleep(1500); //for their timeout.  todo: make this smarter
                    client.SendEmail(request);
                }
            }
        }

        private static void SendMailSmtp(OutgoingMessage mail)
        {
            var message = new MailMessage { Subject = mail.Subject };
            if (mail.From.HasChars())
                message.From = ParseMailAddress(mail.From);
            if (mail.ReplyTo.HasChars())
                ParseMailAddress(mail.ReplyTo).AddTo(message.ReplyToList);
            if (mail.To.HasChars())
                ParseMailAddresses(mail.To).AddAllTo(message.To);
            if (mail.Cc.HasChars())
                ParseMailAddresses(mail.Cc).AddAllTo(message.CC);
            if (mail.Bcc.HasChars())
                ParseMailAddresses(mail.Bcc).AddAllTo(message.Bcc);

            if (mail.HtmlBody.HasChars())
            {
                message.IsBodyHtml = true;
                message.Body = mail.HtmlBody;
                if (mail.TextBody.HasChars())
                    message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(mail.TextBody, null, System.Net.Mime.MediaTypeNames.Text.Plain));
                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(mail.HtmlBody, null, System.Net.Mime.MediaTypeNames.Text.Html));
            }
            else if (mail.TextBody.HasChars())
            {
                message.IsBodyHtml = false;
                message.Body = mail.TextBody;
                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(mail.TextBody, null, System.Net.Mime.MediaTypeNames.Text.Plain));
            }

            SendSmtp(message);
        }

        /// <summary>
        /// Sends the mailmessage via our configured smtp provider.
        /// </summary>
        /// <param name="mail"></param>
        private static void SendSmtp(MailMessage mail)
        {
            if (!EnableOutgoingMail)
                return;//useful when debugging or dev without internet (avoids exceptions when sending mail).

            //send the message
            var mailHost = ConfigurationManager.AppSettings["SmtpHost"];
            if (string.IsNullOrEmpty(mailHost))
                throw new Exception("'SmtpHost' appSetting was missing.");

            bool useSsl = false;
            var strUseSsl = ConfigurationManager.AppSettings["SmtpUseSsl"];
            if (!string.IsNullOrEmpty(strUseSsl))
            {
                try
                {
                    useSsl = bool.Parse(strUseSsl);
                }
                catch (Exception ex)
                {
                    throw new Exception("AppSetting 'SmtpUseSsl' could not be converted to boolean.", ex);
                }
            }

            SmtpClient smtp = new SmtpClient(ConfigUtil.GetRequiredAppSetting("SmtpHost"))
            {
                Credentials = new System.Net.NetworkCredential(ConfigUtil.GetRequiredAppSetting("SmtpUserName"), ConfigUtil.GetRequiredAppSetting("SmtpPassword")),
                Port = ConfigUtil.GetRequiredAppSetting("SmtpPort").ToInt(),
                EnableSsl = useSsl
            };
            smtp.Send(mail);
        }


        static IEnumerable<MailAddress> ParseMailAddresses(string commaSeparatedAddresses)
        {
            foreach (var address in commaSeparatedAddresses.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                yield return ParseMailAddress(address);

            }
        }

        static MailAddress ParseMailAddress(string addressAndMaybeDisplayString)
        {
            //an address could be a simple email and/or displayname.  when it has a displayname, the format is {name} <{address}>

            string email;
            string displayName = addressAndMaybeDisplayString.Before("<");
            if (displayName.HasChars())
            {
                displayName = displayName.Trim();
                email = addressAndMaybeDisplayString.After("<").Before(">").Trim();
                return new MailAddress(email, displayName);
            }
            return new MailAddress(addressAndMaybeDisplayString.Trim());
        }

        /// <summary>
        /// Helper function for formatting plain text email bodies with key/value pairs separated by lines.  Intended mainly for reporting and logging stuff.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string FormatPlainTextBodyForNameValuePairs(IEnumerable<KeyValuePair<string,object>> values)
        {
            if (values == null)
                return string.Empty;
            return values.Select(pair =>
                              {
                                  string value = string.Empty;
                                  if (pair.Value == null)
                                      value = "[null]";
                                  else if (value is DateTime)
                                      value = value.CastTo<DateTime>().ToString(DateTimeFormat.ShortDateTime);
                                  else
                                      value = Convert.ToString(pair.Value);
                                  return pair.Key.CharsOrEmpty() + ": " + value;
                              }).Join(Environment.NewLine);
        }

        private static string FormatParameterValueForEmail(object value)
        {
            if (value == null)
                return "[null]";

            if (value is DateTime)
            {
                return value.CastTo<DateTime>().ToString(DateTimeFormat.ShortDateTime);
            }
            return Convert.ToString(value);
        }


    }

    public enum OutgoingMailProvider
    {
        NotSet,
        PostmarkApi,
        Smtp,
        AwsSes
    }

}
