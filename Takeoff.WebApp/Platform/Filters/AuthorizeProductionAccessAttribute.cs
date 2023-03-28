using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Security;
using Takeoff.Controllers;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.ViewModels;
using MvcContrib;
using Takeoff.Resources;
using System.ComponentModel;

namespace Takeoff
{

    
    /// <summary>
    /// This performs necessary authorization checks around a given production and handles logic like membership requests, anonymous access, missing production, etc.  
    /// Only if the production exists and user has read access to it will the action run.
    /// This currently also handles the work involved with semi-anonymous access to a video.
    /// </summary>
    /// <remarks>
    /// todo: if a user is logged in and get a url to a pw-protected video or production they aren't a member of, right now they have to request membership.  this is wrong.  they should be able to enter a pw and have access.  furthermore, it should be in their dashboard as long as the pw doesn't change or the owner turns off guest access
    /// </remarks>
    public class AuthorizeProductionAccessAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// By default, the "id" actionparameter is used to get the production ID.  This is a custom parameter name if you need it.
        /// </summary>
        public string ProductionIdParam { get; set; }

        /// <summary>
        /// When specified, this indicates a production descendant whose id is used to find the production.  If this is specified, ProductionIdParam will be ignored.
        /// </summary>
        public string DescendantIdParam { get; set; }

        /// <summary>
        /// When true, the ProductionIdParam or DescendantIdParam comes from a single model action parameter.
        /// </summary>
        public bool ParamComesFromModel { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var result = Authorize(filterContext);
            if ( result != null)
            {
                filterContext.Result = result;
            }
        }
 
