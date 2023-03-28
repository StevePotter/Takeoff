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
using System.Configuration;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace Takeoff.Models
{

    [ThingType("Video")]
    [Serializable]
    public class VideoThing : ThingBase, IVideo
    {

        #region Constructors

        public VideoThing()
        {
        }

        protected VideoThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {  
        }

        #endregion

        public static string GuestAccessPasswordIV = ConfigurationManager.AppSettings["ProductionQuickAccessPasswordIV"];
        public static string GuestAccessPasswordEncryptionKey = ConfigurationManager.AppSettings["ProductionQuickAccessPasswordEncryptionKey"];

        public static string EncryptGuestPassword(string password)
        {
            return password.EncryptTwoWay(GuestAccessPasswordIV, GuestAccessPasswordEncryptionKey);
        }
        public static string DescryptGuestPassword(string password)
        {
            return password.Decrypt(GuestAccessPasswordIV, GuestAccessPasswordEncryptionKey);
        }

        public string Title
        {
            get
            {
                return TitleProperty.GetValue(this);
            }
            set
            {
                TitleProperty.SetValue(this, value);
            }
        }
        private string _Title;
        private static readonly ThingDataProperty<string, VideoThing, Data.Video> TitleProperty = new ThingDataProperty<string, VideoThing, Data.Video>()
        {
            Name = "Title",
            SetField = (o, v) => o._Title = v,
            GetField = o => o._Title,
            GetData = o => o.Title,
            SetData = (o, v) => o.Title = v,
        }.Register();


        /// <summary>
        /// Indicates whether this is a sample file created when the user signed up or whatever.  When setting to true, you should also set IsComplimentary to true (it's not automatically done).
        /// </summary>
        public bool IsSample
        {
            get
            {
                return IsSampleProperty.GetValue(this);
            }
            set
            {
                IsSampleProperty.SetValue(this, value);

            }
        }
        private bool _IsSample;
        private static readonly ThingDataProperty<bool, VideoThing, Data.Video> IsSampleProperty = new ThingDataProperty<bool, VideoThing, Data.Video>()
        {
            Name = "IsSample",
            SetField = (o, v) => o._IsSample = v,
            GetField = o => o._IsSample,
            ShouldGetData = r => r.IsSample != null,
            GetData = o => (bool)o.IsSample,
            SetData = (o, v) => o.IsSample = v,
        }.Register();


        public bool CountsTowardLimit
        {
            get { return !IsComplimentary; }
            set { IsComplimentary = !value; }
        }

        /// <summary>
        /// Indicates whether this video doesn't count toward a plan's limit. 
        /// </summary>
        public bool IsComplimentary
        {
            get
            {
                return IsComplimentaryProperty.GetValue(this);
            }
            set
            {
                IsComplimentaryProperty.SetValue(this, value);
            }
        }
        private bool _IsComplimentary;
        private static readonly ThingDataProperty<bool, VideoThing, Data.Video> IsComplimentaryProperty = new ThingDataProperty<bool, VideoThing, Data.Video>()
        {
            Name = "IsComplimentary",
            SetField = (o, v) => o._IsComplimentary = v,
            GetField = o => o._IsComplimentary,
            ShouldGetData = r => r.IsComplimentary != null,
            GetData = o => (bool)o.IsComplimentary,
            SetData = (o, v) => o.IsComplimentary = v,
        }.Register();

       

        public bool IsSourceDownloadable
        {
            get
            {
                return IsSourceDownloadableProperty.GetValue(this);
            }
            set
            {
                IsSourceDownloadableProperty.SetValue(this, value);
            }
        }
        private bool _IsSourceDownloadable;
        private static readonly ThingDataProperty<bool, VideoThing, Data.Video> IsSourceDownloadableProperty = new ThingDataProperty<bool, VideoThing, Data.Video>()
        {
            Name = "IsSourceDownloadable",
            SetField = (o, v) => o._IsSourceDownloadable = v,
            GetField = o => o._IsSourceDownloadable,
            ShouldGetData = r => r.IsSourceDownloadable != null,
            GetData = o => (bool)o.IsSourceDownloadable,
            SetData = (o, v) => o.IsSourceDownloadable = v,
        }.Register();


        //public double SecondsBetweenKeyframes
        //{
        //    get
        //    {
        //        return SecondsBetweenKeyframesProperty.GetValue(this);
        //    }
        //    set
        //    {
        //        SecondsBetweenKeyframesProperty.SetValue(this, value);
        //    }
        //}
        //private double _SecondsBetweenKeyframes;
        //private static readonly ThingDataProperty<double, VideoThing, Data.Video> SecondsBetweenKeyframesProperty = new ThingDataProperty<double, VideoThing, Data.Video>()
        //{
        //    Name = "SecondsBetweenKeyframes",
        //    SetField = (o, v) => o._SecondsBetweenKeyframes = v,
        //    GetField = o => o._SecondsBetweenKeyframes,
        //    ShouldGetData = r => r.SecondsBetweenKeyframes != null,
        //    GetData = o => (double)o.SecondsBetweenKeyframes,
        //    SetData = (o, v) => o.SecondsBetweenKeyframes = v,
        //}.Register();
	


        /// <summary>
        /// The duration of the video.
        /// </summary>
        public TimeSpan? Duration
        {
            get
            {
                return DurationProperty.GetValue(this);
            }
            set
            {
                DurationProperty.SetValue(this, value);
            }
        }
        private TimeSpan? _Duration;
        private static readonly ThingDataProperty<TimeSpan?, VideoThing, Data.Video> DurationProperty = new ThingDataProperty<TimeSpan?, VideoThing, Data.Video>()
        {
            Name = "Duration",
            SetField = (o, v) => o._Duration = v,
            GetField = o => o._Duration,
            ShouldGetData = r => r.Duration != null,
            GetData = o => TimeSpan.FromSeconds((double)o.Duration),
            SetData = (o, v) => o.Duration = (v.HasValue ? new double?(v.Value.TotalSeconds) : null),
            ToJson = (v) => v.GetValueOrDefault().Ticks,
            ToJsonSimple2 = v => v.GetValueOrDefault().Ticks,
            FromJson = (v) => TimeSpan.FromTicks((long)v),
        }.Register();


        public string CustomUrl
        {
            get
            {
                return CustomUrlProperty.GetValue(this);
            }
            set
            {
                CustomUrlProperty.SetValue(this, value);
            }
        }
        private string _CustomUrl;
        private static readonly ThingDataProperty<string, VideoThing, Data.Video> CustomUrlProperty = new ThingDataProperty<string, VideoThing, Data.Video>()
        {
            Name = "CustomUrl",
            SetField = (o, v) => o._CustomUrl = v,
            GetField = o => o._CustomUrl,
            GetData = o => o.CustomUrl,
            SetData = (o, v) => o.CustomUrl = v,
        }.Once(prop =>
        {
            prop.BeforeSet += (e, args) =>
            {
                if (CustomUrlProperty.IsSet(args.Owner) && !args.Owner.CustomUrl.EqualsCaseSensitive(args.NewValue))
                {
                    if (args.Owner.CustomUrl.HasChars())
                    {
                        Takeoff.VantiyUrlHelper.ClearUrlFromCache(args.Owner.CustomUrl);
                    }
                }
            };
        }).Register();


        protected override void AddToCacheOverride()
        {
            base.AddToCacheOverride();
            var url = CustomUrl;
            if (url.HasChars())
            {
                Takeoff.VantiyUrlHelper.AddUrlToCache(url, Id);
            }
        }

        public bool HasVideo
        {
            get
            {
                return HasChildOfType<VideoStreamThing>();
            }
        }


        /// <summary>
        /// The hashed password that people must enter for semi-anonymous "quick" access to the production.
        /// </summary>
        public string GuestPassword
        {
            get
            {
                return GuestPasswordProperty.GetValue(this);
            }
            set
            {
                GuestPasswordProperty.SetValue(this, value);
            }
        }
        private string _GuestPassword;
        private static readonly ThingDataProperty<string, VideoThing, Data.Video> GuestPasswordProperty = new ThingDataProperty<string, VideoThing, Data.Video>()
        {
            Name = "GuestPassword",
            SetField = (o, v) => o._GuestPassword = v,
            GetField = o => o._GuestPassword,
            GetData = o => o.GuestPassword,
            SetData = (o, v) => o.GuestPassword = v,
        }.Register();


        /// <summary>
        /// If quick access is being used, this indicates whether people who have access can comment.
        /// </summary>
        public bool GuestsCanComment
        {
            get
            {
                return GuestsCanCommentProperty.GetValue(this);
            }
            set
            {
                GuestsCanCommentProperty.SetValue(this, value);
            }
        }
        private bool _GuestsCanComment;
        private static readonly ThingDataProperty<bool, VideoThing, Data.Video> GuestsCanCommentProperty = new ThingDataProperty<bool, VideoThing, Data.Video>()
        {
            Name = "GuestsCanComment",
            SetField = (o, v) => o._GuestsCanComment = v,
            GetField = o => o._GuestsCanComment,
            ShouldGetData = o => o.GuestsCanComment != null,
            GetData = o => (bool)o.GuestsCanComment,
            SetData = (o, v) => o.GuestsCanComment = v,
        }.Register();

        protected internal override IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            foreach (var d in base.FillAuxillaryData(db))
                yield return d;
            yield return this.CreateAuxDataFiller<Data.Video>(db);
        }

        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(FillRecordWithProperties(new Video()), InsertIdParam);
        }


        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            var data = (from d in db.Videos where d.ThingId == Id select d).Single();
            FillRecordWithProperties(data);
        }

        protected override void QueueDeleteData(CommandBatcher batcher)
        {
            base.QueueDeleteData(batcher);
            batcher.QueueDeleteViaPrimaryKey(new Data.Video
            {
                ThingId = Id
            });
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

        protected override ThingModelView CreateViewInstance(string viewName)
        {
            if (string.IsNullOrEmpty(viewName) || viewName.EqualsCaseSensitive("Details"))
                return new VideoThingDetailView();
            else if (viewName.EqualsCaseSensitive("Summary"))
                return new VideoThingSummaryView();

            throw new ArgumentException("Unknown view: " + viewName);
        }

        protected override void FillView(string viewName, ThingModelView view, Identity identity)
        {
            base.FillView(viewName, view, identity);

            var sourceVideoExists = this.HasChildOfType<FileThing>(false);

            if (string.IsNullOrEmpty(viewName) || viewName.Equals("Details", StringComparison.OrdinalIgnoreCase))
            {
                var typedView = (VideoThingDetailView)view;
                typedView.Title = Title;
                typedView.HasVideo = HasVideo;
                typedView.HasSource = sourceVideoExists;//source video is a FileThing, whereas watchable videos are VideoStreamThing objects.
                typedView.IsSourceDownloadable = sourceVideoExists && IsSourceDownloadable;
                typedView.Comments = this.Children.OfType<CommentThing>().OrderBy(c =>
                {
                    var asVidComment = c as VideoCommentThing;
                    if (asVidComment == null)
                        return -1.0;
                    else
                        return asVidComment.StartTime;
                }).ThenBy(c => c.CreatedOn.Ticks).Select(c => (CommentThingView)c.CreateViewData(identity)).ToArray();
                if ( typedView.CanEdit)
                {
                    if (GuestPassword.HasChars())
                    {
                        typedView.StandaloneCustomUrl = CustomUrl;
                        typedView.GuestPassword = DescryptGuestPassword(GuestPassword);
                        typedView.GuestsCanComment = GuestsCanComment;
                    }
                }
                typedView.Thumbnails = this.Children.OfType<VideoThumbnailThing>().OrderBy(t => t.Time).Select(c => (VideoThumbnailThingView)c.CreateViewData(identity)).ToArray();
//todo: when you move views over to standalone (not made by Thing), put back in video to play instead of inside productionvideoscontroller
            }
            else
            {
                var typedView = (VideoThingSummaryView)view;
                typedView.HasVideo = HasVideo;
                typedView.IsSourceDownloadable = sourceVideoExists && IsSourceDownloadable;
                typedView.Thumbnails = this.Children.OfType<VideoThumbnailThing>().OrderBy(t => t.Time).Select(c => (VideoThumbnailThingView)c.CreateViewData(identity)).ToArray();
                typedView.Title = Title;
            }
        }

        public static Lazy<string> LocalhostVideoUrl = new Lazy<string>(() =>
        {
            return ConfigurationManager.AppSettings["LocalhostVideoUrl"];
        });

        public VideoStreamThing GetStream(HttpBrowserCapabilitiesBase browser)
        {
            var streams = ChildrenOfType<VideoStreamThing>().ToArray();
            string desiredProfile = browser.IsMobileDevice ? "Mobile" : "Web";
            VideoStreamThing stream = streams.Where(s => s.Profile.EqualsCaseSensitive(desiredProfile)).FirstOrDefault();
            if (stream == null)
            {
                stream = streams.FirstOrDefault();
            }
            return stream;
        }

        public override ChangeDigestEmailItem GetActivityEmailContents(ActionSource change, IUser recipient)
        {
            if (change.Action == ThingChanges.Add && Title.HasChars() && this.HasVideo)
            {
                IUser author = change.UserId.IsPositive() ? Things.GetOrNull<UserThing>(change.UserId) : null;
                var item = new ChangeDigestEmailItem
                {
                    IsRecipientTheAuthor =
                        recipient != null && author != null && recipient.Id == author.Id
                };
                if (item.IsRecipientTheAuthor)
                {
                    item.Text = "You added a video called '" + this.Title.CharsOrEmpty().Truncate(40, StringTruncating.EllipsisCharacter) + "'";
                }
                else if (author == null)
                {
                    item.Text = "Somebody added a video called '" + this.Title.CharsOrEmpty().Truncate(40, StringTruncating.EllipsisCharacter) + "'";
                }
                else
                {
                    item.Text = author.DisplayName + " added a video called '" + this.Title.CharsOrEmpty().Truncate(40, StringTruncating.EllipsisCharacter) + "'";
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
                html.Append(" added a video ");
                if (!withinProject)
                {
                    html.Append(project.HtmlLink() + " : ");
                }

                string url;
                if (withinProject)
                {
                    url = "#video_" + this.Id.ToInvariant();
                }
                else
                {
                    url = project.Url() + "?focus=video_" + Id.ToInvariant();
                }

                html.Append("<a href='" + url + "'>");
                html.Append(Title.HtmlEncode());
                html.Append("</a> <em class='relativeDate'></em>");
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

    public class VideoThingDetailView : ThingBasicModelView
    {
        public string Title { get; set; }
        public bool HasVideo { get; set; }

        /// <summary>
        /// Indicates whether the original source video exists.
        /// </summary>
        public bool HasSource { get; set; }

        public CommentThingView[] Comments { get; set; }
        public bool IsSourceDownloadable { get; set; }
        public VideoThumbnailThingView[] Thumbnails { get; set; }
        /// <summary>
        /// The direct url to the video file.  This is an authenticated s3 url.  
        /// </summary>
        /// <remarks>
        /// Note: i used to have another url like productionvideos/345/watch that redirected to the amazon url.  However this would not work for whatever reason on HTML5 in Safari.
        /// </remarks>
        public string WatchUrl { get; set; }

        public string StandaloneCustomUrl { get; set; }

        public bool GuestsCanComment { get; set; }

        /// <summary>
        /// The decrypted guest password.
        /// </summary>
        public string GuestPassword { get; set; }
    }

    public class VideoThingSummaryView : ThingBasicModelView
    {
        public string Title { get; set; }

        /// <summary>
        /// Indicates whether a video is available for viewing.
        /// </summary>
        public bool HasVideo { get; set; }


        /// <summary>
        /// Indicates whether authorized users can download the source video.
        /// </summary>
        public bool IsSourceDownloadable { get; set; }

        public VideoThumbnailThingView[] Thumbnails { get; set; }
    }

}