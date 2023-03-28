using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Models.Data;

namespace Takeoff.ThingRepositories
{
    /// <summary>
    /// Uses LINQ2SQL to store plans.  
    /// </summary>
    /// <remarks>If you ever go with multiple web servers, you should move the cache into memcached or have some kind of cache sync among web servers.</remarks>
    public class PlansL2SRepository : IPlansRepository
    {
        private static IPlan[] _plans;
        private static Dictionary<string, IPlan> _plansById;

        public IEnumerable<IPlan> Get()
        {
            EnsureCache();
            return _plans;
        }


        public IEnumerable<IPlan> GetPlansForSale()
        {
            EnsureCache();
            return _plans.Where(p => p.AllowSignups).OrderBy(p => p.PriceInCents);
        }


        public IPlan Get(string id)
        {
            EnsureCache();
            if (!id.HasChars())
                return null;
            IPlan plan;
            if (_plansById.TryGetValue(id.ToLowerInvariant(), out plan))
                return plan;
            return null;
        }

        public void Delete(string id)
        {
            using (var db = new DataModel())
            {
                var data = (from p in db.Plans where p.Id == id select p).Single();
                db.Plans.DeleteOnSubmit(data);
                db.SubmitChanges();
            }
            RefreshCache();
        }


        public void Update(IPlan entity)
        {
            using (var db = new DataModel())
            {
                var data = (from p in db.Plans where p.Id == entity.Id select p).Single();
                entity.CopyTo(data);
                db.SubmitChanges();
            }
            RefreshCache();
        }

        public IPlan Insert(IPlan entity)
        {
            using (var db = new DataModel())
            {
                db.Plans.InsertOnSubmit(entity.CastTo<Plan>());
                db.SubmitChanges();
                RefreshCache();
            }
            return entity;
        }


        public IPlan Instantiate()
        {
            return new Plan();
        }

        static void EnsureCache()
        {
            if (_plans == null)
            {
                _plans = DataModel.ReadOnlyQuery(db => db.Plans.Cast<IPlan>().ToArray());
                _plansById = _plans.ToDictionary(p => p.Id.ToLowerInvariant());
            }
        }

        static void RefreshCache()
        {
            _plans = null;
            EnsureCache();
        }


    }
}
