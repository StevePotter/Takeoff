using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Takeoff.Models;


using Takeoff.Models.Data;
using System.Web.Script.Serialization;
using Takeoff.Data;

namespace Takeoff.Models
{
    /// <summary>
    /// Provides notifications of changes on Things.
    /// </summary>
    public static class ThingChanges
    {
        public const string Add = "Add";
        public const string Delete = "Delete";
        public const string Update = "Update";

        /// <summary>
        /// Creates records in the Change table the this entity and all its ancestors to notify everyone that this entity changed.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="action"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static ThingBase LogChange(this ThingBase thing, string action, int? userId = null, string data = null, DateTime? date = null)
        {
            if (!userId.HasValue)
                userId = Users.CurrUserId;
            
            thing.ValidateAsArg();
            Args.HasChars(action,"action");

            using (DataModel db = new DataModel())
            {
                var creationDate = date.HasValue ? date.Value : DateTime.UtcNow;
                //create the original change record
                var change = new ActionSource
                {
                    Action = action,
                    Date = creationDate,
                    UserId = userId.GetValueOrDefault(),
                    Data = data,
                    ThingId = thing.Id,
                    ThingType = thing.Type,//Type, ParentId, ContainerId are only really useful when a thing is deleted
                    ThingParentId = thing.ParentId,
                };
                db.ActionSources.InsertOnSubmit(change);
                db.SubmitChanges();

                //bubble the changes up the tree
                var toUpdate = thing.Ancestors().ToList();
                toUpdate.Add(thing);//add the thing as well 
                toUpdate.Each(t =>
                {
                    db.Actions.InsertOnSubmit(new Takeoff.Models.Data.Action
                    {
                        ChangeDetailsId = change.Id,
                        ThingId = t.Id
                    });
                    t.RemoveFromCache();//or update lastthingId and update in cache
                    t.IfType<ILatestChangeAware>(tc =>
                                                     {
                                                         tc.LastChangeId = change.Id;
                                                         tc.LastChangeDate = change.Date;
                                                     });

                });

                db.SubmitChanges();
                
            }
            return thing;
        }

        /// <summary>
        /// Gets a list of EntityChange objects for all the entities that changed since the change ID passed.
        /// </summary>
        /// <param name="since"></param>
        /// <param name="entityType"></param>
        /// <param name="thingId"></param>
        /// <returns></returns>
        public static ThingChangesView GetChanges(ThingBase thing, int latestChangeId, Identity identity)
        {
            if (thing is ILatestChangeAware && thing.CastTo<ILatestChangeAware>().LastChangeId.GetValueOrDefault() == latestChangeId)
            {
                return null;
            }

            var result = new ThingChangesView();

            using (var db = DataModel.ReadOnly)
            {
                var changes = (from t in db.Things
                               join c in db.Actions on t.Id equals c.ThingId
                               join cd in db.ActionSources on c.ChangeDetailsId equals cd.Id
                               where c.ThingId == thing.Id &&
                               (latestChangeId == 0 || (c.ChangeDetailsId > latestChangeId))
                               orderby c.Id
                               select cd).ToList();

                if (changes.Count == 0)
                {
                    result.LatestChange = latestChangeId;
                    return result;
                }
                else
                {
                    result.LatestChange = changes.Last().Id;
                }

                ////this block filters out any objects that have been modified (maybe created) and then deleted within the change set
                //HashSet<int> toRemove = null;
                //changes.Each((change, index) =>
                //{
                //    if (Delete.Equals(change.Action))
                //    {
                //        changes.Each((otherChange, otherIndex) =>
                //        {
                //            if (index == otherIndex || (toRemove != null && toRemove.Contains(otherIndex)) || otherChange.ThingId != change.ThingId)
                //                return;
                //            if (toRemove == null)
                //                toRemove = new HashSet<int>();
                //            toRemove.Add(otherIndex);
                //            //the change set included adding and deleting the change so we can delete the "Delete" change as well and pretend it never happened
                //            if (Add.Equals(otherChange.Action))
                //                toRemove.Add(index);
                //        });
                //    }
                //});
                //if (toRemove != null)
                //{
                //    toRemove.OrderByDescending(o => o).Each((index) =>
                //    {
                //        changes.RemoveAt(index);
                //    });
                //}

                result.Changes = changes.Select(c =>
                    {
                        return CreateChangeView(c, identity);
                    }).Where(o=> o != null).ToArray();

                return result; 

            };

        }

        public static ThingActionView[] GetLatestChanges(ThingBase thing, int max, Identity identity)
        {
            using (var db = DataModel.ReadOnly)
            {
                var changes = (from t in db.Things
                               join c in db.Actions on t.Id equals c.ThingId
                               join cd in db.ActionSources on c.ChangeDetailsId equals cd.Id
                               where c.ThingId == thing.Id && cd.Action == ThingChanges.Add
                               orderby c.Id descending
                               select cd).Take(max).ToList();
                return changes.Select(c =>
                {
                    return CreateChangeView(c, identity);
                }).Where(o => o != null).ToArray();
            };

        }

