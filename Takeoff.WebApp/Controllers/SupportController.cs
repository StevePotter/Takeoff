using System;
using System.Text;
using System.Web;
using System.Web.Mvc;

using Mediascend.Web;
using Takeoff.Models;
using MvcContrib;
using Takeoff.ViewModels;
using System.Configuration;

namespace Takeoff.Controllers
{
    public class SupportController : BasicController
    {
        static readonly string RequestUrl = ConfigUtil.GetRequiredAppSetting("ZendeskRequestUrl");
        static readonly string SuggestionsUrl = ConfigUtil.GetRequiredAppSetting("ZendeskSuggestionsUrl");
        static readonly string KnowledgeBaseUrl = ConfigUtil.GetRequiredAppSetting("ZendeskKnowledgeBaseUrl");
        static readonly string DiscussionsUrl = ConfigUtil.GetRequiredAppSetting("ZendeskDiscussionsUrl");

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Directs them to zendesk to create a new support request (ticket).
        /// </summary>
        /// <returns></returns>
        [RestrictIdentity(AllowDemo = false)]
        [TrialSignupRequired]
        [ActionName("Request")]
        public ActionResult RequestAction()
        {
            return SendTo(RequestUrl);
        }

        /// <summary>
        /// Directs them to the suggestions forum. If they are logged in, it will log them into zendesk as well
        /// </summary>
        /// <returns></returns>
        public ActionResult Suggestions()
        {
            return SendTo(SuggestionsUrl);
        }

        /// <summary>
        /// Directs them to the knowledgebase where they can find tips and tricks and how tos
        /// </summary>
        /// <returns></returns>
        public ActionResult KnowledgeBase()
        {
            return SendTo(KnowledgeBaseUrl);
        }


        /// <summary>
        /// Directs them to the forum to ask questions that anyone can answer.
        /// </summary>
        /// <returns></returns>
        public ActionResult Discussions()
        {
            return SendTo(DiscussionsUrl);
        }

        ActionResult SendTo(string url)
        {
            var user = this.UserThing();
            if (user == null || !user.Email.HasChars())//demo accounts have no email
                return Redirect(url);//this way they can browse suggestions but have to log in to view one
            else
                return Redirect(BuildZendeskAuthUrl(user.DisplayName, user.Email, user.Id.ToInvariant(), null, url));
        }

        /// <summary>
        /// Users who aren't signed into zendesk are redirected here.  It will pass in the given parameters.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="heading"></param>
        /// <param name="returnUrl"></param>
        /// <param name="signupSource"></param>
        /// <returns></returns>
        /// <remarks>
        /// This performs a redirect to the main login page, which will then subsequently redirect to AfterLogin.  These two redirects could have been avoided but I would have 
        /// needed to copy thte login page code.  This was the quick way although it has two unnecessary redirects.
        /// </remarks>
        [HttpGet]
        public ActionResult Login(string timestamp, string return_to)
        {
            var redirectUrl = Url.Action<SupportController>(c => c.AfterLogin(timestamp, return_to));
            return this.RedirectToAction<AccountController>(c => c.Login(new Account_Login { Heading = "Please log in to continue.", ReturnUrl = redirectUrl }), UrlType.RelativeHttps);
        }

        [RestrictIdentity]
        public ActionResult AfterLogin(string timestamp, string return_to)
        {
            var user = this.UserThing();
            return Redirect(BuildZendeskAuthUrl(user.DisplayName, user.Email, user.Id.ToInvariant(), timestamp, return_to));
        }


        /// <summary>
        /// Builds a url that will allow remote authentication for the current user to log into zendesk.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Documentation at http://www.zendesk.com/api/remote-authentication
        /// </remarks>
        /// <param name="userName"></param>
        /// <param name="userEmail"></param>
        /// <param name="userId"></param>
        /// <param name="timestamp">The timestamp passed in from zendesk after the user tried to access an zendesk page that requires login.  This could be null by the way, in the situation where you provide a link from Takeoff like "Submit a bug"</param>
        /// <param name="redirectTo"></param>
        public static string BuildZendeskAuthUrl(string userName, string userEmail, string userId, string timestamp, string redirectTo)
        {
            //zendesk requires the name be at least two chars
            if (string.IsNullOrEmpty(userName) || userName.Length < 2)
                userName = userEmail;

            if (string.IsNullOrEmpty(timestamp))
            {
                timestamp = ((long)DateTime.UtcNow.SinceEpoch().TotalSeconds).ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            var message = userName + userEmail + (userId ?? string.Empty) + ConfigUtil.GetRequiredAppSetting("ZendeskAuthToken") + timestamp;
            var hash = message.MD5Hash();
            StringBuilder url = new StringBuilder(ConfigUtil.GetRequiredAppSetting("ZendeskAuthUrl").EndWith("?"));
            url.Append("name=" + HttpUtility.UrlEncode(userName) + "&email=" + HttpUtility.UrlEncode(userEmail) + "&timestamp=" + timestamp + "&hash=" + hash);

            if (redirectTo.HasChars())
                url.Append("&return_to=" + redirectTo);
            if (userId.HasChars())
                url.Append("&external_id=" + userId);
            return url.ToString();
        }


    }
}
