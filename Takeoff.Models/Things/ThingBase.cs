using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Takeoff.Data;
using Takeoff.Models.Data;
using ServiceStack.Text;

namespace Takeoff.Models
{
    
    /// <summary>
    /// A "thing" represents a logical object in the app.  Thing objects know how to interface with the underlying database, as well as create view data for Controller classes.
    /// Using Things makes it easy to:
    /// - Drastically reduce amount of data access code strewn in controllers and DAL helpers.
    /// - Batch data commands to improve system performance.
    /// - Improve app performance by providing highly effective caching methods (stored in Memcached with automatic invalidation)
    /// - Determine whether a user has permission to modify an object.  Things easily hook up to user permissions for concise, reliable permission checks.  Bottom line: safer app
    /// - Work with objects in tree structure rather than database tables.
    /// - Decouple the data quite a bit, which would make it easier to switch data providers.
    /// - Simplify database structure by eliminating redundant columns and providing a more "OOP" style of data schema.  Reduces the number of relationship columns in the database, which is very helpful when refactoring the database.
    /// - Hooks into a simple membership system.
    /// - Reduce fat MVC controllers.
    /// - Support a rich, hige performance change notification system.  Every insert, update, and deletion can be tracked and reported, which makes building things like AJAX interfaces much easier.
    /// - Deletions can be kept in the database for an audit trail.  From the API (Things.Get), they will appear as deleted but they can be queried in the database.
    /// - todo: Allow for tracking of accesses, like every time someone opens a project.
    /// - Provide an easy way to create UI-specific view data objects that can be used to construct server-side and client-side views (they are JSON serializable by convention).
    /// 
    /// Each database table represents a set of properties for a Thing type.  Since things can inherit, you can have, say a ImageThing that inherits from FileThing.  FileThing's db table can store file name.  Then the image table can store maybe the width/height.  This makes the database schema much simpler. These "auxillary" tables attach to their Thing record via the ThingId column (only via convention).
    /// 
    /// Things turn the tables into a tree of strongly-typed Thing objects.  With simple APIs (designed from outside in) 
    /// 
    /// "Container" things are User, Accound, and Production.  These are treated a bit differently:
    /// - only containers are cached.  things inside a container require a single DB hit to retrieve, although that will change
    /// - containers are necessary for permissions.  Permissions typically require a container, such as whether a user can add a Comment for a Video within a Project.
    /// 
    /// By convention, Thing class names end in Thing.
    /// 
    /// Things can get a bit annoying.  It's hard to read the data directly.  Creating Thing classes typically requires plenty of repetitive code (Data<>Thing, Thing<>ViewData)
    /// </summary>
    /// <remarks>
    /// note: BINARY SERIALIZATION IS BUSTED FOR SOME REASON.  All children and thing sub-objects serialize fine but when they deserialize, it's all null objects
    /// todo: consider makign this abstract.  This would require you make the Thing data record a separate deal.  Probably a good idea anywyay.
    /// also, it would be nice to dynamically construct the DB command.  First command gets all the Thing records.  Then the tree gets build.  Then each Thing object adds 1-x queuries for auxillary tables.  Using dynamic linq, you can batch the commands into a dynamic object that can then feed back into each thing.  The result is 2 relaitvely straightforward DB commands per get.
    /// </remarks>
    [Serializable]
    public abstract partial class ThingBase : ITypicalEntity, ISerializable
    {
        #region Constructor

        protected ThingBase()
        {
        }

        protected ThingBase(SerializationInfo info, StreamingContext context)
        {
            Deserialize(info);
        }

        #endregion

        #region Properties

        private static readonly ThingDataProperty<int, ThingBase, Thing> AccountIdProperty =
            new ThingDataProperty<int, ThingBase, Thing>
                {
                    Name = "AccountId",
                    SetField = (o, v) => o._AccountId = v,
                    GetField = o => o._AccountId.GetValueOrDefault(),
                    DefaultValueCallback = o => o.GetDefaultAccountId(),
                    ShouldGetData = o => o.AccountId != null,
                    GetData = o => (int) o.AccountId,
                    //safe to case because of ShouldGetData
                    ShouldSetData = o => o.AccountId.IsPositive(),
                    SetData = (o, v) => o.AccountId = v,
                }.Register();

        private static readonly ThingDataProperty<int, ThingBase, Thing> IdProperty =
            new ThingDataProperty<int, ThingBase, Thing>
                {
                    Name = "Id",
                    SetField = (o, v) => o._Id = v,
                    GetField = o => o._Id,
                    GetData = o => o.Id,
                    SetData = (o, v) => o.Id = v,
                }.Register();


