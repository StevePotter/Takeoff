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
    /// This checks for any pending prompts for the current user and either redirects them to the given page or shows them as banners on the current page.
    /// </summary>
    public class CheckPromptAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.RequestContext == null || filterContext.RequestContext.HttpContext == null)
                return;
            if (!filterContext.RequestContext.HttpContext.Request.IsWebPageRequest())
                return;
            
            var user = filterContext.HttpContext.UserThing();
            if (user == null || !user.HasPendingPrompts)
                return;

            var now = filterContext.HttpContext.RequestDate();
            foreach( var prompt in user.PendingPrompts.ToArray() )//toarray prevents modified enumerable exception when a prompt is deleted
            {
                if (prompt.StartsOn.HasValue && now < prompt.StartsOn.Value)
                {
                    continue;//prompt hasn't taken effect yet
                }
                //prompt is dealt with so delete it
                Repos.Prompts.Delete(prompt);

                //prompt is good so return the view
                if (prompt.View.HasChars() && (!prompt.ExpiresOn.HasValue || now < prompt.ExpiresOn.Value))
                {
                    var requestUrl = filterContext.HttpContext.Request.Url.MapIfNotNull(u => u.OriginalString);//used by view to send the person back to the originally requested page
                    var viewData = new ViewDataDictionary();
                    viewData["ControllerName"] = "Prompts";
                    viewData.Model = new ViewPromptViewModel
                                         {
                                             OriginalUrl = requestUrl,
                                         };
                    filterContext.RouteData.Values["controller"] = "Prompts";//this is so the view engine will use the Prompts folder.
                    filterContext.Result = new ViewResult
                                               {
                                                   ViewName = prompt.View,
                                                   ViewData = viewData,
                                               };
                    break;//don't process any other prompts
                }
            }

            ////a special parameter gets appended each time a prompt gets redirected to.  this way we delete the prompt and avoid an infinite loop
            //const string PromptParam = "__promptId";
            //var promptId = filterContext.HttpContext.Request[PromptParam].ToIntTry();
            //if (promptId.HasValue)
            //{//this will only happen when there were multiple prompts and we are showing a redirect. that's because the prompt was already deleted
            //    return;
            //}

            ////there are two types of prompts.  the first redirects to a url and the second is banner stuff.  
            //var redirect = user.PendingPrompts.FirstOrDefault(p => p.RedirectUrl.HasChars());
            //if (redirect != null)
            //{
            //    var url = redirect.RedirectUrl;
            //    var urlWithParam = url.Contains('?') ? (url + "&" + PromptParam + "=" + redirect.Id.ToInvariant()) : (url + "?" + PromptParam + "=" + redirect.Id.ToInvariant());
            //    filterContext.Result = new RedirectResult(urlWithParam);
            //    Repos.Prompts.Delete(redirect);
            //    return;
            //}

            ////show the banners once and delete em!
            //var banners = user.PendingPrompts.Where(p => !p.RedirectUrl.HasChars() && p.BannerHtml.HasChars());
            //if (banners.HasItems())
            //{
            //    var controller = filterContext.Controller as Controller;
            //    if (controller != null)
            //    {
            //        foreach( var banner in banners.ToArray() )
            //        {
            //            controller.AddBanner(banner.BannerHtml, banner.HtmlEncodeBanner);
            //            Repos.Prompts.Delete(banner);
            //        }
            //    }
            //}

        }
    }



}
