//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data.Entity;
//using System.Data.Entity.ModelConfiguration;
//using System.Data.Linq;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using CommandLine;
//using Takeoff.Data;
//using Takeoff.DataTools.Commands.DataFillerModels;
//using Takeoff.Models;
//using Takeoff.Models.Data;
//using System.IO;

//namespace Takeoff.DataTools.Commands
//{
//    public class PrimeCacheOptions
//    {
//        [Option("c", "clear", Required = false, HelpText = "Clears the cache.")]
//        public bool Clear;

//        [Option("l", "log", Required = false, HelpText = "When set, this indicates whether a report is added to our databases.")]
//        public bool Log; 
//    }

//    public class PrimeCacheCommand : BaseCommandWithOptions<PrimeCacheOptions>
//    {
//        public PrimeCacheCommand()
//        {
//            EnableXmlReport = true;
//            NotifyOnErrors = true;
//            LogJobInDatabase = true;
//        }

//        protected override void Perform(PrimeCacheOptions arguments)
//        {
//            LogJobInDatabase = Options.Log;
//            UploadXmlReport = Options.Log;
//            if (arguments.Clear)
//            {
//                Step("ClearCache", CacheUtil.ClearAppCache);
//            }

//            var fillers = new List<IThingAuxDataFiller>();
//            var things = new Dictionary<int, ThingBase>();
//            var productionChanges = new List<ActionSourceFromData>();
//            var db = DataModel.ReadOnly;

//            Step("CreateDataFillers", () => CreateDataFillers(fillers, things, productionChanges, db));

//            var queries = new List<object>(fillers.Select(d => d.GetQuery()));

//            IMultipleResults results = null;
//            Step("ExecuteQuery", () =>
//                                     {
//                                         results = db.ExecuteQuery(queries);
//                                     });

//            Step("FillData", () =>
//            {
//                foreach (var f in fillers)
//                {
//                    f.Fill(results);
//                }
//            });

//            //now we make sense of all the things we just created and filled with data
//            var projects = new List<ProjectThing>();
//            var users = new List<UserThing>();
//            var accounts = new List<AccountThing>();
//            var containers = new List<ThingBase>();                    
//            var toSave = new List<ThingBase>();
//            List<ThingBase> descendants = new List<ThingBase>();
//            foreach( var thing in things.Values)
//            {
//                thing.AddTo(thing.IsContainer ? containers : descendants);
//                thing.IfType<ProjectThing>(p => p.AddTo(projects));
//                thing.IfType<UserThing>(p => p.AddTo(users));
//                thing.IfType<AccountThing>(p => p.AddTo(accounts));
//            }

//            Step("BuildTrees", () =>
//            {
//                AssembleThingTrees(containers, descendants, things);
//                CompleteProductionTrees(projects, productionChanges, things);
//                CompleteUserTrees(users, things);
//                toSave.AddRange(projects);
//                toSave.AddRange(users);
//                toSave.AddRange(accounts);
//                AddReportAttribute("TotalThings", things.Count);
//                WriteLine("{0} things total", false, things.Count);
//            });
//            Step("AddToCache", () =>
//            {
//                AddReportAttribute("ContainerThings", toSave.Count);
//                foreach (var container in toSave)
//                {
//                    container.AddToCache();
//                }
//            });

//            WriteLine("Stats:");
//            WriteLine("{0} things total", false, things.Count);
//            WriteLine("{0} users", false, users.Count);
//            WriteLine("{0} accounts", false, accounts.Count);
//            WriteLine("{0} productions", false, projects.Count);

//            Step("RefreshBlog", () => BlogItemRssRepository.RefreshEntries());
//            Step("RefreshTweets", () => TweetsRepository.RefreshEntries());
//        }

//        private static void AssembleThingTrees(List<ThingBase> containers, List<ThingBase> descendants, Dictionary<int, ThingBase> thingsPerId)
//        {            
//            //assemble thing trees
//            var thingsPerParentId = descendants.ToLookup(t => t.ParentId);
//            foreach (var parentIdAndChildren in thingsPerParentId)
//            {
//                if (parentIdAndChildren.Key.GetValueOrDefault() == 0)
//                    continue;
//                ThingBase parent;
//                if (thingsPerId.TryGetValue(parentIdAndChildren.Key.Value, out parent))
//                {
//                    foreach (var child in parentIdAndChildren)
//                    {
//                        if (child.IsContainer)
//                        {
                            