        private static readonly ThingDataProperty<int?, ThingBase, Thing> ContainerIdProperty =
            new ThingDataProperty<int?, ThingBase, Thing>
                {
                    Name = "ContainerId",
                    SetField = (o, v) => o._ContainerId = v,
                    GetField = o => o._ContainerId,
                    DefaultValueCallback = o =>
                                               {
                                                   ThingBase container = o.Container;
                                                   if (container != null && container.Id.IsPositive())
                                                       return o.Container.Id;
                                                   return new int?();
                                               },
                    ShouldGetData = r => r.ContainerId != null,
                    GetData = o => o.ContainerId,
                    SetData = (o, v) => o.ContainerId = v,
                }.Register();


        private static readonly ThingDataProperty<bool, ThingBase, Thing> IsContainerProperty =
            new ThingDataProperty<bool, ThingBase, Thing>
                {
                    Name = "IsContainer",
                    SetField = (o, v) => o._IsContainer = v,
                    GetField = o => o._IsContainer,
                    GetData = o => o.IsContainer,
                    SetData = (o, v) => o.IsContainer = v,
                }.Register();


        private static readonly ThingDataProperty<int, ThingBase, Thing> CreatedByUserIdProperty =
            new ThingDataProperty<int, ThingBase, Thing>
                {
                    Name = "CreatedByUserId",
                    SetField = (o, v) => o._CreatedByUserId = v,
                    GetField = o => o._CreatedByUserId,
                    GetData = o => o.CreatedByUserId,
                    SetData = (o, v) => o.CreatedByUserId = v,
                }.Once(p =>
                           {
                               p.AfterSet += (e, a) =>
                                                 {
                                                     if (!OwnerUserIdProperty.IsSet(a.Owner))
                                                     {
                                                         OwnerUserIdProperty.SetValue(a.Owner, a.NewValue);
                                                     }
                                                 };
                           }).Register();

        private static readonly ThingDataProperty<int, ThingBase, Thing> OwnerUserIdProperty =
            new ThingDataProperty<int, ThingBase, Thing>
                {
                    Name = "OwnerUserId",
                    SetField = (o, v) => o._OwnerUserId = v,
                    GetField = o => o._OwnerUserId,
                    GetData = o => o.OwnerUserId == null ? o.CreatedByUserId : o.OwnerUserId.Value,
                    SetData = (o, v) => o.OwnerUserId = v,
                }.Register();

        private static readonly ThingDataProperty<DateTime, ThingBase, Thing> CreatedOnProperty =
            new ThingDataProperty<DateTime, ThingBase, Thing>
                {
                    Name = "CreatedOn",
                    SetField = (o, v) => o._CreatedOn = v,
                    GetField = o => o._CreatedOn,
                    GetData = o => o.CreatedOn,
                    SetData = (o, v) => o.CreatedOn = v,
                }.Register();

        private static readonly ThingDataProperty<int?, ThingBase, Thing> ParentIdProperty =
            new ThingDataProperty<int?, ThingBase, Thing>
                {
                    Name = "ParentId",
                    SetField = (o, v) => o._ParentId = v,
                    GetField = o => o._ParentId,
                    DefaultValueCallback = o =>
                                               {
                                                   ThingBase parent = o.Parent;
                                                   if (parent != null && parent.Id.IsPositive())
                                                       return parent.Id;
                                                   return new int?();
                                               },
                    ShouldGetData = r => r.ParentId != null,
                    GetData = o => o.ParentId,
                    SetData = (o, v) => o.ParentId = v,
                }.Register();


        private static readonly ThingDataProperty<int?, ThingBase, Thing> LinkedThingsIdProperty =
            new ThingDataProperty<int?, ThingBase, Thing>
            {
                Name = "LinkedThingsId",
                SetField = (o, v) => o._LinkedThingsId = v,
                GetField = o => o._LinkedThingsId,
                ShouldGetData = r => r.LinkedThingsId != null,
                GetData = o => o.LinkedThingsId,
                SetData = (o, v) => o.LinkedThingsId = v,
            }.Register();


        private int? _AccountId;
        private int? _ContainerId;
        private int _CreatedByUserId;
        private DateTime _CreatedOn;
        private int _Id;
        [NonSerialized] private SqlParameter _InsertIdParam;
        private bool _IsContainer;
        private int? _LinkedThingsId;
        private bool? _LogInsertActivity;
        private int _OwnerUserId;
        private int? _ParentId;
        private List<ThingBase> _children;
        [NonSerialized] private Dictionary<int, ThingBase> _descendantsById;
        [NonSerialized] private ThingBase _parent;

        /// <summary>
        /// The parent thing object.  If this is a container thing, it will be null.
        /// </summary>
        public ThingBase Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public List<ThingBase> Children
        {
            get
            {
                if (_children == null)
                    _children = new List<ThingBase>();
                return _children;
            }
            set //only here for serialization
            { _children = value; }
        }

