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

namespace Takeoff.Controllers
{
    [SubController("/comments/{commentId}/replies", true)]
    public class RepliesController : BasicController
    {
        [HttpPost]
        [AuthorizeProductionAccess(DescendantIdParam = "commentId")]
        public ActionResult Create(int commentId, string body, string name)
        {
            var comment = Things.Get<CommentThing>(commentId);
            var video = comment.FindAncestor<VideoThing>();
            var production = video.FindAncestor<ProjectThing>();

            var replyThing = Productions.AddCommentReply(comment, this.UserId(), body);

            ISemiAnonymousUser semiAnonymousUser = null;
            this.Identity().IfType<SemiAnonymousUserIdentity>(identity =>
            {
                semiAnonymousUser = identity.User;
            });
            if (semiAnonymousUser == null)
            {
                comment.VerifyCreate(typeof(CommentReplyThing), this.UserThing());
            }
            else
            {
                if (semiAnonymousUser.TargetId == production.Id && !production.GuestsCanComment) //can't check using VerifyCreate, so do it this way
                {
                    return this.Forbidden();
                }
                if (semiAnonymousUser.TargetId == video.Id && !video.GuestsCanComment)
                {
                    return this.Forbidden();
                }
                if (name.HasChars() && !semiAnonymousUser.UserName.HasChars())//only let people set it one time
                {
                    semiAnonymousUser.UserName = name;
                    Repos.SemiAnonymousUsers.Update(semiAnonymousUser);
                }
                //AddCommentReply didn't set these so I just did it here
                replyThing.CreatedBySemiAnonymousUserName = semiAnonymousUser.UserName;
                replyThing.CreatedBySemiAnonymousUserId = semiAnonymousUser.Id;
            }
            replyThing.Insert();
            var replyData = replyThing.CreateViewData(this.Identity());
            var activityContents = replyThing.GetActivityPanelContents(new Models.Data.ActionSource { Action = "Add" }, true, this.Identity());//that's the only property it needs
            this.DeferRequest<EmailController>(c => c.NotifyForNewVideoCommentReply(replyData.Id));

            return Json(new
            {
                Data = replyData,
                ActivityPanelItem = activityContents
            });            
        }


        [HttpPost]
        [AuthorizeProductionAccess(DescendantIdParam = "id")]
        public ActionResult Edit(int id, string body)
        {
            Args.NotNull(body, "body");
            body = body.Trim();
            Args.HasChars(body, "body");
            Args.GreaterThanZero(id, "id");

            var reply = Things.GetOrNull<CommentReplyThing>(id);
            if (reply == null)
                return this.DataNotFound();
            var user = this.UserThing();
            if (user == null)
            {
                if (!reply.CreatedBySemiAnonymousUserId.HasValue || this.Identity().CastTo<SemiAnonymousUserIdentity>().User.Id != reply.CreatedBySemiAnonymousUserId.Value)
                    return this.Forbidden();
            }
            else
            {
                if (!reply.HasPermission(Permissions.Edit, user))
                    return this.Forbidden();
            }
            reply.TrackChanges();
            reply.Body = body;
            reply.Update();

            return this.StatusCode(HttpStatusCode.OK);
        }


        [AuthorizeProductionAccess(DescendantIdParam = "id")]
        public ActionResult Delete(int id)
        {
            var reply = Things.GetOrNull<CommentReplyThing>(id);
            if (reply == null)
                return this.DataNotFound();

            var user = this.UserThing();
            if (user == null)
            {
                if (!reply.CreatedBySemiAnonymousUserId.HasValue || this.Identity().CastTo<SemiAnonymousUserIdentity>().User.Id != reply.CreatedBySemiAnonymousUserId.Value)
                    return this.Forbidden();
            }
            else
            {
                if (!reply.HasPermission(Permissions.Delete, user))
                    return this.Forbidden();
            }
            reply.Delete();

            return this.StatusCode(HttpStatusCode.OK);
        }
    }
}
