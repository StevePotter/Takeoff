using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Security;
using Takeoff.Data;
using Takeoff.Models;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Collections;
using System.Data.Linq;
using System.Dynamic;
using Data = Takeoff.Models.Data;
using System.Runtime.Serialization;
using StackExchange.Profiling;

namespace Takeoff.Models
{


    /// <summary>
    /// Provides support for working with Things.
    /// </summary>
    public static class Things
    {

        #region Retrieval

        /// <summary>
        /// Gets the key used in thread and distributed cache to get the given thing.
        /// </summary>
        /// <param name="thingId"></param>
        /// <returns></returns>
        public static string GetCacheKey(int thingId)
        {
            var cacheKey = "t." + thingId.ToInvariant();
            return cacheKey;
        }

        /// <summary>
        /// Gets the fully-constructed entity with the ID and type passed.
        /// </summary>
        /// <param name="thingId"></param>
        /// <param name="entityType">The entity type to match.  All calls to this that originate from user requests should include this parameter to avoid tampering (they delete a Production instead of a File)</param>
        /// <returns></returns>
        private static ThingBase Get(int thingId, Type entityType, bool throwExIfNotExists)
        {
            var things = Get(new int[] { thingId }, throwExIfNotExists);
            if (things.Length == 0)
                return null;

            var thing = things[0];
            if (entityType != null)
                EnsureType(thing, entityType);

            return thing;
        }


        /// <summary>
        /// Gets a bunch of things at once, which is more efficient than individually.
        /// </summary>
        /// <param name="thingIds">The IDs of the various things to get.  All ids must be positive.  There should be no duplicates.</param>
        /// <param name="throwOnProblems">If true, an exception will be thrown if one of the things doesn't exist or is malformed.</param>
        /// <returns></returns>
        /// <remarks>TODO: put a limit on the max number of things at once</remarks>
        private static ThingBase[] Get(int[] thingIds, bool throwOnProblems)
        {
            var profiler = MiniProfiler.Current;
            return GetOld(thingIds, throwOnProblems, MiniProfiler.Current);
        }

        //public static ThingBase[] Get(int[] thingIds, bool throwOnProblems, bool addStepToProfiler)
        //{
        //    if (addStepToProfiler)
        //    {
        //        var profiler = MiniProfiler.Current;
        //        using (profiler.Step("Getting " + thingIds.Length + " Things"))
        //        {
        //            return GetPrivate(thingIds, throwOnProblems, profiler);
        //        }
        //    }
        //    else
        //    {
        //        return GetPrivate(thingIds, throwOnProblems, null);
        //    }

        //}

        private static string GetIdList(int[] thingIds)
        {
            if (thingIds == null || thingIds.Length == 0)
                return "No Things";
            else if (thingIds.Length == 1)
                return "Thing " + thingIds[0].ToInvariant();
            else if (thingIds.Length <= 4)
                return "Things " + string.Join(",", thingIds.Select(i => i.ToInvariant()).ToArray());
            else
                return thingIds.Length.ToInvariant() + " Things";
        }

        /// <summary>
        /// Gets a bunch of things at once, which is more efficient than individually.
        /// </summary>
        /// <param name="thingIds">The IDs of the various things to get.  All ids must be positive.  There should be no duplicates.</param>
        /// <param name="throwOnProblems">If true, an exception will be thrown if one of the things doesn't exist or is malformed.</param>
        /// <returns></returns>
        /// <remarks>TODO: put a limit on the max number of things at once</remarks>
        [Obsolete]
        public static ThingBase[] GetOld(int[] thingIds, bool throwOnProblems, MiniProfiler profiler)
        {
            if (thingIds == null)
                return null;
            if (thingIds.Length == 0)
                return new ThingBase[] { };

            var isSingle = thingIds.Length == 1;
            List<ThingBase> result = new List<ThingBase>(thingIds.Length);

            using (profiler.Step("Get " + GetIdList(thingIds)))
            {
                var ids = new HashSet<int>(thingIds);

                //we want to return the things in teh same ID order that was passed.  results is filled as items are encountered, so it won't necessarily be in that order.  this fixes that.            
                var getResults = new Func<ThingBase[]>(() =>
                    {
                        if (result.Count > 1)
                        {
                            var sortedResults = new List<ThingBase>(result.Count);
                            var byId = result.ToDictionary(t => t.Id);
                            foreach (var id in thingIds)
                            {
                                ThingBase thing;
                                if (byId.TryGetValue(id, out thing))
                                    sortedResults.Add(thing);
                            }
                            return sortedResults.ToArray();
                        }
                        return result.ToArray();
                    });

                using (var timing = (Timing)profiler.Step(""))
                {
                    List<int> idsInThreadCache = timing == null ? null : new List<int>();

                    //1.  check thread cache
                    ids.RemoveWhere((id) =>
                    {
                        if (!id.IsPositive())
                            throw new ArgumentException("ID was not positive");
                        var thing = (ThingBase)CacheUtil.GetFromContext(GetCacheKey(id));
                        if (thing == null)
                        {
                            return false;
                        }
                        else
                        {
                            if (profiler != null)
                                idsInThreadCache.Add(id);
                            result.Add(thing);
                            return true;
                        }
                    });

                    if (timing != null && idsInThreadCache.HasItems())
                    {
                        timing.Name = "Get " + GetIdList(thingIds) + " from thread cache";
                    }
                }

                if (ids.Count == 0)
                {
                    return getResults();
                }
                //2.  check app cache 
                var fromApp = CacheUtil.GetValuesFromAppCache(ids.Select(id => GetCacheKey(id)));
                if (fromApp != null && fromApp.Count() > 0)
                {
                    using (profiler.Step("Got " + GetIdList(thingIds) + " from Memcached"))
                    {
                        foreach (var cachedThing in fromApp)
                        {
                            var thing = ThingBase.Deserialize(cachedThing);
                            result.Add(thing);
                            ids.Remove(thing.Id);
                            CacheUtil.SetInContext(GetCacheKey(thing.Id), thing);
                        }
                    }
                }
                if (ids.Count == 0)
                    return getResults();

                //3.  from db.  if this entity is a container, we build the tree from the DB.  
                using (profiler.Step("Getting " + GetIdList(thingIds) + " from DB"))
                {
                    GetThingsFromDb(throwOnProblems, result, ids);
                }
                if (throwOnProblems && ids.Count > 0)
                {
                    throw new ThingNotFoundException("Thing(s) " + string.Join(",", ids.ToArray()) + " could not be found.");
                }

                return getResults();
            }
        }

