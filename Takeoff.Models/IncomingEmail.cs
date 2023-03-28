using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Takeoff.Models
{

    public class IncomingMail
    {
        /// <summary>
        /// The message itself. This will be the entire message and will need to be manually parsed by your code. Take a look at the parsing email documentation to see how you can do this if you are not familiar with email messages.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// The plain text extracted from the raw message. This will be the first text/plain part of the email if the message is multipart and the body if the content is not multipart.
        /// </summary>
        public string Plain { get; set; }
        /// <summary>
        /// The html text extracted from the raw message. This will be the first text/html part of an email if the message is multipart.
        /// </summary>
        public string Html { get; set; }
        /// <summary>
        /// The recipient as specified by the server. This will be the CloudMailin email address. This could be different to the TO address specified in the message itself. If you are forwarding email from your domain you will want to extract the recipient from the email.
        /// </summary>
        public string To { get; set; }
        /// <summary>
        /// The disposable part of the email address if it exists. For example if your email address was example+something@example.com, the disposable would contain 'something'.
        /// </summary>
        public string Disposable { get; set; }
        /// <summary>
        /// The sender as specified by the server. This can differ from the sender specified within the message.
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// The subject of the message extracted from the message itself.
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// The signature of the message encoded with your private key. You can use this to ensure that the HTTP POST data has come from CloudMailin. See here for more details.
        /// </summary>
        public string Signature { get; set; }
        /// <summary>
        /// This will send the x-forwarded-for header from the mail message if it is present. This is often included by forwarding email servers to show the original to address the message was sent to. This parameter will likely always be experimental as only some servers will include this header.
        /// </summary>
        public string X_Forwarded_For { get; set; }
        /// <summary>
        /// This will send the x-sender header from the mail message if it is present. This parameter will likely always be experimental as only some servers will include this header.
        /// </summary>
        public string X_Sender { get; set; }
        /// <summary>
        /// This is a new header that is being tested and we would appreciate feedback for. The to addresses found in the message header will be listed separated by a comma.
        /// </summary>
        public string X_Header_To { get; set; }
        /// <summary>
        /// This is a new header that is being tested and we would appreciate feedback for. The CC addresses found in the message header will be listed separated by a comma.
        /// </summary>
        public string X_Header_CC { get; set; }
    }

    /// <summary>
    /// Contains the information added to the email address that will indicate what action to perform, what the data is, and who the user is.
    /// NOTE: none of the fields can have an underscore!  Instead use - or .
    /// </summary>
    public class IncomingMailAction
    {
        public string Action { get; set; }
        public string Data { get; set; }
        public string UserId { get; set; }

        public string ToEmailDisposableField()
        {
            var fields = string.Format("{0}_{1}_{2}", Action.CharsOrEmpty(), Data.CharsOrEmpty(), UserId.CharsOrEmpty());
            return fields.AppendWith("_").AppendWith(Signature());
        }

        public string GetEmailAddress()
        {
            return "{0}+{1}@{2}".FormatString(IncomingMailUtil.CloudMailInEmailAddress.Before("@"), ToEmailDisposableField(), IncomingMailUtil.CloudMailInEmailAddress.After("@"));
        }


        public string Signature()
        {
            var fields = string.Format("{0}_{1}_{2}", Action.CharsOrEmpty(), Data.CharsOrEmpty(), UserId.CharsOrEmpty());
            return fields.AppendWith(IncomingMailUtil.CloudMailInIdentityValidationSecret).MD5Hash();
        }


        /// <summary>
        /// Attempts to parse the "disposable" part of an email address into the action.  Returns null if it doesn't work.
        /// </summary>
        /// <param name="disposableField"></param>
        /// <returns></returns>
        public static IncomingMailAction Parse(string disposableField)
        {
            var fields = disposableField.Split(new char[] {'_'}, StringSplitOptions.None);
            if ( fields.Length != 4)
                return null;
            var action = new IncomingMailAction {Action = fields[0], Data = fields[1], UserId = fields[2]};
            var incomingSignature = fields[3];

            var desiredSignature = action.Signature();

            if (!incomingSignature.EqualsCaseSensitive(desiredSignature))
            {
                return null;
            }
            return action;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// </remarks>
    public static class IncomingMailUtil
    {
        /// <summary>
        /// The secret key used to verify requests from cloudmailin.net.  This is provided by cloudmailin.
        /// </summary>
        public static string CloudMailInRequestValidationRequestValidationSecret
        {
            get
            {
                if (_CloudMailInRequestValidationSecret == null)
                {
                    _CloudMailInRequestValidationSecret = ConfigurationManager.AppSettings["CloudMailInRequestValidationSecret"].CharsOrEmpty();
                }
                return _CloudMailInRequestValidationSecret;
            }
            set
            {
                _CloudMailInRequestValidationSecret = value;
            }
        }
        private static string _CloudMailInRequestValidationSecret;

        /// <summary>
        /// Used to generate a signature to make sure nobody hacks an incoming email address (the "disposable part" of it) to fake an identity.
        /// </summary>
        public static string CloudMailInIdentityValidationSecret
        {
            get
            {
                if (_CloudMailInIdentityValidationSecret == null)
                {
                    _CloudMailInIdentityValidationSecret = ConfigurationManager.AppSettings["CloudMailInIdentityValidationSecret"].CharsOrEmpty();
                }
                return _CloudMailInIdentityValidationSecret;
            }
            set
            {
                _CloudMailInIdentityValidationSecret = value;
            }
        }
        private static string _CloudMailInIdentityValidationSecret;
                
        public static string CloudMailInEmailAddress
        {
            get
            {
                if (_CloudMailInEmailAddress == null)
                {
                    _CloudMailInEmailAddress = ConfigurationManager.AppSettings["CloudMailInEmailAddress"].CharsOrEmpty();
                }
                return _CloudMailInEmailAddress;
            }
            set
            {
                _CloudMailInEmailAddress = value;
            }
        }
        private static string _CloudMailInEmailAddress;


        /// <summary>
        /// Determines whether the signature sent in matches the actual computed signature.  If false, this is a bad request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsValidRequest(HttpRequestBase request)
        {
            //as per http://docs.cloudmailin.com/validating_the_sender
            var values = string.Join(string.Empty, request.Form.AllKeys.Where(k => !k.EqualsCaseInsensitive("signature")).OrderBy(k => k).Select(key => request.Form[key]).ToArray());
            var calculatedSignature = values.AppendWith(CloudMailInRequestValidationRequestValidationSecret).MD5Hash();
            var signature = request.Form["signature"];
            return signature.EqualsCaseSensitive(calculatedSignature);
        }


    }
}
