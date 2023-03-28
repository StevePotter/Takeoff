using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Security;
using Takeoff.Controllers;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.ViewModels;

namespace Takeoff
{

    /// <summary>
    /// This filter sets viewdata stuff that is useful for all views.
    /// </summary>
    public class SetViewDataAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            filterContext.Controller.IfType<Controller>(controller => FillViewData(controller.ViewData, filterContext.HttpContext, filterContext.ActionDescriptor));
        }

        /// <summary>
        /// This is useful for setting the user information on views returned from action filters, such as "Not Available in Demo Mode"
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            filterContext.Result.IfType<ViewResult>(
                result => FillViewData(result.ViewData, filterContext.HttpContext, null));
        }

        public static ViewDataDictionary FillViewData(ViewDataDictionary viewData, HttpContextBase context, ActionDescriptor descriptor)
        {
            if (!viewData.ContainsKey("Identity") && context != null)
            {
                context.Identity().IfNotNull(u => viewData.Add("Identity", u));
            }
            if (!viewData.ContainsKey("User") && context != null)
            {
                context.UserThing().IfNotNull(u => viewData.Add("User", u));
            }
            if (descriptor != null)
            {
                //use Ensure in case an action set the value already, which does happen
                viewData.Ensure("ActionName", () => descriptor.ActionName);
                viewData.Ensure("ControllerName", () => descriptor.ControllerDescriptor.ControllerName);
            }

            //it will already be set for certain pages like production details, membership requests, pw prompt, etc
            //the rule for getting an app logo is:
            //1.  if the user is on an app-specific page like Production, then use the production's account
            //2.  if user has an account, use their account logo
            //3.  if the user is associated with only one account, use that account's logo

            if (!viewData.ContainsKey(AppLogoHelper.AppLogoKey) && context != null)
            {
                var fromContext = context.Items[AppLogoHelper.AppLogoKey];
                IImage logo = null;
                if (fromContext == null)
                {
                    var user = context.UserThing2();
                    if (user != null)
                    {
                        var account = user.Account;
                        if (account == null)
                        {
                            //they are associated with a single account
                            if (user.AccountMemberships != null && user.AccountMemberships.Count == 1)
                            {
                                account = Things.GetOrNull<AccountThing>(user.AccountMemberships.First().Value.AccountId);
                            }
                        }
                        if (account != null)
                        {
                            logo = account.Logo;
                        }
                    }
                }
                else
                {
                    logo = fromContext as IImage;//an intentional default has String.Empty in httpcontext.  so we use as operator to check for that.
                }

                if (logo != null)
                {
                    viewData[AppLogoHelper.AppLogoKey] = new AppLogo
                    {
                        Height = logo.Height,
                        Width = logo.Width,
                        Url = logo.GetUrlHttps(),
                    };
                }

            }

            return viewData;
        }
    }



}