        private static void GetThingsFromDb(bool throwOnProblems, List<ThingBase> result, HashSet<int> ids)
        {

            //If one of the things is a container, we build its thing tree.  It it isn't a container, we check for its container in cache.  If the container isn't in cache, we build the thing's tree from db
            using (var db = DataModel.ReadOnly)
            {
                var idArray = ids.ToArray();
                //for containers that are requested, this will give the container and its descendants
                //for non-containers that are requested, only the single record will be returned.
                //we couldn't use standard LINQ because it broke through the 2100 parameter limit a few times
                var idString = string.Join(",", idArray.Select(c => c.ToInvariant()).ToArray());
                var query = db.SimpleSelectQuery(typeof(Data.Thing), "([Id] IN (" + idString + ") OR ([IsContainer] = 0 AND [ContainerId] IN (" + idString + "))) AND [DeletedOn] IS NULL");
                var thingDatasById = db.ExecuteQuery<Data.Thing>(query).ToDictionary(d => d.Id);

                //go through the result records that have been requested and split into containers and noncontainers
                List<Data.Thing> nonContainersData = null;//things that aren't containers (like a Video) that have been requested
                List<Data.Thing> containersData = null;
                foreach (var id in idArray)
                {
                    Data.Thing data;
                    if (thingDatasById.TryGetValue(id, out data))
                    {
                        if (data.IsContainer)
                        {
                            if (containersData == null)
                                containersData = new List<Data.Thing>();
                            containersData.Add(data);
                        }
                        else
                        {
                            var hasContainer = data.ContainerId.HasValue;
                            if (hasContainer)
                            {
                                if (nonContainersData == null)
                                    nonContainersData = new List<Data.Thing>();
                                nonContainersData.Add(data);
                                thingDatasById.Remove(data.Id);//remove from the dictionary so when assembling containers we don't include these
                            }
                            else
                            {
                                //otherwise we just ignore the thing.  in the future you could build the thing's record up anyway but right now the rule is every thingmust have a container or be a container
                                if (throwOnProblems)
                                    throw new InvalidOperationException("No container ID for thing " + data.Id.ToInvariant());
                            }
                        }
                    }
                }

                //if non-containers were requested, this will get their instance by retrieving their container (hopefully from cache)
                if (nonContainersData != null)
                {
                    //look up all the containers at once
                    var containersById = Get(nonContainersData.Select(d => d.ContainerId.Value).ToArray(), throwOnProblems).ToDictionary(t => t.Id);
                    foreach (var nonContainer in nonContainersData)
                    {
                        ThingBase container;
                        if (containersById.TryGetValue(nonContainer.ContainerId.Value, out container))
                        {
                            var thing = container.FindDescendantById(nonContainer.Id);
                            if (thing == null)
                            {
                                if (throwOnProblems)
                                    throw new ThingNotFoundException("Thing ID " + nonContainer.Id + " could not be found in its container.");
                            }
                            else
                            {
                                CacheUtil.SetInContext(GetCacheKey(thing.Id), thing);//useful for when the same thing is requested twice 
                                ids.Remove(thing.Id);
                                result.Add(thing);
                            }
                        }
                        else
                        {
                            //you could create this thing without a container, but that doesn't conform to the rules of Things so why bother
                            if (throwOnProblems)
                                throw new ThingNotFoundException("Thing ID " + nonContainer.Id + "'s container could not be found.");
                        }
                    }
                }

                if (containersData != null)
                {
                    List<ThingBase> containers = new List<ThingBase>();
                    //at this point any requested containers and their descendants will be within thingDatasById.  noncontainers requested were removed up above
                    var things = thingDatasById.Select(keyvalue =>
                    {
                        var data = keyvalue.Value;
                        ThingBase thing = null;
                        try
                        {
                            thing = Things.CreateInstance<ThingBase>(data.Type);
                        }
                        catch
                        {
                            //if the Type field is wrong, this will throw an exception
                            if (throwOnProblems)
                                throw;
                            else
                                return thing;
                        }
                        thing.FillPropertiesWithRecord(data);
                        var id = thing.Id;
                        if (ids.Contains(id))
                        {
                            Debug.Assert(thing.IsContainer);
                            result.Add(thing);
                            ids.Remove(id);
                        }
                        if (thing.IsContainer)
                            containers.Add(thing);
                        return thing;
                    }).ToArray();
                    var thingsById = things.ToDictionary(t => t.Id);

                    //set up children so we can easily construct thing trees.  also queue the auxillary data commands
                    var dataFillers = new List<Tuple<ThingBase, IThingAuxDataFiller>>();
                    foreach (var thing in things)
                    {
                        var parentId = thing.ParentId;
                        if (parentId.HasValue && parentId.Value > 0)
                        {
                            ThingBase parent;
                            if (thingsById.TryGetValue(parentId.Value, out parent))
                            {
                                //don't use AddChild...it is meant for things that have already had their trees built
                                parent.Children.Add(thing);
                            }
                        }

                        //set up the auxillary data.  if these functions ever require a Parent, you should move this below the BuildTree stuff
                        var fillers = thing.FillAuxillaryData(db);
                        if (fillers != null)
                        {
                            fillers.Each(f =>
                            {
                                dataFillers.Add(new Tuple<ThingBase, IThingAuxDataFiller>(thing, f));
                            });
                        }
                    }

                    //build family trees.  things will now have Parent and cotnainer properties set.
                    foreach (var thing in containers)
                    {
                        thing.BuildTree();
                    }

                    if (dataFillers.Count > 0)
                    {
                        //get the queries, execute, and send them back to the things to fill the data 
                        var queries = new List<object>(dataFillers.Select(d => d.Item2.GetQuery()));
                        var results = db.ExecuteQuery(queries);
                        foreach (var f in dataFillers)
                        {
                            f.Item2.Fill(results);
                        }
                    }

                    //we only cache containers
                    foreach (var thing in containers)
                    {
                        thing.AddToCache();
                    }
                }
            }
        }