//                        }
//                        else
//                        {
//                            parent.Children.Add(child);
                            
//                        }
//                    }
//                }
//                else
//                {
//                    //this happened a few times.  for example, a membership was provided that actually lived under an account, not a production
//                    //foreach (var child in parentIdAndChildren)
//                    //{

//                    //}
//                }
//            }

//            foreach (var container in containers)
//            {
//                container.Descendants().Each(d =>
//                {
//                    if ( !d.ContainerId.HasValue )
//                        d.ContainerId = container.Id;
//                });
//            }
//        }

//        private static void CompleteProductionTrees(List<ProjectThing> projects, List<ActionSourceFromData> changes, Dictionary<int, ThingBase> thingsPerId)
//        {

//            //set the changes for each production
//            foreach (var changesForProduction in changes.ToLookup(t => t.ProductionId))
//            {
//                var production = thingsPerId[changesForProduction.Key].CastTo<ProjectThing>();
//                production.Activity = changesForProduction.Take(ProjectThing.MaxActivityCount).Select(c => new Takeoff.Models.Data.ActionSource
//                                                                           {
//                                                                               Id = c.Id,
//                                                                               Action = c.Action,
//                                                                               Data = c.Data,
//                                                                               Date = c.Date,
//                                                                               Description = c.Description,
//                                                                               ThingId = c.ThingId,
//                                                                               ThingParentId = c.ThingParentId,
//                                                                               ThingType = c.ThingType,
//                                                                               UserId = c.UserId,
//                                                                           }).ToArray();
//                var change = changesForProduction.FirstOrDefault(); //they are sorted by ID desc
//                if (change != null)
//                {
//                    Debug.Assert(changesForProduction.First().Id == changesForProduction.Max(c => c.Id));
//                    production.LastChangeDate = change.Date;
//                    production.LastChangeId = change.Id;
//                }
//            }
//        }

//        private  static void CompleteUserTrees(List<UserThing> users, Dictionary<int, ThingBase> thingsPerId  )
//        {
//            foreach( var thing in thingsPerId.Values.Where(t => t is MembershipThing))
//            {
//                var membership = thing.CastTo<MembershipThing>();
//                if ( thingsPerId.ContainsKey(membership.UserId))
//                {
//                    var user = thingsPerId[membership.UserId].CastTo<UserThing>();
//                    user.EntityMemberships[membership.ContainerId.Value] = membership.CastTo<IMembership>();

//                    if (membership is AccountMembershipThing)
//                    {
//                        user.AccountMemberships[
//                            membership.CastTo<AccountMembershipThing>().AccountId] =
//                            membership.CastTo<AccountMembershipThing>();
//                    }
//                }
//            }
//        }

//        #region Data            



//        private static void CreateDataFillers(List<IThingAuxDataFiller> fillers, Dictionary<int, ThingBase> things, List<ActionSourceFromData> changes, DataModel db)
//        {

//            DataFillers.Query(
//                from thing in db.Things
//                where thing.DeletedOn == null
//                select thing
//                ).Fill(data =>
//                           {
//                               Type type;
//                               try
//                               {
//                                   type = Things.ThingType(data.Type);
//                               }
//                               catch( Exception ex)
//                               {
//                                   throw new Exception(string.Format("Thing type '{0}' not found.", data.Type.CharsOr("[null]")));
//                               }
//                               if (type != null)
//                               {
//                                   var thing = Activator.CreateInstance(type).CastTo<ThingBase>();
//                                   thing.FillPropertiesWithRecord(data);
//                                   things.Add(data.Id, thing);
//                               }
//                           }).AddTo(fillers);


//            DataFillers.Query(
//                from data in db.AccountMemberships
//                select data
//                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

//            DataFillers.Query(
//                from data in db.Accounts
//                select data
//                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

//            (from data in db.Comments
//                select data
//                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

//            (from data in db.FeatureAccesses
//                select data
//                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

//            DataFillers.Query(
//                from data in db.Files
//                select data
//                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

//            DataFillers.Query(
//                from data in db.Images
//                select data
//                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

