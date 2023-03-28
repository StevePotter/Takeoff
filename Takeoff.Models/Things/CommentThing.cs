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
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace Takeoff.Models
{

    [ThingType("Comment")]
    [Serializable]
    public class CommentThing : ThingBase, IComment
    {
                #region Constructors

        public CommentThing()
        {
        }

        protected CommentThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {  
        }

        #endregion
        
        public string Body
        {
            get
            {
                return BodyProperty.GetValue(this);
            }
            set
            {
                BodyProperty.SetValue(this, value);
            }
        }
        private string _Body;
        private static readonly ThingDataProperty<string, CommentThing, Data.Comment> BodyProperty = new ThingDataProperty<string, CommentThing, Data.Comment>()
        {
            Name = "Body",
            SetField = (o, v) => o._Body = v,
            GetField = o => o._Body,
            GetData = o => o.Body,
            SetData = (o, v) => o.Body = v,
        }.Register();


        /// <summary>
        /// If the user was logged in via a production password (no account), this is the name supplied by that person.
        /// </summary>
        public string CreatedBySemiAnonymousUserName
        {
            get
            {
                return CreatedBySemiAnonymousUserNameProperty.GetValue(this);
            }
            set
            {
                CreatedBySemiAnonymousUserNameProperty.SetValue(this, value);
            }
        }
        private string _CreatedBySemiAnonymousUserName;
        private static readonly ThingDataProperty<string, CommentThing, Data.Comment> CreatedBySemiAnonymousUserNameProperty = new ThingDataProperty<string, CommentThing, Data.Comment>()
        {
            Name = "CreatedBySemiAnonymousUserName",
            SetField = (o, v) => o._CreatedBySemiAnonymousUserName = v,
            GetField = o => o._CreatedBySemiAnonymousUserName,
            ShouldGetData = o => o.CreatedBySemiAnonymousUserName != null,
            GetData = o => o.CreatedBySemiAnonymousUserName,
            SetData = (o, v) => o.CreatedBySemiAnonymousUserName = v,
        }.Register();

        /// <summary>
        /// If the user was logged in via a production password (no account), this is the Id supplied by that person.
        /// </summary>
        public Guid? CreatedBySemiAnonymousUserId
        {
            get
            {
                return CreatedBySemiAnonymousUserIdProperty.GetValue(this);
            }
            set
            {
                CreatedBySemiAnonymousUserIdProperty.SetValue(this, value);
            }
        }
        private Guid? _CreatedBySemiAnonymousUserId;
        private static readonly ThingDataProperty<Guid?, CommentThing, Data.Comment> CreatedBySemiAnonymousUserIdProperty = new ThingDataProperty<Guid?, CommentThing, Data.Comment>()
        {
            Name = "CreatedBySemiAnonymousUserId",
            SetField = (o, v) => o._CreatedBySemiAnonymousUserId = v,
            GetField = o => o._CreatedBySemiAnonymousUserId,
            ShouldGetData = o => o.CreatedBySemiAnonymousUserId.HasValue,
            GetData = o => o.CreatedBySemiAnonymousUserId,
            SetData = (o, v) => o.CreatedBySemiAnonymousUserId = v,
            FromJson = (o) =>
                           {
                               if (o is string)
                                   return new Guid?(new Guid(o.CastTo<string>()));
                               if (o is Guid)
                                   return new Guid?((Guid) o);
                               return new Guid?();
                           },
        }.Register();

        public override bool IsAuthor(Identity identity)
        {
            if (base.IsAuthor(identity))
                return true;
            if ( identity == null)
                return false;
            var asSemiAnonymous = identity as SemiAnonymousUserIdentity;
            if ( asSemiAnonymous != null && asSemiAnonymous.User != null && CreatedBySemiAnonymousUserId.HasValue)
            {
                return asSemiAnonymous.User.Id == CreatedBySemiAnonymousUserId.Value;
            }
            return false;
        }


        protected internal override IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            foreach (var d in base.FillAuxillaryData(db))
                yield return d;
            yield return this.CreateAuxDataFiller<Data.Comment>(db);
        }


        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(FillRecordWithProperties(new Data.Comment()), InsertIdParam);
        }


        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            FillRecordWithProperties((from d in db.Comments where d.ThingId == Id select d).Single());
        }

        protected override void QueueDeleteData(CommandBatcher batcher)
        {
            base.QueueDeleteData(batcher);
            batcher.QueueDeleteViaPrimaryKey(new Data.Comment
            {
                ThingId = Id
            });
        }

        protected override ThingModelView CreateViewInstance(string viewName)
        {
            return new CommentThingView();
        }

        protected override void FillView(string viewName, ThingModelView view, Identity identity)
        {
            base.FillView(viewName, view, identity);
            var typedView = (CommentThingView)view;
            typedView.ParentId = this.ParentId.GetValueOrDefault();

            typedView.Body = this.Body.CharsOrEmpty().Trim();//guarantees it is never null, which would break javascript
            typedView.Replies = this.Children.OfType<CommentThing>().OrderBy(c => c.CreatedOn).Select(c => (CommentThingView)c.CreateViewData(identity)).ToArray();
            if ( CreatedBySemiAnonymousUserName.HasChars() && typedView.Creator == null )
            {
                typedView.Creator = new UserView
                                        {
                                            Name = CreatedBySemiAnonymousUserName,
                                        };
            }
            if (CreatedBySemiAnonymousUserId.HasValue)
            {
                identity.IfType<SemiAnonymousUserIdentity>(i =>
                                                               {
                                                                   if (i.User != null && i.User.Id == CreatedBySemiAnonymousUserId.Value)
                                                                   {
                                                                       typedView.CanEdit = true;
                                                                       typedView.CanDelete = true;
                                                                   }
                                                               });
            }
        }


        //protected string GetAuthorName()
        //{
        //    if (this.CreatedByUserId > 0)
        //    {
        //        var author = Things.GetOrNull<UserThing>(this.CreatedByUserId);
        //        if (author != null && author.DisplayName.HasChars())
        //            return author.DisplayName;
        //    }
        //}


        public override ChangeDigestEmailItem GetActivityEmailContents(ActionSource change, IUser recipient)
        {
            //only show an added comment.  wil use the latest Body
            if ( change.Action == ThingChanges.Add && Body.HasChars() )
            {
                IUser author = change.UserId.IsPositive() ? Things.GetOrNull<UserThing>(change.UserId) : null;
                var item = new ChangeDigestEmailItem
                               {
                                   IsRecipientTheAuthor =
                                       recipient != null && author != null && recipient.Id == author.Id
                               };
                if (item.IsRecipientTheAuthor)
                {
                    item.Text = "You commented '" + this.Body.CharsOrEmpty().Truncate(40, StringTruncating.EllipsisCharacter) + "'";                    
                }
                else if ( author == null )
                {
                    item.Text = this.CreatedBySemiAnonymousUserName.CharsOr("Someone") + " commented '" + this.Body.CharsOrEmpty().Truncate(40, StringTruncating.EllipsisCharacter) + "'";
                }
                else
                {
                    item.Text = author.DisplayName + " commented '" + this.Body.CharsOrEmpty().Truncate(40, StringTruncating.EllipsisCharacter) + "'";                    
                }
                //change.Date was local time for some reason.  todo: figure this out
                item.Date = this.CreatedOn;
                item.Text = recipient.UtcToLocal(this.CreatedOn).ToString(DateTimeFormat.ShortDateTime) + ": " + item.Text;//prepend the date/time
                item.Html = HttpUtility.HtmlEncode(item.Text);

                return item;
            }
            return null;
        }

        public override ProductionActivityItem GetActivityPanelContents(ActionSource action, bool withinProject, Identity identity)
        {
            if (action.Action == ThingChanges.Add)
            {
                var video = Parent as VideoThing;
                if (video == null)
                    return null;

                var project = video.Parent as ProjectThing;
                if (project == null)
                    return null;

                var html = new StringBuilder();
                if ( this.IsAuthor(identity))
                {
                    html.Append("You");                    
                }
                else
                {
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
                        html.Append(this.CreatedBySemiAnonymousUserName.CharsOr("Someone"));
                    }                    
                }

                var urlPrefix = withinProject ? "#" : project.Url() + "?focus=";
                html.Append(" commented &quot;<a href='" + urlPrefix + "comment_" + this.Id.ToInvariant() + "_video_" + video.Id.ToInvariant() + "'>");
                html.Append(Body.CharsOrEmpty().Truncate(20, StringTruncating.EllipsisCharacter).HtmlEncode());
                html.Append("</a>&quot;");
                html.Append(" for ");
                if (withinProject)
                {
                    html.Append("<a href='#video_" + video.Id.ToInvariant() + "'>" + video.Title.HtmlEncode() + "</a>");
                }
                else
                {
                    html.Append(project.HtmlLink() + " : <a href='" + urlPrefix + "video_" + video.Id.ToInvariant() + "'>" + video.Title.HtmlEncode() + "</a>");
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


    public class CommentThingView : ThingBasicModelView
    {
        public int ParentId { get; set; }
        public string Body { get; set; }
        public CommentThingView[] Replies { get; set; }
    }

}