        /// <summary>
        /// Ensures that the thing passed in is of the expected type or a subclass of it.  Throws an exception if not.
        /// </summary>
        /// <param name="thing"></param>
        /// <param name="expectedType"></param>
        private static void EnsureType(ThingBase thing, Type expectedType)
        {
            if (expectedType == null || thing == null)
                return;

            var thingType = thing.GetType();
            if (!thingType.Equals(expectedType) && !thingType.IsSubclassOf(expectedType))
            {
                throw new ThingNotFoundException("Thing is of wrong type.  Expected " + expectedType.Name + " and got " + thing.GetType().Name);
            }
        }


        /// <summary>
        /// Adds a condition to the input query that will ensure no things that have been deleted will be included in the output.  This results in a join on the Thing table where records with a Thing.DeletedOn value are excluded.
        /// </summary>
        /// <remarks>
        /// This is meant to be used when you are hitting auxillary thing tables (like User or File) directly and want to avoid returning things that have been deleted (well, marked as deleted).  You could write the join yourself, but this is much easier.
        /// 
        /// Note that if you wanted to kill the db parameter, you'd have to do some kind of local thread storage of the current DataModel.  Check out this post: http://www.west-wind.com/weblog/posts/246222.aspx
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="thingIdSelector"></param>
        /// <returns></returns>
        /// <example>
        /// (from mr in db.MembershipRequests where mr.UserId == user.Id select mr).FilterDeletedThings(db,mr=>mr.ThingId)
        /// </example>
        public static IQueryable<T> FilterDeletedThings<T>(this IQueryable<T> input, DataModel db, Expression<Func<T, int>> thingIdSelector)
        {
            return input.Join(db.Things.Where(t => t.DeletedOn == null), thingIdSelector, i => i.Id, (a, b) => a);
        }

        public static IQueryable<int> FilterDeletedThings(this IQueryable<int> input, DataModel db)
        {
            return FilterDeletedThings(input, db, i => i);
        }

        #endregion

        #region Thing Get V2

        public static TThing EnsureExists<TThing>(this TThing thing) where TThing : class
        {
            if (thing == null)
                throw new Exception(string.Format("Data of type {1} couldn't be found.", typeof(TThing).Name));
            return thing;
        }

        public static TThing EnsureExists<TThing>(this TThing thing, int id) where TThing : class
        {
            if (thing == null)
                throw new Exception(string.Format("Data {0} of type {1} couldn't be found.", id, typeof(TThing).Name));
            return thing;
        }