        /// <summary>
        /// This crap should be refactored and cut out
        /// </summary>
        /// <param name="changedThing"></param>
        /// <param name="detail"></param>
        /// <returns></returns>
        public static ThingChangeContext CreateChangeContext(ThingBase changedThing, ActionSource detail, Identity identity)
        {
            UserThing author = null;
            if (detail.UserId > 0)
            {
                author = Things.GetOrNull<UserThing>(detail.UserId);
            }

            object viewData = null;
            if (detail.Action.EqualsCaseSensitive(Add))
            {
                viewData = changedThing.CreateViewData(null, identity);
            }
            else
            {
                if (detail.Data.HasChars())
                    viewData = new JavaScriptSerializer().DeserializeObject(detail.Data);
            }
            var view = new ThingActionView
            {
                UserId = detail.UserId,
                Action = detail.Action,
                ThingType = changedThing == null ? detail.ThingType : changedThing.Type,
                ThingId = detail.ThingId,
                ThingParentId = changedThing == null ? detail.ThingParentId : changedThing.ParentId,
                ViewData = viewData,
            };

            return new ThingChangeContext
            {
                Author = author,
                Details = detail,
                View = view
            };
        }

        public static ThingChangeContext CreateChangeContext(ActionSource detail, Identity identity)
        {
            UserThing author = null;
            if (detail.UserId > 0)
            {
                author = Things.GetOrNull<UserThing>(detail.UserId);
            }

            ThingBase changedThing = null;
            object viewData = null;
            if (!detail.Action.EqualsCaseSensitive(Delete))
            {
                changedThing = Things.GetOrNull(detail.ThingId);
                //in this case the thing was deleted.  so we ignore the change.  the Delete change will maybe be included in the change set or a ancestor was delted
                if (changedThing == null)
                {
                    return null;
                }
            }
            if (detail.Action.EqualsCaseSensitive(Add))
            {
                viewData = changedThing.CreateViewData(null, identity);
            }
            else
            {
                if (detail.Data.HasChars())
                    viewData = new JavaScriptSerializer().DeserializeObject(detail.Data);
            }
            var view = new ThingActionView
            {
                UserId = detail.UserId,
                Action = detail.Action,
                ThingType = changedThing == null ? detail.ThingType : changedThing.Type,
                ThingId = detail.ThingId,
                ThingParentId = changedThing == null ? detail.ThingParentId : changedThing.ParentId,
                ViewData = viewData,
            };

            return new ThingChangeContext
            {
                Author = author,
                Details = detail,
                View = view
            };
        }

        [Obsolete]
        public static ThingActionView CreateChangeView(ActionSource c, Identity identity)
        {
            ThingBase changedThing = null;
            object viewData = null;
            if (!c.Action.EqualsCaseSensitive(Delete))
            {
                changedThing = Things.GetOrNull(c.ThingId);
                //in this case the thing was deleted.  so we ignore the change.  the Delete change will maybe be included in the change set or a ancestor was delted
                if (changedThing == null)
                {
                    return null;
                }
            }
            if (c.Action.EqualsCaseSensitive(Add))
            {
                viewData = changedThing.CreateViewData(null, identity);
            }
            else
            {
                if (c.Data.HasChars())
                    viewData = new JavaScriptSerializer().DeserializeObject(c.Data);
            }
            return new ThingActionView
            {
                UserId = c.UserId,
                Action = c.Action,
                ThingType = changedThing == null ? c.ThingType : changedThing.Type,
                ThingId = c.ThingId,
                ThingParentId = changedThing == null ? c.ThingParentId : changedThing.ParentId,
                ViewData = viewData,
                ActivityPanelItem = changedThing == null ? null : changedThing.GetActivityPanelContents(c, true, identity)
            };
        }


    }



    /// <summary>
    /// Returned by a call to EntityChanges.GetChanges.
    /// </summary>
    public class ThingChangesView
    {
        public int LatestChange { get; set; }

        public ThingActionView[] Changes { get; set; }
    }

    /// <summary>
    /// Part of EntityChangesResult.
    /// </summary>
    [Serializable]
    public class ThingActionView
    {
        public string Action { get; set; }
        public int ThingId { get; set; }
        /// <summary>
        /// The thing's ParentId.
        /// </summary>
        public int? ThingParentId { get; set; }
        /// <summary>
        /// The user that made the change.
        /// </summary>
        public int UserId { get; set; }
        public string ThingType { get; set; }
        public object ViewData { get; set; }

        public object ActivityPanelItem { get; set; }
    }


    public class ChangeDigestEmailItem
    {
        public string Html { get; set; }

        public string Text { get; set; }

        public DateTime Date { get; set; }

        public bool IsRecipientTheAuthor { get; set; }
    }

    public class ThingChangeContext
    {
        public ThingActionView View { get; set; }

        public ActionSource Details { get; set; }

        public UserThing Author { get; set; }

        public bool IsDelete
        {
            get
            {
                return View.Action == ThingChanges.Delete;
            }
        }

        public bool IsAdd
        {
            get
            {
                return View.Action == ThingChanges.Add;
            }
        }

        public bool IsUpdate
        {
            get
            {
                return View.Action == ThingChanges.Update;
            }
        }
    }


}
