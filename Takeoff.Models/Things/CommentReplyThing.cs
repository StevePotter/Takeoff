using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Takeoff.Data;
using Takeoff.Models;



using Takeoff.Models.Data;
using System.Text;
using System.Dynamic;
using System.Globalization;
using System.Runtime.Serialization;

namespace Takeoff.Models
{
    [ThingType("CommentReply")]
    [Serializable]
    public class CommentReplyThing : CommentThing
    {

                #region Constructors

        public CommentReplyThing()
        {
        }

        protected CommentReplyThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {  
        }

        #endregion
        

        public override ChangeDigestEmailItem GetActivityEmailContents(ActionSource change, IUser recipient)
        {
            //only show an added comment.  wil use the latest Body
            if (change.Action == ThingChanges.Add && Body.HasChars())
            {
                IUser author = change.UserId.IsPositive() ? Things.GetOrNull<UserThing>(change.UserId) : null;
                var item = new ChangeDigestEmailItem
                {
                    IsRecipientTheAuthor =
                        recipient != null && author != null && recipient.Id == author.Id
                };
                if (item.IsRecipientTheAuthor)
                {
                    item.Text = "You replied to a comment with '" + this.Body.CharsOrEmpty().Truncate(40, StringTruncating.EllipsisCharacter) + "'";
                }
                else if (author == null)
                {
                    item.Text = this.CreatedBySemiAnonymousUserName.CharsOr("Someone") + " replied to a comment with '" + this.Body.CharsOrEmpty().Truncate(40, StringTruncating.EllipsisCharacter) + "'";
                }
                else
                {
                    item.Text = author.DisplayName + " replied to a comment with '" + this.Body.CharsOrEmpty().Truncate(40, StringTruncating.EllipsisCharacter) + "'";
                }
                //change.Date was local time for some reason.  todo: figure this out
                item.Date = this.CreatedOn;
                item.Text = recipient.UtcToLocal(item.Date).ToString(DateTimeFormat.ShortDateTime) + ": " + item.Text;//prepend the date/time
                item.Html = HttpUtility.HtmlEncode(item.Text);
                return item;
            }
            return null;
        }

        public override ProductionActivityItem GetActivityPanelContents(ActionSource action, bool withinProject, Identity identity)
        {
            if (action.Action == ThingChanges.Add)
            {
                var comment = Parent as CommentThing;
                if (comment == null)
                    return null;
                var video = comment.Parent as VideoThing;
                if (video == null)
                    return null;
                var project = video.Parent as ProjectThing;
                if (project == null)
                    return null;

                var html = new StringBuilder();
                if (this.IsAuthor(identity))
                {
                    html.Append("You");
                }
                else
                {
                    var author = this.GetAuthor();
                    if (author != null)
                    {
                        if (author.Email.HasChars())
                            html.Append("<a href='mailto:" + author.Email + "'>" + author.DisplayName.HtmlEncode() +
                                        "</a>");
                        else
                            html.Append(author.DisplayName.HtmlEncode());
                    }
                    else
                    {
                        html.Append(this.CreatedBySemiAnonymousUserName.CharsOr("Someone"));
                    }
                }

                var urlPrefix = withinProject ? "#" : project.Url() + "?focus=";
                html.Append(" replied &quot;<a href='" + urlPrefix + "reply_" + this.Id.ToInvariant() + "_comment_" + comment.Id.ToInvariant() + "_video_" + video.Id.ToInvariant() + "'>");
                html.Append(Body.CharsOrEmpty().Truncate(20, StringTruncating.EllipsisCharacter).HtmlEncode());
                html.Append("</a>&quot; to a comment ");

                if (!withinProject)
                {
                    html.Append(" in " + project.HtmlLink());
                }

                html.Append(" <em class='relativeDate'></em>");
                return new ProductionActivityItem
                {
                    Html = html.ToString(),
                    CssClass = "commented",
                    Date = this.CreatedOn.ForJavascript(),
                    ThingId = this.Id
                };
            }
            return null;
        }


    }

}