        /// <summary>
        /// Checks whether the current user has access to the production, returning a view result if they don't.  Null indicates all is well.
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        private ActionResult Authorize(ActionExecutingContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            ProjectThing production = null;
            VideoThing videoAccessedViaSemiAnonymous = null;//when set, this is the video that is being accessed.  This adds the condition where someone has semi anonymous access to the video.
            if (DescendantIdParam.HasChars())
            {
                int thingId;
                if (ParamComesFromModel)
                {
                    var model = filterContext.ActionParameters.First().Value;
                    thingId = Convert.ToInt32(TypeDescriptor.GetProperties(model)[DescendantIdParam].GetValue(model));
                }
                else
                {
                    thingId = Convert.ToInt32(filterContext.ActionParameters[DescendantIdParam]);                    
                }
                var thing = Things.GetOrNull(thingId); //could be a production, comment, file, video, whatever
                if (thing == null)//for a video, this will say the production wasn't found.  hope that's okay!
                {
                    return CreateErrorResult(filterContext, "Productions-NotFound", null, ErrorCodes.NotFound, ErrorCodes.NotFoundDescription, HttpStatusCode.NotFound);
                }
                production = thing as ProjectThing ?? thing.FindAncestor<ProjectThing>();
                videoAccessedViaSemiAnonymous = thing as VideoThing ?? thing.FindAncestor<VideoThing>();

                if (videoAccessedViaSemiAnonymous != null && !videoAccessedViaSemiAnonymous.IsGuestAccessEnabled())//we only care about the video if it has semi-anonymous access.  so if it doesn't, just clear it out and only worry about the production access
                    videoAccessedViaSemiAnonymous = null;
            }
            else
            {
                var id = Convert.ToInt32(filterContext.ActionParameters[ProductionIdParam.CharsOr("id")]);
                production = Things.GetOrNull<ProjectThing>(id);
            }
            
            //missing production
            if (production == null)
            {
                return CreateErrorResult(filterContext, "Productions-NotFound", null, ErrorCodes.NotFound, ErrorCodes.NotFoundDescription, HttpStatusCode.NotFound);
            }

            //user is accessing a production whose account was suspended.
            var productionAccount = Things.GetOrNull<AccountThing>(production.AccountId);
            if (productionAccount != null)//this fit nicely into the app, although i'm not sure if it'll work out forever.  anyway, this sets the app logo to that of the production's account
            {
                filterContext.HttpContext.SetAppLogo(productionAccount);

                if (productionAccount.IsSuspended())
                {
                    return CreateForbiddenResult(filterContext, "Account-Suspended-NotOwner", ErrorCodes.AccountProblem, ErrorCodes.AccountProblemDescription);
                }
            }

            var identity = httpContext.Identity();
            //not logged in at all.  in this case, user should log in or enter production/video password
            if (identity == null)
            {
                //if someone was in a trial or demo and their session expired, like putting computer to sleep,
                //there is no way to know they are the owner since it was anonymous, so give them a unique error message
                if (productionAccount != null && productionAccount.Status == AccountStatus.TrialAnonymous)                    
                {
                    return CreateForbiddenResult(filterContext, "Account-AnonymousTrial-ProductionAccess-NotLoggedIn", ErrorCodes.AccountProblem, ErrorCodes.AccountProblemDescription);
                }

                if (videoAccessedViaSemiAnonymous != null)
                    return Login(videoAccessedViaSemiAnonymous, filterContext);
                return Login(production, filterContext);
            }

            var user = httpContext.UserThing();
            //user is a team member
            if ( user != null && production.HasPermission(Permissions.Details, user) )
            {
                return null;//action will execute.  awww yeah!
            }


            //user entered password for production or video
            if ( identity is SemiAnonymousUserIdentity)
            {
                //they entered a pw for a video
                if (videoAccessedViaSemiAnonymous != null && identity.CastTo<SemiAnonymousUserIdentity>().TargetId == videoAccessedViaSemiAnonymous.Id)//they have access to the video specifically
                {              
                    return null;
                }
                //entered a password for a production
                if (identity.CastTo<SemiAnonymousUserIdentity>().TargetId == production.Id)
                {                    
                    if (production.IsSemiAnonymousAccessEnabled)//they have access so return null
                    {
                        return null;
                    }
                    //this would only realistically happen if someone disabled anonymous access for the production while this person was viewing it
                    return Login(production, filterContext);
                }
                //they were logged into another production anonymously and now they've gone to another.  you can't be 
                //logged into two productions anonymously at once, so clear identity and treat them as a completely anonymous user
                IoC.Get<IIdentityService>().ClearIdentity(httpContext);
                return Login(production, filterContext);

            }

            //***** deal with a user without access

            //demo account holders cannot request membership.  neither can nonverified.  but demo account holders don't have an email address in the system so we need them to enter their email address first
            var usersAccout = user.Account;
            if (usersAccout != null && usersAccout.Status == AccountStatus.Demo)
            {
                return RedirectToAction<SignupController>(c => c.Index(null,httpContext.Request.Url.OriginalString, null), filterContext);
            }
            if (!user.IsVerified)
            {
                var url = new UrlHelper(filterContext.RequestContext).Action<AccountController>(c => c.Verify(null), UrlType.AbsoluteHttps);
                return CreateForbiddenResult(filterContext, "RequiresVerification", ErrorCodes.UnverifiedEmail, ErrorCodes.UnverifiedEmailDescription, url);
            }

            //to determine whether access requests are allowed, we look first at who created the production.  normally this is the account holder but in extreme cases this person could have been removed from teh system or from the account, in which case we revert to the current account holder
            var productionCreator = production.Owner;

            //first check to see whether they were invited
            var membershipRequest = production.FindChild<MembershipRequestThing>(r => r.InviteeId == user.Id);
            if (membershipRequest != null && membershipRequest.IsInvitation)
            {
                var videoWithThumbs = videoAccessedViaSemiAnonymous ?? production.GetLatestVideo();
                var invitedBy = Things.GetOrNull<UserThing>(membershipRequest.CreatedByUserId) ?? productionCreator;
                var model = new Production_Details_Invitation
                                {
                                    InvitedByEmail = invitedBy.Email,
                                    InvitedByName = invitedBy.DisplayName,
                                    MembershipRequestId = membershipRequest.Id,
                                    Note = membershipRequest.Note,
                                    RequestCreatedOn = user.UtcToLocal(membershipRequest.CreatedOn),
                                    ProductionTitle = production.Title,
                                };
                if (videoWithThumbs != null)
                {
                    model.Thumbnails = videoWithThumbs.ChildrenOfType<VideoThumbnailThing>().Select(t => new VideoThumbnail
                                                                                                   {
                                                                                                       Url = t.GetUrl(),
                                                                                                       Height = t.Height,
                                                                                                       Width = t.Width,
                                                                                                       Time = t.Time,
                                                                                                   }).ToArray();
                }

                return CreateForbiddenResult(filterContext, "Video-Invitation", model, ErrorCodes.NotMemberMustAcceptInvitation, ErrorCodes.NotMemberMustAcceptInvitationDescription);
            }

            bool allowsRequests = (bool) productionCreator.GetSettingValue(UserSettings.EnableMembershipRequests);
            if (MembershipRequestsController.IsUserBanned(user, production))
            {
                allowsRequests = false; //make it seem like they weren't explicitly banned so their feelings don't get hurt
            }

            if (allowsRequests)
            {
                //let them know their request has been sent
                if (membershipRequest != null)
                {
                    return CreateForbiddenResult(filterContext, "Details-MembershipRequestSent", ErrorCodes.MembershipRequestSent, ErrorCodes.MembershipRequestSentDescription);
                }

                return CreateForbiddenResult(filterContext, "Production-RequestAccess", new Productions_Details_RequestAccess { ProductionId = production.Id }, ErrorCodes.NotMemberCanRequest, ErrorCodes.NotMemberCanRequestDescription);
            }

            return CreateForbiddenResult(filterContext, "Production-MembershipRequests-Disabled", ErrorCodes.MembershipRequestsDisabled, ErrorCodes.MembershipRequestsDisabledDescription);
        }

