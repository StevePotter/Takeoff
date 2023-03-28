using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Models.Data;

namespace Takeoff.ThingRepositories
{
    /// <summary>
    /// Uses LINQ2SQL to store ISemiAnonymousUser objects  
    /// </summary>
    /// <remarks>If you ever go with multiple web servers, you should move the cache into memcached or have some kind of cache sync among web servers.</remarks>
    public class SemiAnonymousUsers2SRepository : ISemiAnonymousUsersRepository
    {
        static string GetCacheKey(Guid id)
        {
            return "sauser-" + id.ToString();//sauser = semi-anonymous user
        }

        class CachedSemiAnonymousUser : ISemiAnonymousUser
        {
            public Guid Id { get; set; }
            public DateTime CreatedOn { get; set; }
            public int? TargetId { get; set; }
            public string UserName { get; set; }
        }

        static void CopyProperties(ISemiAnonymousUser from, ISemiAnonymousUser to)
        {
            to.Id = from.Id;
            to.CreatedOn = from.CreatedOn;
            to.TargetId = from.TargetId;
            to.UserName = from.UserName;
        }

        public ISemiAnonymousUser Get(Guid id)
        {
            var fromCache = (string)CacheUtil.GetValueFromAppCache(GetCacheKey(id));
            if ( fromCache != null)
            {
                var data = JsonConvert.DeserializeObject<CachedSemiAnonymousUser>(fromCache);
                data.CreatedOn = DateTime.SpecifyKind(data.CreatedOn, DateTimeKind.Utc);//needed because deserializer was doign it as local
                return data;
            }
            using ( var db = DataModel.ReadOnly)
            {
                var user = db.SemiAnonymousUsers.FirstOrDefault(u => u.Id == id);
                if (user != null)
                {
                    var json = JsonConvert.SerializeObject(new CachedSemiAnonymousUser().Once(u =>
                                                                                              CopyProperties(user, u)));
                    CacheUtil.SetAppCacheValue(GetCacheKey(id), json);
                }
                return user;
            }
        }

        public ISemiAnonymousUser Instantiate()
        {
            return new SemiAnonymousUser();
        }

        public void Update(ISemiAnonymousUser entity)
        {
            using ( var db = new DataModel())
            {
                SemiAnonymousUser user = db.SemiAnonymousUsers.FirstOrDefault(u => u.Id == entity.Id);
                user.CreatedOn = entity.CreatedOn;
                user.TargetId = entity.TargetId;
                user.UserName = entity.UserName;
                db.SubmitChanges();
                CacheUtil.RemoveFromAppCache(GetCacheKey(entity.Id));
            }
        }

        public void Insert(ISemiAnonymousUser entity)
        {
            using (var db = new DataModel())
            {
                db.SemiAnonymousUsers.InsertOnSubmit((SemiAnonymousUser)entity);
                db.SubmitChanges();
            }

            var json = JsonConvert.SerializeObject(new CachedSemiAnonymousUser().Once(u =>
                                                                                      CopyProperties(entity, u)));
            CacheUtil.SetAppCacheValue(GetCacheKey(entity.Id), json);

        }
    }
}