        public bool HasChildren
        {
            get { return _children != null && _children.Count > 0; }
        }

        /// <summary>
        /// Returns the root entity.  If this entity is a container, it will return itself.
        /// </summary>
        public ThingBase Container
        {
            get
            {
                ThingBase climber = this;
                while (climber.Parent != null)
                    climber = climber.Parent;
                return climber;
            }
        }

        /// <summary>
        /// If this thing has a container, this is its ID.  
        /// </summary>
        /// <remarks>
        /// Usually things have a container.  An exception might be a User or Account.  
        /// Note that if a thing is a container (like Project), its container ID is the container above it.  
        /// </remarks>
        public int? ContainerId
        {
            get { return ContainerIdProperty.GetValue(this); }
            set { ContainerIdProperty.SetValue(this, value); }
        }

        /// <summary>
        /// If true, this thing is marked as a container.
        /// </summary>
        /// <remarks>
        /// The difference between containers and non-containers:
        /// - Containers get cached.  Non-containers don't get cached, instead they are found by retrieving their container from cache.
        /// </remarks>
        public bool IsContainer
        {
            get { return IsContainerProperty.GetValue(this); }
            set { IsContainerProperty.SetValue(this, value); }
        }

        /// <summary>
        /// Gets the owner if this thing.
        /// </summary>
        /// <returns></returns>
        public virtual UserThing Owner
        {
            get
            {
                int id = OwnerUserId;
                if (id > 0)
                    return Things.GetOrNull<UserThing>(id);
                return null;
            }
        }

        /// <summary>
        /// If this thing has a parent with a positive ID, this returns that ID.  Otherwise, if there is no parent or the parent's ID is 0, this returns null.
        /// </summary>
        public int? ParentId
        {
            get { return ParentIdProperty.GetValue(this); }
            set { ParentIdProperty.SetValue(this, value); }
        }

        /// <summary>
        /// Sometimes one thing needs to exist in multiple thing trees.  For example, a membership exists underneath the member (UserThing) as well as the thing the user belongs to.
        /// When that happens, multiple things need to get created and then "linked" together.  They are essentially clones of each other.  When one of the things is deleted or updated, the other linked ones will be automatically updated or deleted as well.
        /// </summary>
        /// <remarks>If there are no linked things, this should be null.  However, it shouldn't cause a problem if this property has a value but this is the only linked thing.</remarks>
        public int? LinkedThingsId
        {
            get { return LinkedThingsIdProperty.GetValue(this); }
            set { LinkedThingsIdProperty.SetValue(this, value); }
        }

        /// <summary>
        /// Type name used for storing in DB.
        /// </summary>
        public string Type
        {
            get { return Things.ThingType(GetType()); }
        }

        /// <summary>
        /// Only set during the execution of a db insert command.  This is the parameter that will hold the ID value for this thing.
        /// </summary>
        /// <remarks>Originally I passed the id parameters throughout various QueueInsert methods but the change bubbling made it very tricky and I just said screw it, this is cleaner (yes, one more member but whatever)</remarks>
        public SqlParameter InsertIdParam
        {
            get { return _InsertIdParam; }
            protected set { _InsertIdParam = value; }
        }

        /// <summary>
        /// If true, an activity record will be added when this thing is inserted.
        /// </summary>
        public bool LogInsertActivity
        {
            get { return _LogInsertActivity.GetValueOrDefault(Parent.MapIfNotNull(p => p.LogInsertActivity, true)); }
            set { _LogInsertActivity = value; }
        }

        private Dictionary<int, ThingBase> DescendantsById
        {
            get
            {
                if (_descendantsById == null)
                    _descendantsById = new Dictionary<int, ThingBase>();
                return _descendantsById;
            }
        }

        /// <summary>
        /// The account ID this thing belongs to.  This implementation changes per thing type and the data is not stored in the database.
        /// </summary>
        public int AccountId
        {
            get { return AccountIdProperty.GetValue(this); }
            set { AccountIdProperty.SetValue(this, value); }
        }

        /// <summary>
        /// The unique global ID for this thing.
        /// </summary>
        public int Id
        {
            get { return IdProperty.GetValue(this); }
            set { IdProperty.SetValue(this, value); }
        }

        /// <summary>
        /// The ID of the UserThing that created this.
        /// </summary>
        public int CreatedByUserId
        {
            get { return CreatedByUserIdProperty.GetValue(this); }
            set { CreatedByUserIdProperty.SetValue(this, value); }
        }

        /// <summary>
        /// The ID of the person that currently owns this.  By default, this is the same as the creator.
        /// </summary>
        public int OwnerUserId
        {
            get { return OwnerUserIdProperty.GetValue(this); }
            set { OwnerUserIdProperty.SetValue(this, value); }
        }

