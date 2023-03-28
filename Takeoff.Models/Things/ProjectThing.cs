using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Takeoff.Data;
using Takeoff.Models;
using System.Web.Script.Serialization;
using Takeoff.Models.Data;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Takeoff.Models
{

    [ThingType("Project")]
    [Serializable]
    public class ProjectThing : ThingBase, IProduction, IContainerThing
    {
        public const int MaxActivityCount = 50;
                
        #region Constructors

        public ProjectThing()
        {
        }

        protected ProjectThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {  
        }

        #endregion
        
        #region Properties

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
        private static readonly ThingDataProperty<string, ProjectThing, Data.Project> TitleProperty = new ThingDataProperty<string, ProjectThing, Data.Project>()
        {
            Name = "Title",
            SetField = (o, v) => o._Title = v,
            GetField = o => o._Title,
            GetData = o => o.Title,
            SetData = (o, v) => o.Title = v,
        }.Register();


        public bool IsSemiAnonymousAccessEnabled
        {
            get { return GuestPassword.HasChars(); }
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
        private static readonly ThingDataProperty<string, ProjectThing, Data.Project> GuestPasswordProperty = new ThingDataProperty<string, ProjectThing, Data.Project>()
        {
            Name = "GuestPassword",
            SetField = (o, v) => o._GuestPassword = v,
            GetField = o => o._GuestPassword,
            GetData = o => o.SemiAnonymousPassword,
            SetData = (o, v) => o.SemiAnonymousPassword = v,
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
        private static readonly ThingDataProperty<bool, ProjectThing, Data.Project> GuestsCanCommentProperty = new ThingDataProperty<bool, ProjectThing, Data.Project>()
        {
            Name = "GuestsCanComment",
            SetField = (o, v) => o._GuestsCanComment = v,
            GetField = o => o._GuestsCanComment,
            ShouldGetData = o => o.SemiAnonymousUserCanComment != null,
            GetData = o =>(bool)o.SemiAnonymousUserCanComment,
            SetData = (o, v) => o.SemiAnonymousUserCanComment = v,
        }.Register();
        
        /// <summary>
        /// When set, indicates the ID of the direct child ImageThing that represents a logo shown next to the title.
        /// </summary>
        public int? LogoImageId
        {
            get
            {
                return LogoImageIdProperty.GetValue(this);
            }
            set
            {
                LogoImageIdProperty.SetValue(this, value);
            }
        }
        private int? _LogoImageId;
        private static readonly ThingDataProperty<int?, ProjectThing, Data.Project> LogoImageIdProperty = new ThingDataProperty<int?, ProjectThing, Data.Project>()
        {
            Name = "LogoImageId",
            SetField = (o, v) => o._LogoImageId = v,
            GetField = o => o._LogoImageId,
            ShouldGetData = r => r.LogoImageId != null,
            GetData = o => o.LogoImageId,
            SetData = (o, v) => o.LogoImageId = v,
        }.Register();


        /// <summary>
        /// Indicates whether this is a sample production created when the user signed up.  When setting to true, you should also set IsComplimentary to true (it's not automatically done).
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
        private static readonly ThingDataProperty<bool, ProjectThing, Data.Project> IsSampleProperty = new ThingDataProperty<bool, ProjectThing, Data.Project>()
        {
            Name = "IsSample",
            SetField = (o, v) => o._IsSample = v,
            GetField = o => o._IsSample,
            ShouldGetData = r => r.IsSample != null,
            GetData = o => (bool)o.IsSample,
            SetData = (o, v) => o.IsSample = v,
        }.Register();


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
        private static readonly ThingDataProperty<bool, ProjectThing, Data.Project> IsComplimentaryProperty = new ThingDataProperty<bool, ProjectThing, Data.Project>()
        {
            Name = "IsComplimentary",
            SetField = (o, v) => o._IsComplimentary = v,
            GetField = o => o._IsComplimentary,
            ShouldGetData = r => r.IsComplimentary != null,
            GetData = o => (bool)o.IsComplimentary,
            SetData = (o, v) => o.IsComplimentary = v,
        }.Register();

      
        public string FilesLocation
        {
            get
            {
                return FilesLocationProperty.GetValue(this);
            }
            set
            {
                FilesLocationProperty.SetValue(this, value);
            }
        }
        private string _FilesLocation;
        private static readonly ThingDataProperty<string, ProjectThing, Data.Project> FilesLocationProperty = new ThingDataProperty<string, ProjectThing, Data.Project>()
        {
            Name = "FilesLocation",
            SetField = (o, v) => o._FilesLocation = v,
            GetField = o => o._FilesLocation,
            GetData = o => o.FilesLocation,
            SetData = (o, v) => o.FilesLocation = v,
        }.Register();

        public string VanityUrl
        {
            get
            {
                return VanityUrlProperty.GetValue(this);
            }
            set
            {
                VanityUrlProperty.SetValue(this, value);
            }
        }
        private string _VanityUrl;
        private static readonly ThingDataProperty<string, ProjectThing, Data.Project> VanityUrlProperty = new ThingDataProperty<string, ProjectThing, Data.Project>()
        {
            Name = "VanityUrl",
            SetField = (o, v) => o._VanityUrl = v,
            GetField = o => o._VanityUrl,
            GetData = o => o.VanityUrl,
            SetData = (o, v) => o.VanityUrl = v,
        }.Once(prop => {
            prop.BeforeSet += (e, args) =>
            {
                if (VanityUrlProperty.IsSet(args.Owner) && !args.Owner.VanityUrl.EqualsCaseSensitive(args.NewValue))
                {
                    if ( args.Owner.VanityUrl.HasChars())
                    {
                        Takeoff.VantiyUrlHelper.ClearUrlFromCache(args.Owner.VanityUrl);
                    }
                }
            };
        }).Register();



        /// <summary>
        /// The ID in ChangeDetails that is the latest change to happen to this thing or its descendants.
        /// </summary>
        public int? LastChangeId
        {
            get { return LastChangeIdProperty.GetValue(this); }
            set { LastChangeIdProperty.SetValue(this, value); }
        }

        private static readonly ThingProperty<int?, ProjectThing> LastChangeIdProperty = new ThingProperty<int?, ProjectThing>
        {
            Name = "LastChangeId",
            SetField =
                (o, v) =>
                o._LastChangeId = v,
            GetField =
                o => o._LastChangeId,
        }.Register();
        private int? _LastChangeId;

        /// <summary>
        /// The date of the latest change that occured for this thing.
        /// </summary>
        public DateTime? LastChangeDate
        {
            get { return LastChangeDateProperty.GetValue(this); }
            set { LastChangeDateProperty.SetValue(this, value); }
        }

        private static readonly ThingProperty<DateTime?, ProjectThing> LastChangeDateProperty =
            new ThingProperty<DateTime?, ProjectThing>
            {
                Name = "LastChangeDate",
                SetField = (o, v) => o._LastChangeDate = v,
                GetField = o => o._LastChangeDate,
            }.Register();
        private DateTime? _LastChangeDate;


        public ActionSource[] Activity { get; set; }


        #endregion

        public ImageThing GetLogo()
        {
            if (LogoImageId.HasValue)
            {
                var logo = (ImageThing)FindDescendantById(LogoImageId.Value);
                if (logo != null)
                    return logo;
            }

            var account = Things.GetOrNull<AccountThing>(AccountId);
            if (account != null && account.LogoImageId.HasValue)
            {
                var logo = (ImageThing)account.FindDescendantById(account.LogoImageId.Value);
                if (logo != null)
                    return logo;
            }
            return null;
        }

        /// <summary>
        /// Gets the latest video that was uploaded for this production.
        /// </summary>
        /// <returns></returns>
        public VideoThing GetLatestVideo()
        {
            return this.ChildrenOfType<VideoThing>().OrderByDescending(o => o.CreatedOn).FirstOrDefault();
        }

        public override JObject SerializeJson()
        {
            dynamic json = base.SerializeJson();
            if (Activity.HasItems())
            {
                json.Activity = new JArray(Activity.Select(a => JObject.FromObject(a)).ToArray());
            }
            return json;
        }

        public override void SerializeJsonProperties2(JsonWriter writer)
        {
            base.SerializeJsonProperties2(writer);
            if (Activity.HasItems())
            {
                writer.WritePropertyName("Activity");
                writer.WriteStartArray();
                foreach( var activity in Activity.Take(10))
                {
                    writer.WriteStartObject();
                    if (activity.Action != null)
                    {
                        writer.WritePropertyName("Action");
                        writer.WriteValue(activity.Action);
                    }
                    if (activity.Data != null)
                    {
                        writer.WritePropertyName("Data");
                        writer.WriteValue(activity.Data);
                    }
                    writer.WritePropertyName("Date");
                    writer.WriteValue(activity.Date);
                    if (activity.Description != null)
                    {
                        writer.WritePropertyName("Description");
                        writer.WriteValue(activity.Description);
                    }
                    if (activity.Id.IsPositive())
                    {
                        writer.WritePropertyName("Id");
                        writer.WriteValue(activity.Id);
                    }
                    if (activity.ThingId.IsPositive())
                    {
                        writer.WritePropertyName("ThingId");
                        writer.WriteValue(activity.ThingId);
                    }
                    if (activity.ThingParentId.HasValue)
                    {
                        writer.WritePropertyName("ThingParentId");
                        writer.WriteValue(activity.ThingParentId.Value);
                    }
                    if (activity.ThingType != null)
                    {
                        writer.WritePropertyName("ThingType");
                        writer.WriteValue(activity.ThingType);
                    }
                    if (activity.UserId.IsPositive())
                    {
                        writer.WritePropertyName("UserId");
                        writer.WriteValue(activity.UserId);
                    }
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }
        }



        public override void DeserializeJson(JObject json)
        {
            base.DeserializeJson(json);
            json["Activity"].IfNotNull(v =>
            {
                this.Activity = ((JArray)v).Cast<JObject>().Select(activityJson =>
                {
                    var action = new ActionSource();
                    activityJson["Action"].IfNotNull(av => action.Action = (string)av);
                    activityJson["Data"].IfNotNull(av => action.Data = (string)av);
                    activityJson["Description"].IfNotNull(av => action.Description = (string)av);
                    activityJson["Date"].IfNotNull(av => action.Date = (DateTime)av);
                    activityJson["Id"].IfNotNull(av => action.Id = (int)av);
                    activityJson["ThingId"].IfNotNull(av => action.ThingId = (int)av);
                    activityJson["ThingParentId"].IfNotNull(av => action.ThingParentId = (int?)av);
                    activityJson["ThingType"].IfNotNull(av => action.ThingType = (string)av);
                    activityJson["UserId"].IfNotNull(av => action.UserId = (int)av);
                    return action;
                }).ToArray();
            });
        }


        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(FillRecordWithProperties(new Project()), InsertIdParam);
        }

        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            var data = (from d in db.Projects where d.ThingId == Id select d).Single();
            FillRecordWithProperties(data);
        }

        protected override void QueueDeleteData(CommandBatcher batcher)
        {
            base.QueueDeleteData(batcher);
            batcher.QueueDeleteViaPrimaryKey(new Data.Project
            {
                ThingId = Id
            });
        }


        protected override ThingModelView CreateViewInstance(string viewName)
        {
            if (viewName == "Summary")
                return new ProjectThingSummaryView();
            return new ProjectThingView();
        }

        private class ChangeSourceDuringFillData
        {
            public int ChangeId { get; set; }
            public DateTime ChangeDate { get; set; }
        }

        class ProjectDataModel
        {
            public string Title;
            public object Props;
        }

        public override void AddDataFillers(List<IThingAuxDataFiller> fillers, Dictionary<int, ThingBase> things, DataModel db)
        {
            base.AddDataFillers(fillers, things, db);

            (from data in db.Comments
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
            ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.Files
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
            ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.Images
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.MembershipRequests
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.Memberships
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.Projects
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.VideoComments
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.VideoStreams
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.VideoThumbnails
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.Videos
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);


            (from change in db.Actions
            join project in db.Projects on change.ThingId equals project.ThingId
            join productionThing in db.Things on project.ThingId equals productionThing.Id
            join changeSource in db.ActionSources on change.ChangeDetailsId equals changeSource.Id
            where productionThing.DeletedOn == null && productionThing.Id == Id
            orderby changeSource.Id descending
            select new ActionSourceFromData
            {
                Action = changeSource.Action,
                Data = changeSource.Data,
                Date = changeSource.Date,
                Description = changeSource.Description,
                Id = changeSource.Id,
                ProductionId = project.ThingId,
                ThingId = changeSource.ThingId,
                ThingParentId = changeSource.ThingParentId,
                ThingType = changeSource.ThingType,
                UserId = changeSource.UserId
            }
            ).Take(ProjectThing.MaxActivityCount).FillAll(data =>
            {
                var activity = new List<ActionSource>();
                foreach (var change in data)
                {
                    if (activity.Count == 0)
                    {
                        LastChangeDate = change.Date;
                        LastChangeId = change.Id;
                    }
                    activity.Add(new ActionSource
                    {
                        Id = change.Id,
                        Action = change.Action,
                        Data = change.Data,
                        Date = change.Date,
                        Description = change.Description,
                        ThingId = change.ThingId,
                        ThingParentId = change.ThingParentId,
                        ThingType = change.ThingType,
                        UserId = change.UserId,
                    });
                }
                this.Activity = activity.ToArray();
            }).AddTo(fillers);

        }

        private static void FillThingData(int id, object data, Dictionary<int, ThingBase> things)
        {
            ThingBase thing;
            if (things.TryGetValue(id, out thing))
            {
                thing.FillPropertiesWithRecord(data);
            }
        }


        protected internal override IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            foreach (var d in base.FillAuxillaryData(db))
                yield return d;

            yield return this.CreateAuxDataFiller<Data.Project>(db);



            //I did this to reduce the number of sql parameters because we broke through the limit a bunch of times
            string query =
                "SELECT TOP (1) [ChangeDetailsId] AS [ChangeId], [Date] AS [ChangeDate] FROM [dbo].[Change] AS [t0] " +
                "INNER JOIN [dbo].[ChangeSource] AS [t1] ON [t0].[ChangeDetailsId] = [t1].[Id]" +
                "WHERE [t0].[ThingId] = " + Id.ToInvariant() + " ORDER BY [t0].[Id] DESC";
            yield return new ThingAuxDataFiller<ChangeSourceDuringFillData>(query, (d) =>
            {
                LastChangeId = d.ChangeId;
                LastChangeDate = d.ChangeDate;
            });


            //yield return DataFillers.Query(
            //    (from change in db.Actions
            //    join project in db.Projects on change.ThingId equals project.ThingId
            //    join productionThing in db.Things on project.ThingId equals productionThing.Id
            //    join changeSource in db.ActionSources on change.ChangeDetailsId equals changeSource.Id
            //    where productionThing.DeletedOn == null && productionThing.Id == Id
            //    orderby changeSource.Id descending
            //    select new ActionSourceFromData
            //               {
            //                   Action = changeSource.Action,
            //                   Data = changeSource.Data,
            //                   Date = changeSource.Date,
            //                   Description = changeSource.Description,
            //                   Id = changeSource.Id,
            //                   ProductionId = project.ThingId,
            //                   ThingId = changeSource.ThingId,
            //                   ThingParentId = changeSource.ThingParentId,
            //                   ThingType = changeSource.ThingType,
            //                   UserId = changeSource.UserId
            //               }).Take(ProjectThing.MaxActivityCount)
            //    ).FillAll(data =>
            //               {
            //                   List<Data.ActionSource> activity = new List<ActionSource>();
            //                   foreach( var change in data)
            //                   {
            //                       if ( activity.Count == 0 )
            //                       {
            //                           LastChangeDate = change.Date;
            //                           LastChangeId = change.Id;
            //                       }
            //                       activity.Add(new ActionSource
            //                                        {
            //                                            Id = change.Id,
            //                                            Action = change.Action,
            //                                            Data = change.Data,
            //                                            Date = change.Date,
            //                                            Description = change.Description,
            //                                            ThingId = change.ThingId,
            //                                            ThingParentId = change.ThingParentId,
            //                                            ThingType = change.ThingType,
            //                                            UserId = change.UserId,
            //                                        });
            //                   }
            //                   this.Activity = activity.ToArray();
            //               });

            query = "SELECT top (50) [t1].[Id], [t1].[Date], [t1].[UserId], [t1].[ThingId], [t1].[ThingType], [t1].[ThingParentId], [t1].[Action], [t1].[Data] " +
                "FROM [dbo].[Change] AS [t0] " +
                "INNER JOIN [dbo].[ChangeSource] AS [t1] ON [t0].[ChangeDetailsId] = [t1].[Id] " +
                "WHERE [t0].[ThingId] = " + Id.ToInvariant() + " " +
                "ORDER BY [t0].[Id] DESC";

            yield return new ThingAuxDataFiller<Data.ActionSource>(query, new Action<IEnumerable<Data.ActionSource>>((a) =>
                                                                   {
                                                                       Activity = a.ToArray();
                                                                   }));
        }

        protected override void OnDeleted()
        {
            base.OnDeleted();
            //because AccountThing stores stats in its cache (for performance since it's hit a lot), clear the cache for it
            Things.GetOrNull<AccountThing>(AccountId).IfNotNull(a => a.RemoveFromCache());
        }

        protected override void OnInserted()
        {
            base.OnInserted();
            //because AccountThing stores stats in its cache (for performance since it's hit a lot), clear the cache for it
            Things.GetOrNull<AccountThing>(AccountId).IfNotNull(a => a.RemoveFromCache());
        }


        protected override void AddToCacheOverride()
        {
            base.AddToCacheOverride();
            var url = VanityUrl;
            if (url.HasChars())
            {
                Takeoff.VantiyUrlHelper.AddUrlToCache(url, Id);
            }
        }

        protected override void RemoveFromCacheOverride()
        {
            base.RemoveFromCacheOverride();
            var url = VanityUrl;
            if (url.HasChars())
            {
                Takeoff.VantiyUrlHelper.ClearUrlFromCache(url);
            }
        }

        

        protected override void FillView(string viewName, ThingModelView viewIn, Identity identity)
        {
            var asSummary = viewIn as ProjectThingSummaryView;
            if (asSummary != null)
            {
                asSummary.LastChangeDate = LastChangeDate.GetValueOrDefault(this.CreatedOn).ForJavascript();
                base.FillView(viewName, asSummary, identity);
                asSummary.Title = Title;
                return;
            }
            else
            {
                var view = (ProjectThingView)viewIn;
                base.FillView(viewName, view, identity);
                view.Title = Title;
                view.LastChangeId = LastChangeId;
                view.LastChangeDate = LastChangeDate.GetValueOrDefault(this.CreatedOn).ForJavascript();

                if (LogoImageId.HasValue)
                {
                    view.HasSpecificLogo = true;
                    view.Logo = (ImageThingView)FindDescendantById(LogoImageId.Value).MapIfNotNull(t => t.CreateViewData(identity));
                }

                var userIdentity = identity as UserIdentity;
                if (userIdentity != null)
                {
                    UserThing user = userIdentity.MapIfNotNull(u => Things.GetOrNull<UserThing>(u.UserId).EnsureExists(u.UserId));
                    view.IsMember = true;
                    view.CanAddComment = Permissions.HasPermission(this, typeof(VideoCommentThing), Permissions.Add,
                                                                   user);
                    view.CanAddCommentReply = Permissions.HasPermission(this, typeof (CommentReplyThing),
                                                                        Permissions.Add, user);
                    view.CanAddMember = Permissions.HasPermission(this, typeof (MembershipThing), Permissions.Add, user);
                    view.CanAddVideo = Permissions.HasPermission(this, typeof (VideoThing), Permissions.Add, user);
                    view.CanAddFile = Permissions.HasPermission(this, typeof (CommentReplyThing), Permissions.Add, user);
                }
                else
                {
                    view.IsMember = false;

                    if (this.GuestsCanComment)
                    {
                        var semiAnonymousUser = identity as SemiAnonymousUserIdentity;
                        if (semiAnonymousUser != null)
                        {
                            view.CanAddComment = true;
                            view.CanAddCommentReply = true;
                        }
                    }
                }
                view.Videos = this.ChildrenOfType<VideoThing>().OrderByDescending(o => o.CreatedOn).Select((c, i) => (VideoThingSummaryView)c.CreateViewData("Summary", identity)).ToArray();
                view.Files = this.ChildrenOfType<FileThing>().Select(c => (FileThingView)c.CreateViewData(identity)).ToArray();
                view.Members = this.ChildrenOfType<MembershipThing>().Where(t => t.UserId > 0).Select(c => (MembershipThingView)c.CreateViewData(identity)).ToArray();
                view.MembershipRequests = this.ChildrenOfType<MembershipRequestThing>().Select(c => (MembershipRequestThingView)c.CreateViewData(identity)).Where(c => c.UserId > 0).ToArray();//the > 0 handles orphan requests for users that were deleted from the system.  rare because those should be purged when account is deleted
                int max = 5;
                view.Activity = Activity == null ? new object[0] : GetActivityPanelItems(max, Activity, identity).ToArray(); //Activity.Take(10).Select(c => ThingChanges.CreateChangeView(c, this)).ToArray();
            }
        }

        public List<object> GetActivityPanelItems(int max, IEnumerable<ActionSource> activitySources, Identity identity)
        {
            var activity = new List<object>();
            foreach (var currAction in activitySources)
            {
                ThingBase changedThing = null;
                if (currAction.ThingId == Id)
                    changedThing = this;
                else
                {
                    changedThing = FindDescendantById(currAction.ThingId);
                }
                //in this case the thing was deleted.  so we ignore the change.  the Delete change will maybe be included in the change set or a ancestor was delted
                if (changedThing != null)
                {
                    var contents = changedThing.GetActivityPanelContents(currAction, true, identity);
                    if (contents != null)
                    {
                        activity.Add(contents);
                        if (activity.Count == max)
                            break;
                    }
                }
            }
            return activity;
        }

        public override ProductionActivityItem GetActivityPanelContents(ActionSource action, bool withinProject, Identity identity)
        {
            if (action.Action == ThingChanges.Add && !withinProject)
            {
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

                html.Append(" created a production " + this.HtmlLink());
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


        IChange[] IProduction.Activity
        {
            get
            {
                if (!Activity.HasItems())
                    return null;
                return Activity.Cast<IChange>().ToArray();
            }
        }
    }


    /// <summary>
    /// Contains everything required for the production details view.
    /// </summary>
    public class ProjectThingView : ThingBasicModelView
    {
        public string Title { get; set; }
        public int? LastChangeId { get; set; }
        public long LastChangeDate { get; set; }

        /// <summary>
        /// The videos, ordered by CreatedOn descending.
        /// </summary>
        public VideoThingSummaryView[] Videos { get; set; }
        public FileThingView[] Files { get; set; }

        public VideoThingDetailView CurrentVideo { get; set; }
        public MembershipThingView[] Members { get; set; }
        public MembershipRequestThingView[] MembershipRequests { get; set; }

        public ImageThingView Logo { get; set; }

        /// <summary>
        /// Indicates whether there is a 'standard' logo that applies to all productions without their own specific one.
        /// </summary>
        public bool HasStandardLogo { get; set; }
        /// <summary>
        /// Indicates whether there is a specific logo for this particular production.
        /// </summary>
        public bool HasSpecificLogo { get; set; }

        public object[] Activity { get; set; }

        public bool IsMember { get; set; }

        public bool CanAddComment { get; set; }
        public bool CanAddCommentReply { get; set; }
        public bool CanAddMember { get; set; }
        public bool CanAddVideo { get; set; }
        public bool CanAddFile { get; set; }

    }

    public class ProjectThingSummaryView : ThingBasicModelView
    {
        public string Title { get; set; }
        public long LastChangeDate { get; set; }

    }




}
