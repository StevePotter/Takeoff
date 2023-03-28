using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CommandLine;
using Newtonsoft.Json;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.DataTools.Commands
{

    //263054 is my user id
    public class TestCommand : BaseCommand
    {
        public TestCommand()
        {
            this.EnableXmlReport = false;
            this.UploadXmlReport = false;
            this.LogJobInDatabase = false;

            var d = ServiceStack.Text.JsonSerializer.DeserializeFromString("[1,2,{ \"3\":true }]", typeof(object[]));
            
        }
        //protected override void Perform(string[] commandLineArgs)
        //{
        //    Step("Hi", () =>
        //                   {
        //                       Thread.Sleep(1000);
        //                   });

        //}
        protected override void Perform(string[] commandLineArgs)
        {
            List<string> thingJsons = new List<string>();
            int[] ids = null;
            using (var db = DataModel.ReadOnly)
            {
                ids = (from t in db.Things where t.DeletedOn == null && t.IsContainer == true select t.Id).ToArray();
            }

            for (int i = 0; i < ids.Length; i++)
            {
                Console.WriteLine("Checking {0}, {1} of {2}", ids[i], i + 1, ids.Length);
                var thing = Things.Get(ids[i]);
                if (thing == null)
                    continue;
                Console.WriteLine(thing.Type);
                var json = thing.SerializeToString();
                thingJsons.Add(json);
            }

            Console.WriteLine("Running tests");
            var tests = GhettoPerfTest.GhettoTest(4, 4, () =>
                {
                    foreach (var json in thingJsons)
                    {
                        Newtonsoft.Json.Linq.JObject.Parse(json);
                    }
                },
                () =>
                {
                    foreach (var json in thingJsons)
                    {
                        Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeof(Dictionary<string, object>));
                    }
                },
                () =>
                {
                    foreach (var json in thingJsons)
                    {
                        ServiceStack.Text.JsonObject.Parse(json);
                    }
                });

            Console.WriteLine(tests[0].TotalSeconds);
            Console.WriteLine(tests[1].TotalSeconds);
            Console.WriteLine(tests[2].TotalSeconds);
            Console.WriteLine("Done");
        }

        void parseUsingServiceStack(string json)
        {
            var obj = ServiceStack.Text.JsonObject.Parse(json);
            foreach (var key in new string[] { "Children", "AccountMemberships", "Activity", "Memberships" })
            {
                string value;
                if (obj.TryGetValue(key, out value))
                {
                    //var children = 
                }
            }
        }

        //protected override void Perform(string[] commandLineArgs)
        //{
        //    int[] ids;
        //    using (var db = DataModel.ReadOnly)
        //    {
        //        ids = (from t in db.Things where t.DeletedOn == null && t.IsContainer == true select t.Id).ToArray();
        //    }

        //    for (int i = 0; i < ids.Length; i++)
        //    {
        //        Console.WriteLine("Checking {0}, {1} of {2}", ids[i], i + 1, ids.Length);
        //        var thing = Things.Get(ids[i]);
        //        if (thing == null)
        //            continue;
        //        Console.WriteLine(thing.Type);
        //        var toJsonOld = thing.SerializeToString();
        //        var toJsonNew = thing.SerializeToString2();
        //        if (!toJsonNew.Equals(toJsonOld))
        //        {
        //            throw new Exception("Don't match!");
        //        }
        //    }

        //}
    }
}
//Things.GetOrNull<VideoThing>(11147);

//using (var dh = DataModel.ReadOnly)
//{
//    var ids = (from t in dh.Things where t.Type == "Project" && t.DeletedOn == null select t.Id).ToArray();
//    int i = 1;
//    foreach (var id in ids)
//    {
//        Console.WriteLine("Checking Project {0} {1} of {2}", id, i, ids.Length);
//        ProjectThing newWay = Things.GetOrNull<ProjectThing>(id);
//        newWay.RemoveFromCache();
//        ProjectThing oldWay = Things.Get<ProjectThing>(id);
//        var newSer = newWay.SerializeToString();
//        var oldSer = oldWay.SerializeToString();
//        Debug.Assert(newSer.Equals(oldSer));
//        i++;
//    }

//    //ids = (from t in dh.Things where t.Type == "Account" && t.DeletedOn == null select t.Id).ToArray();
//    //i = 1;
//    //foreach (var id in ids)
//    //{
//    //    Console.WriteLine("Checking Account {0} {1} of {2}", id, i, ids.Length);
//    //    AccountThing newWay = Things.Get2<AccountThing>(id);
//    //    newWay.RemoveFromCache();
//    //    AccountThing oldWay = Things.Get<AccountThing>(id);
//    //    var newSer = newWay.SerializeToString();
//    //    var oldSer = oldWay.SerializeToString();
//    //    Debug.Assert(newSer.Equals(oldSer));
//    //    i++;
//    //}


//    //ids = (from t in dh.Things where t.Type == "User" && t.DeletedOn == null select t.Id).ToArray();
//    //i = 1;
//    //foreach (var id in ids)
//    //{
//    //    Console.WriteLine("Checking User {0} {1} of {2}", id, i, ids.Length);
//    //    UserThing newWay = Things.Get2<UserThing>(id);
//    //    newWay.RemoveFromCache();
//    //    UserThing oldWay = Things.Get<UserThing>(id);
//    //    var newSer = newWay.SerializeToString();
//    //    var oldSer = oldWay.SerializeToString();
//    //    Debug.Assert(newSer.Equals(oldSer));
//    //    i++;
//    //}
//}



        //private static void VerifyData(PrimeCacheOptions arguments, List<ThingBase> toSave)
        //{
        //    if (arguments.VerifyData)
        //    {
        //        Console.WriteLine("Double checking with single thing code");
        //        Dictionary<int, string> json = toSave.ToDictionary(p => p.Id, p => p.SerializeToString());
        //        Console.WriteLine("Getting JSON from bulk data took {0}", w.Elapsed.TotalSeconds);

        //        //clear everything out to force db fetch
        //        CacheUtil.ClearAppCache();
        //        CacheUtil.ThreadCache.Items.Clear();
        //        int curr = 0;
        //        foreach (var thingIdAndJson in json)
        //        {
        //            curr++;
        //            var fromThings = Things.Get(thingIdAndJson.Key);
        //            var currJson = thingIdAndJson.Value;
        //            var fromThingsJson = fromThings.SerializeToString();
        //            if (!fromThingsJson.Equals(currJson))
        //            {
        //                Console.WriteLine("{0} of {1}.  NO MATCH FOR {2} {3}", curr, json.Count, fromThings.Type, fromThings.Id);
        //            }
        //            else
        //            {
        //                Console.WriteLine("{0} of {1}.  Successful match {2} {3}", curr, json.Count, fromThings.Type,
        //                                  fromThings.Id);
        //            }
        //        }
        //    }
        //}