//            DataFillers.Query(
//                from data in db.MembershipRequests
//                select data
//                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

//            DataFillers.Query(
//                from data in db.Memberships
//                select data
//                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);


//                    DataFillers.Query(
//                from data in db.Projects
//                select data
//                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

//                (
//                from data in db.Settings
//                select data
//                ).Fill(data =>
//                           {
//                               FillThingData(data.ThingId, data, things);
//                               if (things.ContainsKey(data.ThingId))
//                               {
//                                   var setting = things[data.ThingId].CastTo<SettingThing>();
//                                   setting.SetValueFromData(data.Value);
//                               }
//                           }).AddTo(fillers);

//                            DataFillers.Query(
//                from data in db.Users
//                select data
//                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

//            DataFillers.Query(
//                from data in db.VideoComments
//                select data
//                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

//                    DataFillers.Query(
//                from data in db.VideoStreams
//                select data
//                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

//                            DataFillers.Query(
//                from data in db.VideoThumbnails
//                select data
//                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

//                                DataFillers.Query(
//                from data in db.Videos
//                select data
//                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);


//                                DataFillers.Query(
//                from data in db.ViewPrompts
//                select data
//                ).Fill(data =>
//                           { 
//                               FillThingData(data.ThingId, data, things); 
//                           }).AddTo(fillers);

//            DataFillers.Query(
//                from change in db.Actions
//                join project in db.Projects on change.ThingId equals project.ThingId
//                join productionThing in db.Things on project.ThingId equals productionThing.Id
//                join changeSource in db.ActionSources on change.ChangeDetailsId equals changeSource.Id
//                where productionThing.DeletedOn == null
//                orderby changeSource.Id descending
//                select new ActionSourceFromData
//                           {
//                               Action = changeSource.Action,
//                               Data = changeSource.Data,
//                               Date = changeSource.Date,
//                               Description = changeSource.Description,
//                               Id = changeSource.Id,
//                               ProductionId = project.ThingId,
//                               ThingId = changeSource.ThingId,
//                               ThingParentId = changeSource.ThingParentId,
//                               ThingType = changeSource.ThingType,
//                               UserId = changeSource.UserId
//                           }
//                ).Fill(data =>
//                {
//                    data.AddTo(changes);
//                }).AddTo(fillers);


//                        //        DataFillers.Query<ActionSourceFromData>(
//                        //"SELECT [t2].[ThingId] as ProductionId, [t1].[Id], [t1].[Date], [t1].[UserId], [t1].[ThingId], [t1].[ThingType], [t1].[ThingParentId], " +
//                        //"[t1].[Action], [t1].[Data], [t0].ThingId as ProductionId FROM [dbo].[Change] AS [t0] " +
//                        //"INNER JOIN [dbo].[ChangeSource] AS [t1] ON [t0].[ChangeDetailsId] = [t1].[Id] " +
//                        //"INNER JOIN [dbo].[Project] AS [t2] ON [t0].[ThingId] = [t2].[ThingId] " +
//                        //"INNER JOIN [dbo].[Thing] AS [t3] ON [t0].[ThingId] = [t3].[Id] " +
//                        //"WHERE [t3].[DeletedOn] is NULL  " +
//                        //"ORDER BY [t0].[Id] DESC"
//                        //).Fill(data =>
//                        //{
//                        //    data.AddTo(changes);
//                        //}).AddTo(fillers);



