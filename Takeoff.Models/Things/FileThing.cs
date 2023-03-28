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

    [ThingType("File")]
    [Serializable]
    public class FileThing : FileBaseThing, IFile
    {
                #region Constructors

        public FileThing()
        {
        }

        protected FileThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {  
        }

        #endregion
        
        public override ChangeDigestEmailItem GetActivityEmailContents(ActionSource change, IUser recipient)
        {
            //only show an added comment.  wil use the latest Body
            if (change.Action == ThingChanges.Add && OriginalFileName.HasChars())
            {
                //only add asset files, which fall right under a ProjectThing
                var project = Parent as ProjectThing;
                if (project == null)
                    return null;

                IUser author = change.UserId.IsPositive() ? Things.GetOrNull<UserThing>(change.UserId) : null;
                var item = new ChangeDigestEmailItem
                {
                    IsRecipientTheAuthor =
                        recipient != null && author != null && recipient.Id == author.Id
                };
                if (item.IsRecipientTheAuthor)
                {
                    item.Text = "You uploaded a file called '" + this.OriginalFileName.CharsOrEmpty().Truncate(40, StringTruncating.EllipsisCharacter) + "'";
                }
                else if (author == null)
                {
                    item.Text = "Somebody uploaded a file called '" + this.OriginalFileName.CharsOrEmpty().Truncate(40, StringTruncating.EllipsisCharacter) + "'";
                }
                else
                {
                    item.Text = author.DisplayName + " uploaded a file called '" + this.OriginalFileName.CharsOrEmpty().Truncate(40, StringTruncating.EllipsisCharacter) + "'";
                }
                //change.Date was local time for some reason.  todo: figure this out
                item.Date = this.CreatedOn;
                item.Text = recipient.UtcToLocal(this.CreatedOn).ToString(DateTimeFormat.ShortDateTime) + ": " + item.Text;//prepend the date/time
                item.Html = HttpUtility.HtmlEncode(item.Text);

                return item;
            }
            return null;
        }


        protected override void OnDeleted()
        {
            base.OnDeleted();
            //because AccountThing stores video stats in its cache (for performance since it's hit a lot), clear the cache for it

            Things.GetOrNull<AccountThing>(AccountId).IfNotNull(a => a.RemoveFromCache());
        }

        protected override void OnInserted()
        {
            base.OnInserted();
            //because AccountThing stores video stats in its cache (for performance since it's hit a lot), clear the cache for it
            Things.GetOrNull<AccountThing>(AccountId).IfNotNull(a => a.RemoveFromCache());
        }


        public override ProductionActivityItem GetActivityPanelContents(ActionSource action, bool withinProject, Identity identity)
        {
            if (action.Action == ThingChanges.Add)
            {
                //only add asset files, which fall right under a ProjectThing
                var project = Parent as ProjectThing;
                if (project == null)
                    return null;
                var html = new StringBuilder();
                var author = this.GetAuthor();
                if (author != null)
                {
                    if (author.Email.HasChars())
                        html.Append("<a href='mailto:" + author.Email + "'>" + author.DisplayName.HtmlEncode() + "</a>");
                    else
                        html.Append(author.DisplayName.HtmlEncode());
                }
                else
                {
                    html.Append("Someone");
                }

                html.Append(" added a file <a href='/ProductionFiles/Download/" + this.Id.ToInvariant() + "'>");
                html.Append(this.OriginalFileName.CharsOrEmpty().HtmlEncode());
                html.Append("</a>");

                if (!withinProject)
                {
                    html.Append(" in " + project.HtmlLink());
                }

                html.Append(" <em class='relativeDate'></em>");
                return new ProductionActivityItem
                {
                    Html = html.ToString(),
                    CssClass = "added",
                    Date = this.CreatedOn.ForJavascript(),
                    ThingId = this.Id
                };
            }
            return null;
        }


    }


    public class FileThingView : ThingBasicModelView
    {
        public string FileName { get; set; }

        public int SizeBytes { get; set; }

        public string SizeFormatted { get; set; }
    }

}