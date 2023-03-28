using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Web;
using System.Web.Mvc;

using Mediascend.Web;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.ViewModels;

namespace Takeoff.Controllers
{
    public class CommentsController : BasicController
    {

        [HttpPost]
        [AuthorizeProductionAccess(DescendantIdParam = "VideoId", ParamComesFromModel = true)]
        public ActionResult Create(Comments_Create model)
        {
            if (!ModelState.IsValid)
            {
                return this.Invalid();
            }
            var video = Things.Get<VideoThing>(model.VideoId);
            var production = video.FindAncestor<ProjectThing>();
            var commentThing = Productions.AddVideoComment(video, this.UserId(), model.Body, model.StartTime);

            ISemiAnonymousUser semiAnonymousUser = null;
            this.Identity().IfType<SemiAnonymousUserIdentity>(identity =>
                                                                  {
                                                                      semiAnonymousUser = identity.User;
                                                                  });
            if (semiAnonymousUser == null)
            {
                //ensure the user has permission to comment 
                if (!video.CanCreate(typeof(VideoCommentThing), this.UserThing()))
                {
                    return this.Forbidden();
                };               
            }
            else
            {
                if (semiAnonymousUser.TargetId == production.Id && !production.GuestsCanComment) //can't check using VerifyCreate, so do it this way
                {
                    return this.Forbidden();
                }
                if ( semiAnonymousUser.TargetId == video.Id && !video.GuestsCanComment)
                {
                    return this.Forbidden();
                }
                if (model.UserName.HasChars() && !semiAnonymousUser.UserName.HasChars())//only let people set it one time
                {
                    semiAnonymousUser.UserName = model.UserName;
                    Repos.SemiAnonymousUsers.Update(semiAnonymousUser);
                }
                //AddVideoComment didn't set these so I just did it here
                commentThing.CreatedBySemiAnonymousUserName = semiAnonymousUser.UserName;
                commentThing.CreatedBySemiAnonymousUserId = semiAnonymousUser.Id;
            }

            commentThing.Insert();
            var activityContents = commentThing.GetActivityPanelContents(new Models.Data.ActionSource { Action = "Add" }, true, this.Identity());//that's the only property it needs
            var commentData = commentThing.CreateViewData(this.Identity());
            this.DeferRequest<EmailController>(c => c.NotifyForNewVideoComment(commentData.Id));
            return Json(new
            {
                Data = commentData,
                ActivityPanelItem = activityContents
            });
        }


        [HttpPost]
        [AuthorizeProductionAccess(DescendantIdParam = "id")]
        public ActionResult Edit(int id, string body)
        {
            if (!body.HasChars(CharsThatMatter.NonWhitespace))
            {
                ModelState.AddModelError("body","Body requires non-whitespace characters.");
            }
            if (!ModelState.IsValid)
            {
                return this.Invalid();
            }

            var comment = (CommentThing)Things.GetOrNull<CommentThing>(id);
            if ( comment == null)
                return this.DataNotFound();
            var user = this.UserThing();
            if (user == null)
            {
                if (!comment.CreatedBySemiAnonymousUserId.HasValue || this.Identity().CastTo<SemiAnonymousUserIdentity>().User.Id != comment.CreatedBySemiAnonymousUserId.Value)
                    return this.Forbidden();
            }
            else
            {
                if (!comment.HasPermission(Permissions.Edit, user))
                    return this.Forbidden();
            }
            comment.TrackChanges();
            comment.Body = body;
            comment.Update();

            return this.StatusCode(HttpStatusCode.OK);
        }


        [AuthorizeProductionAccess(DescendantIdParam = "id")]
        public ActionResult Delete(int id)
        {
            var comment = Things.GetOrNull<CommentThing>(id);
            if (comment == null)
                return this.DataNotFound();

            var user = this.UserThing();
            if (user == null)
            {
                if (!comment.CreatedBySemiAnonymousUserId.HasValue || this.Identity().CastTo<SemiAnonymousUserIdentity>().User.Id != comment.CreatedBySemiAnonymousUserId.Value)
                    return this.Forbidden();
            }
            else
            {
                if (!comment.HasPermission(Permissions.Delete, user))
                    return this.Forbidden();
            }
            comment.Delete();

            return this.StatusCode(HttpStatusCode.OK);
        }

    }
}
