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

    /// <summary>
    /// Represents a member of an account.  These things should fall directly under an AccountThing.  
    /// </summary>
    [ThingType("AccountMembership")]
    [Serializable]
    public class AccountMembershipThing : MembershipThing, IAccountMembership
    {
        #region Constructors

        //static AccountMembershipThing()
        //{
        //}

        public AccountMembershipThing()
        {
        }

        protected AccountMembershipThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {  
        }

        #endregion
        
        
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
        public static ThingDataProperty<string, AccountMembershipThing, Data.AccountMembership> RoleProperty = new ThingDataProperty<string, AccountMembershipThing, Data.AccountMembership>()
        {
            Name = "Role",
            SetField = (o, v) => o._Role = v,
            GetField = o => o._Role,
            GetData = o => o.RoleName,
            SetData = (o, v) => o.RoleName = v,
        }.Register();

        protected override ThingModelView CreateViewInstance(string viewName)
        {
            return new AccountMembershipThingView();
        }

        protected override void FillView(string viewName, ThingModelView view, Identity identity)
        {
            base.FillView(viewName, view, identity);
            ((AccountMembershipThingView)view).Role = Role;
        }

        protected internal override IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            foreach (var d in base.FillAuxillaryData(db))
                yield return d;
            yield return this.CreateAuxDataFiller<Data.AccountMembership>(db);
        }


        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(FillRecordWithProperties(new Data.AccountMembership()), InsertIdParam);
        }


        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            FillRecordWithProperties((from d in db.AccountMemberships where d.ThingId == Id select d).Single());

            Things.GetOrNull<UserThing>(UserId).IfNotNull((user) => user.RemoveFromCache());
        }


        protected override void QueueDeleteData(CommandBatcher batcher)
        {
            base.QueueDeleteData(batcher);
            batcher.QueueDeleteViaPrimaryKey(new Data.AccountMembership
            {
                ThingId = Id
            });
        }

        public override T CreateLinkedThing<T>()
        {
            var thing = base.CreateLinkedThing<T>();
            thing.IfType<AccountMembershipThing>(t =>
            {
                t.Role = this.Role;
            });
            return thing;
        }
    }

    public class AccountMembershipThingView : MembershipThingView
    {
        public string Role { get; set; }
    }

}