        /// <summary>
        /// UTC date when this was created.
        /// </summary>
        public DateTime CreatedOn
        {
            get { return CreatedOnProperty.GetValue(this); }
            set { CreatedOnProperty.SetValue(this, value); }
        }


        public DateTime? DeletedOn
        {
            get { return DeletedOnProperty.GetValue(this); }
            set { DeletedOnProperty.SetValue(this, value); }
        }
        private DateTime? _DeletedOn;

        private static readonly ThingDataProperty<DateTime?, ThingBase, Thing> DeletedOnProperty =
            new ThingDataProperty<DateTime?, ThingBase, Thing>
            {
                Name = "DeletedOn",
                SetField = (o, v) => o._DeletedOn = v,
                GetField = o => o._DeletedOn,
                GetData = o => o.DeletedOn,
                SetData = (o, v) => o.DeletedOn = v,
            }.Register();

        protected virtual int GetDefaultAccountId()
        {
            if (Parent == null)
                return 0;
            return Parent.AccountId;
        }

        #endregion

        #region Get

        /// <summary>
        /// Returns shit to fill this Thing with extra tables from the database.  
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        protected internal virtual IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            yield break;            
        }


        public UserThing GetAuthor()
        {
            return CreatedByUserId > 0 ? Things.GetOrNull<UserThing>(CreatedByUserId) : null;
        }

        public virtual bool IsAuthor(Identity identity)
        {
            if ( identity == null || CreatedByUserId == 0)
                return false;
            var asUser = identity as UserIdentity;
            if (asUser != null)
                return asUser.UserId == CreatedByUserId;
            return false;
        }

        #endregion

        #region Tree Building and Traversal

        /// <summary>
        /// Adds a child to the thing tree underneath this Thing.  Does NOT add any database records.
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="thing"></param>
        /// <returns></returns>
        public ThingBase AddChild(ThingBase thing)
        {
            return AddChild<ThingBase>(thing);
        }


        /// <summary>
        /// Adds a child to the thing tree underneath this Thing.  Does NOT add any database records.  Returns the thing that was added.
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="thing"></param>
        /// <returns></returns>
        public TChild AddChild<TChild>(TChild thing) where TChild : ThingBase
        {
            Children.Add(thing);
            OnChildAdded(thing);

            return thing;
        }

        protected virtual void OnChildAdded(ThingBase child)
        {
            child.Parent = this;
            ThingBase container = IsContainer ? this : Container;
            if (container != null && child.Id > 0)
                container.DescendantsById[child.Id] = child;
        }

        protected virtual void OnChildRemoved(ThingBase child)
        {
        }

        public void BuildTree()
        {
            if (!HasChildren)
                return;

            foreach (ThingBase child in Children)
            {
                OnChildAdded(child);
                child.BuildTree();
            }
        }


        /// <summary>
        /// Returns this thing and all its ancestors.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IEnumerable<ThingBase> ThisAndDescendants()
        {
            yield return this;
            foreach (ThingBase a in Descendants())
                yield return a;
        }

        /// <summary>
        /// Returns all the entities underneath this one in the tree.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IEnumerable<ThingBase> Descendants()
        {
            if (HasChildren)
            {
                foreach (ThingBase child in Children)
                {
                    yield return child;
                    foreach (ThingBase descendant in child.Descendants())
                        yield return descendant;
                }
            }
        }