//                                (  
//                                        from account in db.Accounts      
//                                        join accountThing in db.Things on account.ThingId equals accountThing.Id
//                                        where accountThing.DeletedOn == null
//                                        select new AccountComputedData
//                                        {
//                                            AccountId = accountThing.Id,
//                                            VideoCount = (from vt in db.Things
//                                                        join v in db.Videos on vt.Id equals v.ThingId
//                                                          where vt.AccountId == accountThing.Id && vt.DeletedOn == null && v.Duration != null && (v.IsSample == null || !(bool)v.IsSample)
//                                                        select vt).Count(),
//                                            VideoCountBillable = (from vt in db.Things
//                                                                        join v in db.Videos on vt.Id equals v.ThingId
//                                                                  where vt.AccountId == accountThing.Id && vt.DeletedOn == null && v.Duration != null && (v.IsComplimentary == null || !(bool)v.IsComplimentary)
//                                                                        select vt).Count(),
//                                            VideosAddedInBillingCycle = (from vt in db.Things
//                                                                        join v in db.Videos on vt.Id equals v.ThingId
//                                                                        join a in db.Accounts on vt.AccountId equals a.ThingId
//                                                                         where vt.AccountId == accountThing.Id && a.CurrentBillingPeriodStartedOn != null && v.Duration != null && vt.CreatedOn >= (DateTime)a.CurrentBillingPeriodStartedOn && (v.IsComplimentary == null || !(bool)v.IsComplimentary)//includes deleted ones as well
//                                                                        select vt).Count(),
//                                            ProductionCount = (from t in db.Things
//                                                                join p in db.Projects on t.Id equals p.ThingId
//                                                               where t.AccountId == accountThing.Id && t.DeletedOn == null && (p.IsSample == null || !(bool)p.IsSample)
//                                                                select t).Count(),
//                                            ProductionCountBillable = (from t in db.Things
//                                                                                join p in db.Projects on t.Id equals p.ThingId
//                                                                               where t.AccountId == accountThing.Id && t.DeletedOn == null && (p.IsComplimentary == null || !(bool)p.IsComplimentary)
//                                                                                select t).Count(),
//                                            AssetBytesTotal = (from ft in db.Things
//                                                               join f in db.Files on ft.Id equals f.ThingId
//                                                               join p in db.Projects on ft.ParentId equals p.ThingId//needed because source video files are not included in assets.  Assets are children of production, where source videos are children of videos.  this fixes that, as ghetto as it may be
//                                                               where ft.AccountId == accountThing.Id && ft.DeletedOn == null && f.Bytes.HasValue && (f.IsSample == null || !(bool)f.IsSample)
//                                                               select (long?)f.Bytes).Sum().GetValueOrDefault(),
//                                            AssetBytesTotalBillable = (from ft in db.Things
//                                                               join f in db.Files on ft.Id equals f.ThingId
//                                                               join p in db.Projects on ft.ParentId equals p.ThingId//needed because source video files are not included in assets.  Assets are children of production, where source videos are children of videos.  this fixes that, as ghetto as it may be
//                                                               where ft.AccountId == accountThing.Id && ft.DeletedOn == null && f.Bytes.HasValue && (f.IsComplimentary == null || !(bool)f.IsComplimentary)
//                                                               select (long?)f.Bytes).Sum().GetValueOrDefault(),

//                                            AssetFilesCount = (from ft in db.Things
//                                                                join f in db.Files on ft.Id equals f.ThingId
//                                                               where ft.AccountId == accountThing.Id && ft.DeletedOn == null && f.Bytes.HasValue && (f.IsSample == null || !(bool)f.IsSample)
//                                                                select f).Count(),
//                                            AssetFilesAllTimeCount = (from ft in db.FileUploadLogs where ft.AccountId == accountThing.Id && ft.FileThingType == Things.ThingType(typeof(FileThing)) select ft).Count()
//                                        }).Fill(data =>
//                                                    {
//                                                        var accountThing = things[data.AccountId].CastTo<AccountThing>();
//                                                        accountThing.VideoCount = data.VideoCount;
//                                                        accountThing.VideoCountBillable = data.VideoCountBillable;
//                                                        accountThing.VideosAddedInBillingCycle = data.VideosAddedInBillingCycle;
//                                                        accountThing.ProductionCount = data.ProductionCount;
//                                                        accountThing.ProductionCountBillable = data.ProductionCountBillable;
//                                                        accountThing.AssetBytesTotal = data.AssetBytesTotal;
//                                                        accountThing.AssetsTotalSizeBillable = new FileSize(data.AssetBytesTotalBillable);
//                                                        accountThing.AssetFilesCount = data.AssetFilesCount;
//                                                        accountThing.AssetFilesAllTimeCount = data.AssetFilesAllTimeCount;

//                                                    }).AddTo(fillers);

//        }

   

//        private static void FillThingData(int id, object data, Dictionary<int, ThingBase> things)
//        {
//            ThingBase thing;
//            if (things.TryGetValue(id, out thing))
//            {
//                thing.FillPropertiesWithRecord(data);
//            }
//        }

//        #endregion

//    }

//}
