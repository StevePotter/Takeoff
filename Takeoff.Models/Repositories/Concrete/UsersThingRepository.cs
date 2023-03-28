using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.ThingRepositories
{
    public class UsersThingRepository: IUsersRepository
    {

        public IUser Get(int id)
        {
            return Things.GetOrNull<UserThing>(id);
        }


        public IUser GetByEmail(string email)
        {
            var id = GetId(email);
            if (id <= 0)
                return null;
            else
                return Get(id);
        }

        public bool IsMemberOf(IUser user, ITypicalEntity entity)
        {
            return user.CastTo<UserThing>().IsMemberOf(entity.Id);//right now IDs are globally unique amongst anything you can be a member of, so ID is all you need.
        }


        public IUser Instantiate()
        {
            return new UserThing();
        }

        public IUser Insert(IUser entity)
        {
            var user = (UserThing) entity;
            user.IsContainer = true;
            user.Insert();
            return user;
        }

        public void BeginUpdate(IUser entity)
        {
            entity.CastTo<UserThing>().TrackChanges();
        }

        public IUser Update(IUser entity)
        {
            var user = (UserThing)entity;
            user.Update();
            return user;
        }


        public int GetId(string email)
        {
            email = Args.HasCharsLower(email);
            var cacheKey = UserThing.CacheKeyForEmail(email);
            return CacheUtil.GetCachedWithStringSerialization<int>(cacheKey, true, () =>
            {
                using (var db = DataModel.ReadOnly)
                {
                    var user = (from o in db.Users where o.Email == email select new { o.ThingId }).FilterDeletedThings(db, t=>t.ThingId).SingleOrDefault();
                    return user == null ? 0 : user.ThingId;
                }
            });
        }



        public ISetting GetSetting(IUser user, string name)
        {
            return user.CastTo<UserThing>().GetSetting(name);
        }


        public IUser Update(IUser entity, string description)
        {
            var user = (UserThing)entity;
            user.Update(description: description);
            return user;
        }


        public bool IsPasswordRight(IUser entity, string password)
        {
            throw new NotImplementedException();
        }


        public EntityExistance GetExistanceByEmail(string email)
        {
            using (var db = DataModel.ReadOnly)
            {
                //include deleted users
                var existingUser = (from ut in db.Things
                                    join u in db.Users on ut.Id equals u.ThingId
                                    where u.Email == email.ToLowerInvariant().Trim()
                                    select new { ut.DeletedOn }).FirstOrDefault();
                if (existingUser == null)
                    return EntityExistance.Never;

                return existingUser.DeletedOn.HasValue ? EntityExistance.Deleted : EntityExistance.Active;
            }
        }
    }
}
