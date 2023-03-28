using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Takeoff.Data;
using Takeoff.Models;

using Takeoff.Models.Data;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace Takeoff.Models
{

    /// <summary>
    /// Created when someone requests membership for a given thing.  Specifically, when someone requests membership to a production.  This will be placed directly under the Project thing.
    /// </summary>
    [ThingType("MembershipRequest")]
    [Serializable]
    public class MembershipRequestThing : ThingBase
    {

        #region Constructors

        public MembershipRequestThing()
        {
        }

        protected MembershipRequestThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {  
        }

        #endregion
        

        /// <summary>
        /// The user that either requested the membership or was invited
        /// </summary>
        public int InviteeId
        {
            get
            {
                return InviteeIdProperty.GetValue(this);
            }
            set
            {
                InviteeIdProperty.SetValue(this, value);
            }
        }
        private int _InviteeId;
        private static readonly ThingDataProperty<int, MembershipRequestThing, Data.MembershipRequest> InviteeIdProperty = new ThingDataProperty<int, MembershipRequestThing, Data.MembershipRequest>()
        {
            Name = "InviteeId",
            SetField = (o, v) => o._InviteeId = v,
            GetField = o => o._InviteeId,
            GetData = o => o.UserId,
            SetData = (o, v) => o.UserId = v,
        }.Register();
	

        /// <summary>
        /// If true, the request was made by a person who owns the production to a person who is not yet a member.  If false, the person who is not the member is requesting from the owner of the production.
        /// </summary>
        public bool IsInvitation
        {
            get
            {
                return IsInvitationProperty.GetValue(this);
            }
            set
            {
                IsInvitationProperty.SetValue(this, value);
            }
        }
        private bool _IsInvitation;
        private static readonly ThingDataProperty<bool, MembershipRequestThing, Data.MembershipRequest> IsInvitationProperty = new ThingDataProperty<bool, MembershipRequestThing, Data.MembershipRequest>()
        {
            Name = "IsInvitation",
            SetField = (o, v) => o._IsInvitation = v,
            GetField = o => o._IsInvitation,
            ShouldGetData = r => r.IsInvitation != null,
            GetData = o => (bool)o.IsInvitation,
            SetData = (o, v) => o.IsInvitation = v,
        }.Register();
	

        /// <summary>
        /// Only for invitations sent to new account members.  This indicates the role that will be set for the invitee once they accept the invitation.
        /// </summary>
        public string Role
        {
            get
            {
                return RoleProperty.GetValue(this);
            }
            set
            {
                RoleProperty.SetValue(this, value);
            }
        }
        private string _Role;
        private static readonly ThingDataProperty<string, MembershipRequestThing, Data.MembershipRequest> RoleProperty = new ThingDataProperty<string, MembershipRequestThing, Data.MembershipRequest>()
        {
            Name = "Role",
            SetField = (o, v) => o._Role = v,
            GetField = o => o._Role,
            GetData = o => o.Role,
            SetData = (o, v) => o.Role = v,
        }.Register();
	

        /// <summary>
        /// A personal note written by the user when they requested membership.
        /// </summary>
        public string Note
        {
            get
            {
                return NoteProperty.GetValue(this);
            }
            set
            {
                NoteProperty.SetValue(this, value);
            }
        }
        private string _Note;
        private static readonly ThingDataProperty<string, MembershipRequestThing, Data.MembershipRequest> NoteProperty = new ThingDataProperty<string, MembershipRequestThing, Data.MembershipRequest>()
        {
            Name = "Note",
            SetField = (o, v) => o._Note = v,
            GetField = o => o._Note,
            GetData = o => o.Note,
            SetData = (o, v) => o.Note = v,
        }.Register();


        protected internal override IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            foreach (var d in base.FillAuxillaryData(db))
                yield return d;
            yield return this.CreateAuxDataFiller<Data.MembershipRequest>(db);
        }


        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(FillRecordWithProperties(new Data.MembershipRequest()), InsertIdParam);
        }


        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            FillRecordWithProperties((from d in db.MembershipRequests where d.ThingId == Id select d).Single());
        }


        protected override ThingModelView CreateViewInstance(string viewName)
        {
            if ("Dashboard".EqualsCaseSensitive(viewName))
                return new MembershipRequestThingDashboardView();
            return new MembershipRequestThingView();
        }

        protected override void FillView(string viewName, ThingModelView view, Identity identity)
        {
            base.FillView(viewName, view, identity);

            if (InviteeId > 0)
            {
                var user = Things.GetOrNull<UserThing>(InviteeId);
                if (user != null)
                {
                    var typedView = (MembershipRequestThingView)view;
                    typedView.UserId = user.Id;
                    typedView.Email = user.Email;
                    typedView.Name = user.DisplayName;
                    typedView.IsInvitation = IsInvitation;

                    var asDashboard = view as MembershipRequestThingDashboardView;
                    if (asDashboard != null)
                    {
                        var production = Parent as ProjectThing;
                        if (production != null)
                        {
                            asDashboard.ProductionId = production.Id;
                            asDashboard.ProductionTitle = production.Title;
                        }
                    }
                }
            }

        }

    }


    /// <summary>
    /// Used when listing pending membership requests.
    /// </summary>
    public class MembershipRequestThingDashboardView : MembershipRequestThingView
    {
        public int ProductionId { get; set; }
        public string ProductionTitle { get; set; }
    }

    public class MembershipRequestThingView : ThingBasicModelView
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public bool IsInvitation { get; set; }
    }

}