        private static RedirectResult RedirectToAction<T>(Expression<Action<T>> action, ActionExecutingContext filterContext) where T : Controller
        {
            var url = new UrlHelper(filterContext.RequestContext).Action(action);
            return new RedirectResult(url);
        }

        private static ActionResult View(string viewName, object model, ActionExecutingContext filterContext)
        {
            return new ViewResult
            {
                ViewData = SetViewDataAttribute.FillViewData(new ViewDataDictionary(model), filterContext.HttpContext, filterContext.ActionDescriptor),
                ViewName = viewName,
            };
        }

        protected ActionResult CreateForbiddenResult(ActionExecutingContext filterContext, string htmlViewName, string errorCode, string errorDescription = null, string resolveUrl = null)
        {
            return CreateForbiddenResult(filterContext, htmlViewName, null, errorCode, errorDescription, resolveUrl);
        }

        protected ActionResult CreateForbiddenResult(ActionExecutingContext filterContext, string htmlViewName, object viewModel, string errorCode, string errorDescription = null, string resolveUrl = null)
        {
            return CreateErrorResult(filterContext, htmlViewName, viewModel, errorCode, errorDescription, HttpStatusCode.Forbidden, resolveUrl);
        }

        protected ActionResult CreateErrorResult(ActionExecutingContext filterContext, string htmlViewName, object viewModel, string errorCode, string errorDescription, HttpStatusCode? statusCode, string resolveUrl = null)
        {
            var request = filterContext.HttpContext.Request;
            if (request.IsWebPageRequestOrNonAjaxFormPost())//contenttype checks ensure xml, json, etc are excluded
            {
                return viewModel == null
                           ? new ViewResult { ViewName = htmlViewName }
                           : new ViewResult { ViewName = htmlViewName, ViewData = new ViewDataDictionary(viewModel) };
            }

            return new NonHtmlErrorResponse
            {
                ErrorCode = errorCode,
                ErrorDescription = errorDescription,
                StatusCode = statusCode.GetValueOrDefault(HttpStatusCode.OK),
            };
        }



        private static ActionResult Login(ProjectThing production, ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            if (!request.IsWebPageRequestOrNonAjaxFormPost())
                return new Http401NoLoginPageResult();

            var url = request.Url.MapIfNotNull(u => u.OriginalString);
            return View(production.IsSemiAnonymousAccessEnabled ? "Production-Login-SemiAnonymousAccess" : "Production-Login", new Productions_Login
            {
                ProductionId = production.Id,
                ReturnUrl = url,
            }, filterContext);
        }

        private static ActionResult Login(VideoThing video, ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            if (!request.IsWebPageRequestOrNonAjaxFormPost())
                return new Http401NoLoginPageResult();

            var url = request.Url.MapIfNotNull(u => u.OriginalString);
            return View("Video-Login-SemiAnonymousAccess", new Video_Login
            {
                VideoId = video.Id,
                ReturnUrl = url,
            }, filterContext);
        }




    }



}