        /// <summary>
        /// Finds the first descendant using a basic search.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public ThingBase FindDescendant(Predicate<ThingBase> match)
        {
            if (HasChildren)
            {
                foreach (ThingBase child in Children)
                {
                    if (match(child))
                        return child;
                }
                foreach (ThingBase child in Children)
                {
                    ThingBase deepMatch = child.FindDescendant(match);
                    if (deepMatch != null)
                        return deepMatch;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the first direct child that matches the supplied predicate.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public ThingBase FindChild(Predicate<ThingBase> match)
        {
            if (HasChildren)
            {
                foreach (ThingBase child in Children)
                {
                    if (match(child))
                        return child;
                }
            }
            return null;
        }


        /// <summary>
        /// Finds the first direct child that is of the type supplied and matches the given predicate.  Just a bit more convenient than the other FindChild in some cases.
        /// If no predicate is supplied (null), it will just return the first child of that type.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public T FindChild<T>(Predicate<T> match = null) where T : ThingBase
        {
            if (HasChildren)
            {
                foreach (ThingBase child in Children)
                {
                    var asT = child as T;
                    if (asT != null && (match == null || match(asT)))
                        return asT;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a family member thing within this thing's tree by ID.  This uses a fast dictionary lookup.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ThingBase FindDescendantById(int id)
        {
            ThingBase container = IsContainer ? this : Container;
            if (container == null || container._descendantsById == null)
                return null;
            ThingBase thing;
            if (container._descendantsById.TryGetValue(id, out thing))
                return thing;
            return null;
        }

        public IEnumerable<T> ChildrenOfType<T>() where T : ThingBase
        {
            return Children.OfType<T>();
        }

        /// <summary>
        /// Indicates whether there is at least one child with the given type passed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includeSubTypes">If true, children that inherit from teh type will return true.  If false, the type must be the exact one passed.</param>
        /// <returns></returns>
        public bool HasChildOfType<T>(bool includeSubTypes = true) where T : ThingBase
        {
            foreach (ThingBase child in Children)
            {
                if ((includeSubTypes && child is T) || (!includeSubTypes && child.GetType().Equals(typeof (T))))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns all the ancestor entities up to the root level.
        /// </summary>
        public IEnumerable<ThingBase> Ancestors()
        {
            ThingBase climber = Parent;
            while (climber != null)
            {
                yield return climber;
                climber = climber.Parent;
            }
        }


        /// <summary>
        /// Finds the first descendant using a basic search.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public ThingBase FindAncestor(Predicate<ThingBase> match)
        {
            ThingBase climber = Parent;
            while (climber != null)
            {
                if (match(climber))
                    return climber;
                climber = climber.Parent;
            }
            return null;
        }

        public T FindAncestor<T>() where T : ThingBase
        {
            return (T) FindAncestor(t => t is T);
        }

        #endregion

        #region Caching

        /// <summary>
        /// Adds the current entity into the cache.  Does NOT check for concurrency.  This is useful if you insert an entity (which doesn't add to cache because cache is done on demand) and plan on using it immediately.  This method should 
        /// only be called on Root entities and should be called after ALL sub-entities are inserted.
        /// 
        /// Override this if you want to cache an entity according to something besides ID, like a user's email address.
        /// </summary>
        public void AddToCache()
        {
            //we only cache root entities for now
            if (IsContainer)
            {
                AddToCacheOverride();
            }
            else
            {
                Container.AddToCacheOverride();
            }
        }

        protected virtual void AddToCacheOverride()
        {
            string key = Things.GetCacheKey(Id);
            CacheUtil.SetInContext(key, this);


            foreach( var descendant in Descendants())
            {
                CacheUtil.SetAppCacheValue(Things.GetCacheKey(descendant.Id), "d." + Id.ToInvariant());
            }

            string json = SerializeToString(); //serialization is in json
            CacheUtil.SetAppCacheValue(key, json);
        }


        /// <summary>
        /// Removes this entity from the cache.  If this entity isn't a container, RemoveFromCache will be called on its container instead.
        /// 
        /// Call this if you do anything funky that might make the cached version out of sync with the actual one.  Calling the native Thing methods like Delete, Update, and CommitInsert will autmoatically clear the cache.
        /// Override this if you want to cache an entity according to something besides ID, like a user's email address.
        /// </summary>
        public void RemoveFromCache()
        {
            //we only cache root entities for now
            if (IsContainer)
            {
                RemoveFromCacheOverride();
            }
            else
            {
                Container.RemoveFromCacheOverride();
            }
        }

        protected virtual void RemoveFromCacheOverride()
        {
            string key = Things.GetCacheKey(Id);
            CacheUtil.RemoveFromContext(key);
            CacheUtil.RemoveFromAppCache(key);
        }

        #endregion

        #region Views

        public ThingModelView CreateViewData(Identity identity)
        {
            return CreateViewData(null, identity);
        }
        
        public ThingModelView CreateViewData(string viewName, Identity identity)
        {
            ThingModelView view = CreateViewInstance(viewName);
            if (view == null)
                return null;
            FillView(viewName, view, identity);
            return view;
        }

        protected virtual ThingModelView CreateViewInstance(string viewName)
        {
            return null;
        }

        protected virtual void FillView(string viewName, ThingModelView view, Identity identity)
        {
            view.Id = Id;

            var asBasic = view as ThingBasicModelView;
            if (asBasic != null)
            {
                asBasic.CreatedOn = CreatedOn.ForJavascript();
                if (CreatedByUserId > 0)
                {
                    var creator = Things.GetOrNull<UserThing>(CreatedByUserId);
                    if (creator != null)
                    {
                        asBasic.Creator = (UserView) creator.CreateViewData(identity);
                    }
                }
                asBasic.IsCreator = this.IsAuthor(identity);
                var userIdentity = identity as UserIdentity;
                if (userIdentity != null)
                {
                    UserThing user = userIdentity.MapIfNotNull(u => Things.GetOrNull<UserThing>(u.UserId).EnsureExists(u.UserId));
                    asBasic.CanEdit = this.HasPermission(Permissions.Edit, user);
                    asBasic.CanDelete = this.HasPermission(Permissions.Delete, user);
                }
            }
        }

        #endregion

        #region Changes/Activity

        /// <summary>
        /// Gets the contents for the activity email. 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public virtual ChangeDigestEmailItem GetActivityEmailContents(ActionSource change, IUser recipient)
        {
            return null;
        }


        /// <summary>
        /// Gets the information for adding an activity panel entry in the dashboard or production shell.
        /// </summary>
        /// <param name="change"></param>
        /// <param name="withinProduction"> </param>
        /// <param name="identity"> </param>
        /// <returns></returns>
        public virtual ProductionActivityItem GetActivityPanelContents(ActionSource change, bool withinProduction, Identity identity)
        {
            return null;
        }

        #endregion

        #region Linked Things

        /// <summary>
        /// Gets the other things (not included this one) that are linked to this one via the same LinkedThingsId.  
        /// </summary>
        /// <returns></returns>
        public ThingBase[] GetLinkedThings()
        {
            if (!LinkedThingsId.HasValue)
                return null;
            using (DataModel db = DataModel.ReadOnly)
            {
                int[] ids =
                    (from t in db.Things where t.Id != Id && t.LinkedThingsId == LinkedThingsId.Value select t.Id).
                        ToArray();
                if (ids.Length == 0)
                    return null;
                return ids.Select(id => Things.GetOrNull(id)).Where(t => t != null).ToArray();
            }
        }

        /// <summary>
        /// Creates a thing with the basic properties of this thing.  Also sets the LinkedThingsId on it (and this as well if necessary).  Does not insert the new thing anywhere.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks>
        /// todo: include command batcher to use when this.LinkedThingsId is not set.
        /// </remarks>
        public virtual T CreateLinkedThing<T>() where T : ThingBase
        {
            if (!LinkedThingsId.HasValue)
            {
                TrackChanges();
                if (!Id.IsPositive())
                    throw new InvalidOperationException(
                        "Cannot link things together because the original thing doesn't have an ID yet.");
                LinkedThingsId = Id;
                Update();
            }

            var linkedThing = Activator.CreateInstance<T>();
            linkedThing.CreatedOn = CreatedOn;
            linkedThing.CreatedByUserId = CreatedByUserId;
            AccountId.IfPositive(v => linkedThing.AccountId = v);
            linkedThing.LinkedThingsId = LinkedThingsId;

            return linkedThing;
        }


        //var linkedThing = new MembershipThing();
        //linkedThing.CreatedOn = membership.CreatedOn;
        //linkedThing.CreatedByUserId = membership.CreatedByUserId;
        //linkedThing.UserId = membership.UserId;
        //membership.AccountId.IfPositive(v => linkedThing.AccountId = v);
        ////linkedThing.LinkedThingsId = membership.LinkedThingsId;
        //linkedThing.QueueInsert(batcher, new List<SubstituteParameter>(new SubstituteParameter[]{ new SubstituteParameter{
        //    ColumnName = "LinkedThingsId",
        //    ValueParameter = membership.InsertIdParam,
        //    ApplyValue = (v) =>
        //        {
        //            linkedThing.LinkedThingsId = (int)v;
        //        }
        //}, new SubstituteParameter{
        //    ColumnName = "TargetId",
        //    ValueParameter = production.InsertIdParam,
        //    ApplyValue = (v) =>
        //        {
        //            linkedThing.TargetId = (int)v;
        //        }
        //}}));

        #endregion

        #region Serialization

        /// <summary>
        /// Serializes this object to a JSON string.
        /// </summary>
        /// <returns></returns>
        public string SerializeToString()
        {
            return JsonConvert.SerializeObject(SerializeJson());
        }

        /// <summary>
        /// Creates a JObject that has all the properties needed to recreated this thing via JSON.
        /// </summary>
        /// <returns></returns>
        public virtual JObject SerializeJson()
        {
            dynamic json = new JObject();
            json.Type = Type;
            
            foreach (ThingProperty prop in PropertiesMetaData.AllProperties())
            {
                prop.SerializeJson(this, json);
            }

            if (HasChildren)
            {
                json.Children = new JArray(Children.Select(c => c.SerializeJson()));
            }

            return json;
        }

        public string SerializeToString2()
        {

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);
            SerializeJson2(jsonWriter);
            jsonWriter.Close();
            return sb.ToString();

        }

        public virtual void SerializeJson2(JsonWriter writer)
        {
            writer.WriteStartObject();

            SerializeJsonProperties2(writer);

            writer.WriteEndObject();
        }

        public virtual void SerializeJsonProperties2(JsonWriter writer)
        {
            writer.WritePropertyName("Type");
            writer.WriteValue(Type);

            foreach (var prop in PropertiesMetaData.AllProperties())
            {
                prop.SerializeJson2(this, writer);
            }

            if (HasChildren)
            {
                writer.WritePropertyName("Children");
                writer.WriteStartArray();
                foreach( var child in Children)
                {
                    child.SerializeJson2(writer);
                }
                writer.WriteEndArray();
            }
        }



        /// <summary>
        /// Creates a new Thing from a serialized version.  This could be a byte array (obsolete) or a json string.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ThingBase Deserialize(object data)
        {
            ThingBase thing = null;
            var asString = data as string;
            if (asString != null)
            {
//                CreateThingFromServiceStackJson(asString);
                dynamic json = JObject.Parse(asString);
                thing = CreateThingFromJson(json);
            }
            else if (data is byte[])
            {
                var bytes = (byte[]) data;
                using (var memStream = new MemoryStream())
                {
                    var binForm = new BinaryFormatter
                                      {
                                          SurrogateSelector = Things.SurrogateSelector
                                      };
                    memStream.Write(bytes, 0, bytes.Length);
                    memStream.Seek(0, SeekOrigin.Begin);
                    thing = (ThingBase) binForm.Deserialize(memStream);
                }
            }
            thing.BuildTree();
            return thing;
        }


        /// <summary>
        /// Creates the thing and fills its properties and children.  DOES NOT CALL BUILDTREE, which should be called on the container separatly.
        /// </summary>
        /// <param name="thing"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static ThingBase CreateThingFromJson(JObject json)
        {
            Type thingType = Things.ThingType((string) json["Type"]);
            var thing = (ThingBase) Activator.CreateInstance(thingType);
            thing.DeserializeJson(json);
            return thing;
        }

        public virtual void DeserializeJson(JObject json)
        {

            foreach (ThingProperty prop in PropertiesMetaData.AllProperties())
            {
                prop.DeserializeJson(this, json);
            }

            json["Children"].IfNotNull(
                v =>
                {
                    _children =
                        (v).Cast<JObject>().Select(thingJson => { return CreateThingFromJson(thingJson); }).ToList();
                });
        }



        #region Deserialize using ServiceStack JSON Parser

        public static ThingBase CreateThingFromServiceStackJson(string jsonText)
        {
            var json = ServiceStack.Text.JsonObject.Parse(jsonText);
            return CreateThingFromServiceStackJson(json);
        }

        public static ThingBase CreateThingFromServiceStackJson(IDictionary<string,string> json)
        {
            Type thingType = Things.ThingType((string)json["Type"]);
            var thing = (ThingBase)Activator.CreateInstance(thingType);
            thing.DeserializeJsonServiceStack(json);
            return thing;
        }

        public virtual void DeserializeJsonServiceStack(IDictionary<string, string> json)
        {
            foreach (ThingProperty prop in PropertiesMetaData.AllProperties())
            {

                prop.DeserializeJsonServiceStack(this, json);
            }

            //if (json["Children"] != null)
            //{
            //    var children = ServiceStack.Text.JsonObject.Parse(jsonText);
            //}
            //json["Children"].IfNotNull(
            //    v =>
            //    {
            //        _children = v.Select(thingJson => { return CreateThingFromServiceStackJson(thingJson); }).ToList();
            //    });
        }

        #endregion

        //public static ThingBase CreateThingFromJson2(JsonTextReader reader)
        //{
        //    ThingBase thing = null;
        //    while (true)
        //    {
        //        if (reader.TokenType == JsonToken.PropertyName)
        //        {
        //            string propertyName = Convert.ToString(reader.Value);
        //            if (!propertyName.EqualsCaseSensitive("Type"))
        //            {
        //                throw new Exception("Error during parsing.  First JSON property should be Type.");
        //            }

        //            reader.Read();
        //            Type thingType = Things.ThingType(Convert.ToString(reader.Value));
        //            thing = (ThingBase)Activator.CreateInstance(thingType);
        //            reader.Read();//go the next token
        //            thing.DeserializeProperties2(reader);
        //            return thing;
        //        }
        //        else if (!reader.Read())
        //            return null;
        //    }
        //}

        //public virtual void DeserializeProperties2(JsonTextReader reader)
        //{
        //    while (reader.TokenType != JsonToken.EndObject)
        //    {
        //        if (reader.TokenType == JsonToken.PropertyName)
        //        {
        //            string propertyName = Convert.ToString(reader.Value);
        //            reader.Read();//move to value, which could be a string, array start, object start, etc
        //            DeserializeProperty2(propertyName, reader);
        //        }
        //        else if (!reader.Read())
        //            return;
        //    }
        //}

        //public virtual void DeserializeProperty2(string propertyName, JsonTextReader reader)
        //{

        //    switch (propertyName)
        //    {
        //        case "Type":
        //            throw new Exception("Type should have been read already.");
        //        case "Children":
        //            this._children = new List<ThingBase>();
        //            if ( reader.TokenType != JsonToken.StartArray)
        //                throw new Exception("Invalid Json.  Children property isn't an array.");
        //            reader.Read();
        //            while (reader.TokenType != JsonToken.EndArray)
        //            {
        //                var child = CreateThingFromJson2(reader);
        //                _children.Add(child);
        //                reader.Read();//should be at EndObject
        //            }
        //            break;
        //        default:
        //            ThingProperty prop = PropertiesMetaData.Property(propertyName);
        //            if (prop == null)
        //            {
        //                DeserializeCustomProperty2(propertyName, reader);
        //            }
        //            else
        //            {
        //                prop.DeserializeJson2(this, reader);
        //            }
        //            break;
        //    }
        //}

        //protected virtual void DeserializeCustomProperty2(string propertyName, JsonTextReader reader)
        //{

        //}


        /// <summary>
        /// Serializes this thing's property and descendants for .net serialzation.
        /// </summary>
        /// <param name="info"></param>
        public virtual void Serialize(SerializationInfo info)
        {
            info.SetType(GetType());
            foreach (ThingProperty prop in PropertiesMetaData.AllProperties())
            {
                prop.Serialize(this, info);
            }

            if (HasChildren)
                info.AddValue("Children", Children);
        }

        public virtual void Deserialize(SerializationInfo info)
        {
            ThingPropertyContainerMetaData props = PropertiesMetaData;
            SerializationInfoEnumerator e = info.GetEnumerator();
            while (e.MoveNext())
            {
                DeserializeProperty(e.Name, e.Value, info);
            }
        }

        public virtual bool DeserializeProperty(string name, object value, SerializationInfo info)
        {
            switch (name)
            {
                case "Children":
                    _children = (List<ThingBase>) value;
                    return true;
                default:
                    ThingProperty property = PropertiesMetaData.Property(name);
                    if (property == null)
                        return false;

                    property.Deserialize(this, value, info);
                    return true;
            }
        }


        //public ThingBase boner()
        //{
        //    BinaryFormatter bf = new BinaryFormatter
        //    {
        //        //SurrogateSelector = Things.SurrogateSelector
        //    };

        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        bf.Serialize(ms, this);
        //        var bytes = ms.ToArray();

        //        using (MemoryStream memStream = new MemoryStream())
        //        {
        //            memStream.Write(bytes, 0, bytes.Length);
        //            memStream.Seek(0, SeekOrigin.Begin);
        //            var thing = (ThingBase)bf.Deserialize(memStream);
        //            return thing;
        //        }
        //    }
        //}

        /// <summary>
        /// Implements ISerializable to serialize this object.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            Serialize(info);
        }

        #endregion
    }


    /// <summary>
    /// Contains data that is transformed from Thing objects (with children, etc) into an object that 
    /// is useful for constructing views.   These objects are nice to use in an MVC view as the view model.
    /// They also (unless you screw it up) are able to be serlialized into JSON where javascript can have its way with meaninful data. 
    /// Contains data that is used to create a typical UI view.  Can also be serialized into JSON and sent to the UI for javascript processing.
    /// </summary>
    /// <remarks>
    /// Eventually this whole system should be removed in favor for plain viewmodel classes.
    /// </remarks>
    public abstract class ThingModelView
    {
        public int Id { get; set; }
    }

    /// <summary>
    /// ViewData that has other properties that are needed by most things.
    /// </summary>
    public class ThingBasicModelView : ThingModelView
    {
        public long CreatedOn { get; set; }

        public UserView Creator { get; set; }

        /// <summary>
        /// When true, the current user is the creator of this thing.
        /// </summary>
        public bool IsCreator { get; set; }

        /// <summary>
        /// Whether the current user can edit this thing.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Whether the current user can delete this thing.
        /// </summary>
        public bool CanDelete { get; set; }
    }


    internal sealed class ThingSerializationSurrogate : ISerializationSurrogate
    {
        #region ISerializationSurrogate Members

        public void GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
        {
            ((ThingBase) obj).Serialize(info);
        }

        public Object SetObjectData(Object obj, SerializationInfo info, StreamingContext context,
                                    ISurrogateSelector selector)
        {
            ((ThingBase) obj).Deserialize(info);
            return obj;
        }

        #endregion
    }

    //temporary for db code update
    public interface IContainerThing
    {
        
    }
}