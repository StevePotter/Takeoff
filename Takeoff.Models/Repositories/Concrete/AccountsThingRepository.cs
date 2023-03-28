using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.ThingRepositories
{
    public class AccountsThingRepository: IAccountsRepository
    {
        public IAccount Get(int id)
        {
            return Things.GetOrNull<AccountThing>(id);
        }

        /// <summary>
        /// Gets the account that is owned by the given user, but only if the account has been deleted.  
        /// </summary>
        /// <param name="ownerId"></param>
        /// <returns></returns>
        public IAccount GetDeletedAccount(int ownerUserId)
        {
            using (DataModel db = DataModel.ReadOnly)
            {
                var oldAccount = (from at in db.Things
                                  join a in db.Accounts on at.Id equals a.ThingId
                                  where
                                      at.Type == Things.ThingType(typeof(AccountThing)) &&
                                      at.OwnerUserId == ownerUserId && at.DeletedOn != null
                                  select new { Account = a, Thing = at }).FirstOrDefault();
                if ( oldAccount == null)
                    return null;
                var account = new AccountThing();
                account.FillPropertiesWithRecord(oldAccount.Thing);
                account.FillPropertiesWithRecord(oldAccount.Account);
                return account;
            }

        }


        public void BeginUpdate(IAccount entity)
        {
            entity.CastTo<AccountThing>().TrackChanges();
        }

        public void Update(IAccount entity)
        {
            entity.CastTo<AccountThing>().Update();
        }
    }
}
