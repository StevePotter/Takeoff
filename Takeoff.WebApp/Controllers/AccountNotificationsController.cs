using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mediascend.Web;
using Takeoff.Data;
using Takeoff.Models;
using Recurly;
using MvcContrib;
using Takeoff.ViewModels;

namespace Takeoff.Controllers
{

    /// <summary>
    /// Lets account owners view and modify their email alert settings.
    /// </summary>
    [SubController("/account/notifications", true)]
    [RestrictIdentity(AllowDemo = false)]
    [TrialSignupRequired]
    public class AccountNotificationsController : BasicController
    {
        [OldUrl("/Settings/Notifications")]//emails referenced this old link
        public ActionResult Index()
        {
            var user = this.UserThing();

            var model = new Account_Notifications
            {

            };

            user.GetSettingValue(UserSettings.DigestEmailFrequency.Name).IfNotNull(v =>
            {
                EmailDigestFrequency
                    frequency;
                if (
                    Enum.TryParse(
                        v.ToString(), true,
                        out frequency))
                {
                    model.
                        DigestEmailFrequency
                        = frequency;
                }
            });

            model.NotifyWhenNewVideo = user.GetSettingValue(UserSettings.NotifyWhenNewVideo).CastTo<bool>();
            model.NotifyWhenNewComment = user.GetSettingValue(UserSettings.NotifyWhenNewComment).CastTo<bool>();
            model.NotifyWhenNewCommentReply = user.GetSettingValue(UserSettings.NotifyWhenNewCommentReply).CastTo<bool>();
            model.NotifyWhenNewReplyToAuthoredComment = user.GetSettingValue(UserSettings.NotifyWhenNewReplyToAuthoredComment).CastTo<bool>();
            model.NotifyWhenNewMember = user.GetSettingValue(UserSettings.NotifyWhenNewMember).CastTo<bool>();
            model.NotifyWhenNewFile = user.GetSettingValue(UserSettings.NotifyWhenNewFile).CastTo<bool>();
            model.NotifyForMaintenance = user.GetSettingValue(UserSettings.NotifyForMaintenance).CastTo<bool>();
            model.NotifyForNewFeatures = user.GetSettingValue(UserSettings.NotifyForNewFeatures).CastTo<bool>();
            model.NotifyForPlanChanges = user.GetSettingValue(UserSettings.NotifyForPlanChanges).CastTo<bool>();
            model.NotifyForSpecials = user.GetSettingValue(UserSettings.NotifyForSpecials).CastTo<bool>();

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(FormCollection forms)
        {
            var user = this.UserThing();
            ViewData.Model = user;
            AccountController.UpdateSettingsFromFormCollection(forms, user);
            //no need to return the view because we use the jquery ajax form plugin.  in the future if you wanted, you could check Request.IsAjaxRequest() and return a view with a success message if it isn't
            return this.Empty();
        }


        ///// <summary>
        ///// In the future (make this public) this will let someone unsubscribe 
        ///// from a particular type of email from a link within the email.  It will 
        ///// give them confirmation the email won't be sent anymore but will require them to log in to change any more settings.
        ///// 
        ///// Signatures should be done using md5 with a key generated specifically for that user.
        ///// </summary>
        ///// <param name="what"></param>
        ///// <param name="userId"></param>
        ///// <param name="signature"></param>
        ///// <returns></returns>
        //private ActionResult Delete(string what, int userId, string signature)
        //{
        //    return null;
        //}
     
    }
}
