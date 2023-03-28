using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.ThingRepositories
{
    public class ProductionsThingRepository : IProductionsRepository
    {

        public IProduction Get(int id)
        {
            return Things.GetOrNull<ProjectThing>(id);
        }


        public int GetSampleProductionId(int accountId)
        {
            using (var db = DataModel.ReadOnly)
            {
                return (from t in db.Things
                            join p in db.Projects on t.Id equals p.ThingId
                            where t.AccountId == accountId && (bool)p.IsSample
                            select t.Id).FirstOrDefault();
            }
        }
    }
}
