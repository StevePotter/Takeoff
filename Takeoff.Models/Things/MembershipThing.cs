using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Takeoff.Data;
using Takeoff.Models.Data;

namespace Takeoff.Models
{

    /// <summary>
    /// A member of some thing.
    /// </summary>
    /// <remarks>
    /// Note: TargetId in the new data model should be implemented soon (default to ParentId)
    /// Note: before inserting, you should check whether the user already has access and in that case, throw an exception.  
    /// </remarks>
    [ThingType("Membership")]
    [Serializable]
    public class MembershipThing : ThingBase, IMembership
    {

                #region Constructors

        public MembershipThing()
        {
        }

        protected MembershipThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {  
        }

        #endregion
        


        /// <summary>
        /// The user who has access to the thing specified by TargetId.
        /// </summary>
        /// TODO: clear user cache if changed 
        public int UserId
        {
            get
            {
                return UserIdProperty.GetValue(this);
            }
            set
            {
                UserIdProperty.SetValue(this, value);
            }
        }
        private int _UserId;
        private static readonly ThingDataProperty<int, MembershipThing, Data.Membership> UserIdProperty = new ThingDataProperty<int, MembershipThing, Data.Membership>()
        {
            Name = "UserId",
            SetField = (o, v) => o._UserId = v,
            GetField = o => o._UserId,
            GetData = o => o.UserId,
            SetData = (o, v) => o.UserId = v,
        }.Register();
	

        /// <summary>
        /// Indicates the ID of the thing this membership applies to.  If this is not set explicitly, it will default to ParentId.
        /// </summary>
        public int? TargetId
        {
            get
            {
                return TargetIdProperty.GetValue(this);
            }
            set
            {
                TargetIdProperty.SetValue(this, value);
            }
        }
        private int? _TargetId;
        private static readonly ThingDataProperty<int?, MembershipThing, Data.Membership> TargetIdProperty = new ThingDataProperty<int?, MembershipThing, Data.Membership>()
        {
            Name = "TargetId",
            SetField = (o, v) => o._TargetId = v,
            GetField = o => o._TargetId,
            DefaultValueCallback = o => o.ParentId,
            GetData = o => o.TargetId,
            SetData = (o, v) => o.TargetId = v,
        }.Register();
	


        protected internal override IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            foreach (var d in base.FillAuxillaryData(db))
                yield return d;
            yield return this.CreateAuxDataFiller<Data.Membership>(db);

        }


        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(FillRecordWithProperties(new Data.Membership()), InsertIdParam);

            Things.GetOrNull<UserThing>(UserId).IfNotNull((user) => user.RemoveFromCache());
        }

        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            FillRecordWithProperties((from d in db.Memberships where d.ThingId == Id select d).Single());
        }


        protected override void QueueDeleteData(CommandBatcher batcher)
        {
            base.QueueDeleteData(batcher);
            batcher.QueueDeleteViaPrimaryKey(new Data.Membership
            {
                ThingId = Id,
            });
        }

        protected override void OnDeleted()
        {
            Things.GetOrNull<UserThing>(UserId).IfNotNull((user) => user.RemoveFromCache());
        }

        public override T CreateLinkedThing<T>()
        {
            var thing = base.CreateLinkedThing<T>();
            thing.IfType<MembershipThing>(t => {
                t.UserId = this.UserId;
                t.TargetId = this.TargetId;
            });
            return thing;
        }


        protected override ThingModelView CreateViewInstance(string viewName)
        {
            return new MembershipThingView();
        }

        protected override void FillView(string viewName, ThingModelView view, Identity identity)
        {
            base.FillView(viewName, view, identity);

            var typedView = view as MembershipThingView;
            typedView.UserId = UserId;
            typedView.TargetId = TargetId.GetValueOrDefault();
            var user = Things.GetOrNull<UserThing>(UserId);
            if (user != null)
            {
                typedView.Email = user.Email;
                typedView.Name = user.DisplayName;
            }

        }


        public override ProductionActivityItem GetActivityPanelContents(ActionSource action, bool withinProject, Identity identity)
        {
            if (action.Action == ThingChanges.Add)
            {
                var project = Parent as ProjectThing;
                if (project == null)
                    return null;

                var newMember = Things.GetOrNull<UserThing>(UserId);
                if (newMember == null || project.CreatedByUserId == UserId)
                    return null;

                var html = new StringBuilder();
                if (newMember.Email.HasChars())
                    html.Append("<a href='mailto:" + newMember.Email + "'>" + newMember.DisplayName.HtmlEncode() + "</a>");
                else
                    html.Append(newMember.DisplayName.HtmlEncode());

                html.Append(" joined ");
                if (withinProject)
                    html.Append("this production ");
                else
                    html.Append(project.HtmlLink());
                html.Append(" <em class='relativeDate'></em>");
                return new ProductionActivityItem
                {
                    Html = html.ToString(),
                    CssClass = "invited",
                    Date = this.CreatedOn.ForJavascript(),
                    ThingId = this.Id
                };
            }
            return null;
        }

    }

    public class MembershipThingView : ThingBasicModelView
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public int TargetId { get; set; }
    }

}