        /// <summary>
        /// Creates and returns the thing, with all its descendants, from cache or database.  If there is no data or the data is malformed, an exception is thrown.
        /// </summary>
        public static ThingBase Get(int id)
        {
            return Get(id, null, true, true, MiniProfiler.Current);
        }

        /// <summary>
        /// Creates and returns the thing, with all its descendants, from cache or database.  If there is no data, null is returned.  If the data is not of the type requested, null is returned as well.
        /// </summary>
        public static ThingBase GetOrNull(int id)
        {
            return Get(id, null, false, false, MiniProfiler.Current);
        }

        /// <summary>
        /// Creates and returns the thing, with all its descendants, from cache or database.  If there is no data, null is returned.  If the data is not of the type requested, null is returned as well.
        /// </summary>
        public static TThing GetOrNull<TThing>(int id) where TThing : ThingBase
        {
            return (TThing)Get(id, typeof(TThing), false, false, MiniProfiler.Current);
        }

        /// <summary>
        /// Creates and returns the thing, with all its descendants, from cache or database.  If there is no data or the data is malformed, an exception is thrown.
        /// </summary>
        public static TThing Get<TThing>(int id) where TThing : ThingBase
        {
            return (TThing)Get(id, typeof(TThing), true, true, MiniProfiler.Current);
        }

        /// <summary>
        /// The main getter for a particular thing.
        /// </summary>
        private static ThingBase Get(int id, Type returnType, bool throwIfMissing, bool throwIfWrong, StackExchange.Profiling.MiniProfiler profiler)
        {
            using (profiler.Step("Get Thing " + (returnType == null ? "[unknown type]" : returnType.FullName) + " " + id.ToInvariant()))
            {
                if (!id.IsPositive())
                {
                    if (throwIfWrong)
                        throw new ArgumentException("Id was not valid.");
                    return null;
                }

                var cacheKey = GetCacheKey(id);

                //thread cache contains only container things
                using (profiler.Step("Checking Thread Cache"))
                {
                    ThingBase fromThreadCache;
                    if (GetThingFromThreadCache(returnType, throwIfWrong, cacheKey, out fromThreadCache)) 
                        return fromThreadCache;
                }

                //distributed cache will have either a full container json, or a string indicating the given thing's container
                using (profiler.Step("Checking Distributed Cache"))
                {
                    ThingBase fromDistributed;
                    if (GetThingFromDistributedCache(id, returnType, throwIfMissing, throwIfWrong, profiler, cacheKey, out fromDistributed)) 
                        return fromDistributed;
                }

                //now we resort to the database.  in this case, we need to determine the thing's type

                if (returnType == null)
                {
                    using (profiler.Step("Determining Thing Type"))
                    {
                        returnType = DetermineThingType(id, throwIfWrong);
                        if (returnType == null) 
                            return null;
                    }
                }
                bool isContainer = typeof(IContainerThing).IsAssignableFrom(returnType);
                if (isContainer)
                {
                    using (profiler.Step("Checking Database"))
                    {
                        return GetContainerThingFromDb(id, returnType, throwIfMissing, false, true);
                    }                    
                }
                else
                {
                    using (profiler.Step("Getting container thing."))
                    {
                        return GetNonContainerThing(id, throwIfMissing, throwIfWrong, profiler);
                    }
                }
            }

        }

        /// <summary>
        /// Returns a non-container thing.  Does it by looking up the container, getting the container, then finding the desired thing within it.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="throwIfMissing"></param>
        /// <param name="throwIfWrong"></param>
        /// <param name="profiler"></param>
        /// <returns></returns>
        private static ThingBase GetNonContainerThing(int id, bool throwIfMissing, bool throwIfWrong, MiniProfiler profiler)
        {
            //get the container ID and type from the db
            using (var db = DataModel.ReadOnly)
            {
                var containerInfo = (from thingData in db.Things
                                     join containerData in db.Things on thingData.ContainerId equals
                                         containerData.Id
                                     where thingData.Id == id
                                     //no need for deletedon checks because we check for existance below
                                     select new
                                                {
                                                    containerData.Id,
                                                    containerData.Type
                                                }
                                    ).FirstOrDefault();
                if (containerInfo == null)
                {
                    if (throwIfMissing)
                        throw new Exception("Missing data");
                    return null;
                }
                //todo: better error checking
                var container = Get(containerInfo.Id, Things.ThingType(containerInfo.Type),
                                     throwIfMissing,
                                     throwIfWrong, profiler);
                if (container == null)
                {
                    if (throwIfMissing)
                        throw new Exception("Missing data");
                    return null;
                }

                var thing = container.FindDescendantById(id);
                if (thing == null)
                {
                    if (throwIfMissing)
                        throw new Exception("Missing data");
                    return null;
                }
                //todo: check type
                return thing;
            }
        }

        private static Type DetermineThingType(int id, bool throwIfWrong)
        {
            Type returnType;
            using (var db = DataModel.ReadOnly)
            {
                var thingInfo = (from thingData in db.Things
                                 where thingData.Id == id && thingData.DeletedOn == null
                                 //no need for deletedon checks because we check for existance below
                                 select new
                                            {
                                                thingData.Type,
                                            }
                                ).FirstOrDefault();
                if (thingInfo == null)
                {
                    if (throwIfWrong)
                        throw new ArgumentException("Data ID " + id + "could not be found");
                    return null;
                }
                returnType = Things.ThingType(thingInfo.Type);
            }
            return returnType;
        }

        private static bool GetThingFromDistributedCache(int id, Type returnType, bool throwIfMissing, bool throwIfWrong, MiniProfiler profiler, string cacheKey, out ThingBase thing)
        {
            thing = null;
            var fromCache = CacheUtil.GetValueFromAppCache(cacheKey);
            if (fromCache != null)
            {
                var asString = fromCache as string;
                //certain descendants get their container information cached.  this allows for a quick lookup wihtout having to access db
                //format is d.{containerid}.  this is set in AddToCacheOverride
                if ( asString != null && asString.StartsWith("d.") )
                {
                    var containerId = asString.After("d.").ToIntTry();
                    if ( containerId.HasValue)
                    {
                        using (var step = profiler.Step("Child in memcached.  Getting container."))
                        {
                            var container = Get(containerId.Value, null, throwIfWrong, throwIfWrong, profiler);
                            if (container != null)
                            {
                                thing = container.FindDescendantById(id);
                                if (thing != null)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    var deserialized = ThingBase.Deserialize(fromCache);
                    var result = deserialized as ThingBase;
                    if (returnType != null && !returnType.IsAssignableFrom(result.GetType()))
                    {
                        if (throwIfWrong)
                            throw new Exception(string.Format("Thing type was wrong.  Expected {0} and got {1}",
                                                              returnType.FullName, fromCache.GetType().FullName));
                        {
                            return true;
                        }
                    }
                    CacheUtil.SetInContext(cacheKey, result); //so next lookup is mega fast
                    {
                        thing = result;
                        return true;
                    }                    
                }
            }
            return false;
        }

        private static bool GetThingFromThreadCache(Type returnType, bool throwIfWrong, string cacheKey, out ThingBase thingBase)
        {
            thingBase = null;
            var fromCache = CacheUtil.GetFromContext(cacheKey);
            if (fromCache != null)
            {
                if (returnType != null && !returnType.IsAssignableFrom(fromCache.GetType()))
                {
                    if (throwIfWrong)
                        throw new Exception(string.Format("Thing type was wrong.  Expected {0} and got {1}",
                                                          returnType.FullName, fromCache.GetType().FullName));
                }
                thingBase = fromCache.CastTo<ThingBase>();                
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets a thing (must be a container) that was potentially deleted.  If it wasn't deleted, it will be returned anyway.
        /// </summary>
        /// <typeparam name="TThing"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static TThing GetDeleted<TThing>(int id) where TThing : ThingBase
        {
            return (TThing)GetContainerThingFromDb(id, typeof(TThing), false, true, false);
        }

        private static ThingBase GetContainerThingFromDb(int id, Type returnType, bool throwIfMissing, bool includeDeleted, bool addToCache)
        {
            var container = Activator.CreateInstance(returnType).CastTo<ThingBase>();
            container.Id = id;
            bool foundData = false;
            var descendants = new List<ThingBase>();
            var things = new Dictionary<int, ThingBase>();

            using (var db = DataModel.ReadOnly)
            {
                var fillers = new List<IThingAuxDataFiller>();

                (from thing in db.Things
                 where (thing.Id == id || thing.ContainerId == id) && (includeDeleted || thing.DeletedOn == null)
                 select thing).Fill(data =>
                                        {
                                            Type type;
                                            try
                                            {
                                                type = Things.ThingType(data.Type);
                                            }
                                            catch (Exception ex)
                                            {
                                                throw new Exception(string.Format(
                                                    "Thing type '{0}' not found.", data.Type.CharsOr("[null]")));
                                            }
                                            if (type != null)
                                            {
                                                ThingBase thing;
                                                if (data.Id == id) //check the container for proper type
                                                {
                                                    foundData = true;
                                                    thing = container;
                                                    //if (container == null)
                                                    //{
                                                    //    if (throwIfWrong)
                                                    //        throw new Exception(string.Format("Thing type was wrong.  Expected {0} and got {1}",
                                                    //                                          returnType.FullName, thing.GetType().FullName));
                                                    //    //return null;
                                                    //}
                                                }
                                                else
                                                {
                                                    thing = Activator.CreateInstance(type).CastTo<ThingBase>();
                                                    descendants.Add(thing);
                                                }
                                                thing.FillPropertiesWithRecord(data);
                                                things.Add(data.Id, thing);
                                            }
                                        }
                    ).AddTo(fillers);
                container.AddDataFillers(fillers, things, db);

                var queries = new List<object>(fillers.Select(d => d.GetQuery()));
                var results = db.ExecuteQuery(queries);
                foreach (var f in fillers)
                {
                    f.Fill(results);
                }
            }

            AssembleThingTrees(container, descendants, things);
            container.BuildTree();
            if (foundData)
            {
                if ( addToCache)
                    container.AddToCache();
                return container;
            }
            else
            {
                if (throwIfMissing)
                    throw new Exception(string.Format("{0} with ID {1} could not be found.", returnType.FullName,
                                                      id));
                return null;
            }
        }

        private static void AssembleThingTrees(ThingBase container, List<ThingBase> descendants, Dictionary<int, ThingBase> thingsPerId)
        {
            //assemble thing trees
            var thingsPerParentId = descendants.ToLookup(t => t.ParentId);
            foreach (var parentIdAndChildren in thingsPerParentId)
            {
                if (parentIdAndChildren.Key.GetValueOrDefault() == 0)
                    continue;
                ThingBase parent;
                if (thingsPerId.TryGetValue(parentIdAndChildren.Key.Value, out parent))
                {
                    foreach (var child in parentIdAndChildren)
                    {
                        if (child.IsContainer)
                        {

                        }
                        else
                        {
                            parent.Children.Add(child);

                        }
                    }
                }
            }

            container.Descendants().Each(d =>
            {
                if (!d.ContainerId.HasValue)
                    d.ContainerId = container.Id;
            });
        }


        #endregion

        #region Type Mapping

        /// <summary>
        /// Returns the "db" type of the thing from the ThingType attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string ThingType(Type thingType)
        {
            return thingTypesPerClass[thingType.Name];
        }

        /// <summary>
        /// Returns the CLR thing type from the "db" type.
        /// </summary>
        public static Type ThingType(string thingType)
        {
            return thingTypesPerName[thingType];
        }

        /// <summary>
        /// Call once when app is starting to perform some initial required reflection.
        /// </summary>
        public static void Init()
        {
            if (thingTypesPerName != null)
                return;

            thingTypesPerName = new Dictionary<string, Type>();
            thingTypesPerClass = new Dictionary<string, string>();

            SurrogateSelector = new SurrogateSelector();

            typeof(ThingBase).Assembly.GetTypes().Each((type) =>
                {
                    var thingNameAtt = type.GetCustomAttributes(typeof(ThingTypeAttribute), true).FirstOrDefault() as ThingTypeAttribute;
                    if (thingNameAtt != null)
                    {
                        thingTypesPerName[thingNameAtt.Type] = type;
                        thingTypesPerClass[type.Name] = thingNameAtt.Type;
                    }

                    if (typeof(ThingBase).IsAssignableFrom(type))
                    {
                        //this is crucial.  before, if a class didn't have an explicit static constructure but had thing props that use Register(), the properties weren't registering in time, which resulted in missing properties from serialization and whatnot.
                        //this only happened in release mode without a debugger running.  so if you think you wanna change this, you better test it without debugger
                        foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        {
                            if (typeof(ThingProperty).IsAssignableFrom(field.FieldType))
                            {
                                field.GetValue(null);//touches the field, causing the implicit static constructor to run
                            }
                        }
                    }

                });

            //needed for binary serialization
            thingTypesPerName.Values.Each(type =>
                {
                    SurrogateSelector.AddSurrogate(type, new StreamingContext(StreamingContextStates.All), new ThingSerializationSurrogate());
                    Activator.CreateInstance(type);//forces all thing properties to register now instead of when they are possibly deserialized (which resulted in an error)
                });

        }

        public static SurrogateSelector SurrogateSelector { get; private set; }

        /// <summary>
        /// Stores the CLR types of Thing objects based on the name in the DB (from EntityTypeAttribute)
        /// </summary>
        private static Dictionary<string, Type> thingTypesPerName;

        /// <summary>
        /// Opposite of thingTypesPerName, this stores a list of DB thing names (from EntityTypeAttribute) based on a CLR type (using FullName).
        /// </summary>
        private static Dictionary<string, string> thingTypesPerClass;

        #endregion

        #region Creation

        /// <summary>
        /// Creates a Thing instance from the type specified.  Does not add to database.
        /// </summary>
        /// <typeparam name="TThing"></typeparam>
        /// <param name="thingType"></param>
        /// <returns></returns>
        public static TThing CreateInstance<TThing>(string thingType) where TThing : ThingBase
        {
            var type = ThingType(thingType);
            return (TThing)Activator.CreateInstance(type);
        }

        #endregion

        #region Undelete


        public static void Undelete(int[] thingIds, TimeSpan? deletedOnPadding = null)
        {
            using (var db = new DataModel())
            {
                //update data then cache since certain things can rely on each other, such as User requiring Account be there so User.AccountMemberships will be populated
                List<int> undeletedIds = new List<int>(thingIds.Length);
                foreach (var thingId in thingIds)
                {
                    var thing = (from t in db.Things where t.Id == thingId select t).FirstOrDefault();
                    if ( thing == null)//permanently deleted
                        continue;
                    if ( !thing.DeletedOn.HasValue)
                        continue;
                    if (!deletedOnPadding.HasValue)
                        deletedOnPadding = TimeSpan.FromSeconds(5);//basically a guess because DeletedOn was sometimes different for batch deletions...ugg
                    var minDeleted = thing.DeletedOn.Value.Subtract(deletedOnPadding.Value);
                    var maxDeleted = thing.DeletedOn.Value.Add(deletedOnPadding.Value);
                    thing.DeletedOn = null;
                    undeletedIds.Add(thingId);
                    if ( thing.IsContainer)
                    {
                        var deletedDescendants = (from t in db.Things where t.ContainerId == thing.Id && t.DeletedOn != null && t.DeletedOn >= minDeleted && t.DeletedOn <= maxDeleted select t);
                        foreach (var descendant in deletedDescendants)
                        {
                            descendant.DeletedOn = null;
                        }
                    }
                    else
                    {
                        //todo: recursiely clear DeletedOn, then update container thing in cache
                        throw new NotImplementedException();
                    }                    
                }
                if ( undeletedIds.HasItems())
                {
                    db.SubmitChanges();
                    foreach (var id in undeletedIds)
                    {
                        var thing = GetOrNull(id);
                        thing.RemoveFromCache();
                        thing = GetOrNull(id);
//                        Get(id);//will add to cache, which was important for things like user models who store email in memcache
                        
                    }
                }

            }
        }



        #endregion

        #region Membership


        /// <summary>
        /// Gets a list of the Ids for all users that have membership on the entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static IEnumerable<int> GetMembers(this ThingBase thing)
        {
            Things.ValidateAsArg(thing);
            if (!thing.IsContainer)
                thing = thing.Container;

            return thing.Children.OfType<MembershipThing>().Select(m => m.UserId);
        }


        /// <summary>
        /// Adds the user with the given ID as a member to the entity.  An extra parameter is provided to avoid an extra db hit if you already know they aren't a member.
        /// No change notifications or emails are sent out by this.  It is assumed the user already has a role on the account. 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="entityType"></param>
        /// <param name="thingId"></param>
        public static ThingBase AddMember(this ThingBase thing, IUser newMember, int addedBy)
        {
            thing.ValidateAsArg();
            if (!thing.IsContainer)
                thing = thing.Container;

            if (newMember.IsMemberOf(thing))
                throw new ArgumentException("User is already a member of this thing.");

            var membership = thing.AddChild(new MembershipThing
            {
                CreatedByUserId = addedBy,
                CreatedOn = DateTime.UtcNow,
                UserId = newMember.Id,
                TargetId = thing.Id,
            }).Insert();

            //create a copy of the membership underneath the user
            //newMember.AddChild(membership.CreateLinkedThing<MembershipThing>()).Insert();
            return membership;

        }




        #endregion

        /// <summary>
        /// Used for argument validation on methods that require the entity Id and Type.  Assumes the entity parameter passed is called "thing".
        /// </summary>
        /// <param name="entity"></param>
        public static void ValidateAsArg(this ThingBase thing)
        {
            ValidateAsArg(thing, "thing");
        }

        /// <summary>
        /// Used for argument validation on methods that require the entity Id and Type.
        /// </summary>
        /// <param name="entity"></param>
        public static void ValidateAsArg(this ThingBase thing, string thingArgName)
        {
            Args.NotNull(thing, thingArgName);
            Args.GreaterThanZero(thing.Id, thingArgName + ".Id");
            Args.HasChars(thing.Type, thingArgName + ".Type");
        }

        /// <summary>
        /// Inserts database records for this thing and all its descendants.  Properties like ID, ParentId, etc will all be set after this.  You should only call this once on the top-level thing you are inserting.
        /// </summary>
        /// <remarks>
        /// A CommandBatcher is used to avoid a ton of separate insert commands.  This is a huge performance benefit (cuts new account creation from 3+ seconds to way less than one).
        /// 
        /// Note: this is only an extension method so it can be chained with strong typing.
        /// </remarks>
        public static TThing Insert<TThing>(this TThing thing) where TThing : ThingBase
        {
            using (var batcher = new CommandBatcher())
            {
                thing.QueueInsert(batcher);
                batcher.Execute();
            }
            return thing;
        }


        /// <summary>
        /// Sets it up so when the command batcher is executed, this thing's records are all created.
        /// </summary>
        /// <typeparam name="TThing"></typeparam>
        /// <param name="thing"></param>
        /// <param name="batcher"></param>
        /// <returns></returns>
        /// <remarks>Todo: provide a way to specify substitute params.  see Accounts.Create for what would then become possible.</remarks>
        public static TThing QueueInsert<TThing>(this TThing thing, CommandBatcher batcher, List<SubstituteParameter> substituteParams = null) where TThing : ThingBase
        {
            if (thing.CreatedOn == default(DateTime))
                thing.CreatedOn = DateTime.UtcNow;
            thing.QueueInsertData(batcher, substituteParams);
            batcher.Executed += (o, e) =>
                {
                    thing.RemoveFromCache();
                };
            return thing;
        }


    }

    /// <summary>
    /// Decorate entity extension classes with this to get the Type field for the Thing record associated with it.
    /// </summary>
    public class ThingTypeAttribute : System.Attribute
    {
        public string Type
        {
            get;
            set;
        }

        public ThingTypeAttribute(string type)
        {
            Type = type;
        }
    }

    /// <summary>
    /// I wanted to make data access code for Things easy.  A container shouldn't have to execute a massive hand-written error prone query to fill its Thing tree.  So a more automatic way was in order.
    /// When a Thing is requested from the database, a query is done on the Thing table to get all Things within the target container's tree.  The Thing tree is then constructed with strongly-typed Things.
    /// Next the auxillary data needs to be filled.  Auxillary data is held in tables, each table representing a set of properties attached to a certain type of thing.  Things can have 0-x auxillary tables, although they usually have one.
    /// The table convention is a primary key called ThingId that will point to the target thing's Id.  There should only be one entry in an auxillary table for any given Thing.  
    /// 
    /// So, in order to do this, you have to get data for every thing in the tree.  LINQ doesn't support batch select queries, so to build a Thing tree would require a database hit for every thing!  This would get pretty insane for larger thigns (hundreds of Things in a tree).
    /// I found a way to batch queries.  It's two steps.  First, collect the queries.  Then, after the combined query has been executed, the data needs to be given back to each Thing.  Hence this.
    /// </summary>
    public interface IThingAuxDataFiller
    {
        object GetQuery();

        void Fill(IMultipleResults queryResult);
    }

    public class ThingAuxDataFiller<TResults> : IThingAuxDataFiller
    {
        public ThingAuxDataFiller(IQueryable<TResults> query)
        {
            Query = query;
        }

        public ThingAuxDataFiller(IQueryable query, Action<TResults> fillEach)
        {
            Query = query;
            FillEachCallback = fillEach;
        }

        public ThingAuxDataFiller(IQueryable query, Action<IEnumerable<TResults>> fill)
        {
            Query = query;
            FillCallback = fill;
        }


        public ThingAuxDataFiller(DataContext db, int keyEquals, Action<IEnumerable<TResults>> fill)
        {
            QueryText = db.SimpleSelectQuery(typeof(TResults), keyEquals);
            FillCallback = fill;
        }
        public ThingAuxDataFiller(DataContext db, int keyEquals, Action<TResults> fillEach)
        {
            QueryText = db.SimpleSelectQuery(typeof(TResults), keyEquals);
            FillEachCallback = fillEach;
        }

        public ThingAuxDataFiller(string query)
        {
            QueryText = query;
        }

        public ThingAuxDataFiller(string query, Action<IEnumerable<TResults>> fill)
        {
            QueryText = query;
            FillCallback = fill;
        }

        public ThingAuxDataFiller(string query, Action<TResults> fillEach)
        {
            QueryText = query;
            FillEachCallback = fillEach;
        }

        private IQueryable Query;
        private string QueryText;
        public Action<TResults> FillEachCallback;
        public Action<IEnumerable<TResults>> FillCallback;

        #region IThingAuxillaryQuery Members

        object IThingAuxDataFiller.GetQuery()
        {
            if (Query == null)
                return QueryText;

            return (IQueryable)Query;
        }

        void IThingAuxDataFiller.Fill(IMultipleResults resultDataRecord)
        {
            if (FillEachCallback != null)
            {
                resultDataRecord.GetResult<TResults>().Each(d => FillEachCallback(d));
            }
            else if (FillCallback != null)
            {
                FillCallback(resultDataRecord.GetResult<TResults>());
            }
            else
            {
                throw new Exception("You gotta set either FillEachCallback or FillCallback.");
            }
        }

        #endregion
    }

    public static class DataFillers
    {
        public static ThingAuxDataFiller<TResults> Query<TResults>(IQueryable<TResults> query)
        {
            return new ThingAuxDataFiller<TResults>(query);
        }

        public static ThingAuxDataFiller<TResults> Fill<TResults>(this IQueryable<TResults> query, Action<TResults> callback)
        {
            return new ThingAuxDataFiller<TResults>(query).Fill(callback);
        }

        public static ThingAuxDataFiller<TResults> FillAll<TResults>(this IQueryable<TResults> query, Action<IEnumerable<TResults>> callback)
        {
            return new ThingAuxDataFiller<TResults>(query).FillAll(callback);
        }

        public static ThingAuxDataFiller<TResults> Query<TResults>(string query)
        {
            return new ThingAuxDataFiller<TResults>(query);
        }

        public static ThingAuxDataFiller<TResults> Fill<TResults>(this ThingAuxDataFiller<TResults> filler, Action<TResults> callback)
        {
            filler.FillEachCallback = callback;
            return filler;
        }

        public static ThingAuxDataFiller<TResults> FillAll<TResults>(this ThingAuxDataFiller<TResults> filler, Action<IEnumerable<TResults>> callback)
        {
            filler.FillCallback = callback;
            return filler;
        }

        //public static ThingAuxDataFiller<TResults> Fill<TResults>(this ThingAuxDataFiller<TResults> filler, Action<IEnumerable<TResults>> callback)
        //{
        //    filler.FillCallback = callback;
        //    return filler;
        //}

    }

}
