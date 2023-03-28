using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Web.Script.Serialization;
using System.Data.SqlClient;

namespace Takeoff.Models
{
    public partial class ThingBase : ITypicalEntity
    {

        #region Select

        /// <summary>
        /// Creates a simple auxillary data filler that gets a single row from an auxillary table and fills data properties automatically.  This is useful for simple thing types.
        /// </summary>
        /// <typeparam name="TDataModel"></typeparam>
        /// <param name="db"></param>
        /// <returns></returns>
        protected ThingAuxDataFiller<TDataModel> CreateAuxDataFiller<TDataModel>(DataModel db)
        {
            return new ThingAuxDataFiller<TDataModel>(db, Id, new Action<TDataModel>((d) =>
            {
                FillPropertiesWithRecord(d);
            }));
        }

        /// <summary>
        /// Sets all the auto properties with data from the data model.  
        /// </summary>
        /// <param name="dataRecord"></param>
        public void FillPropertiesWithRecord(object dataRecord)
        {
            //todo: put properties into dictionary by type...put that in PropertiesMetaData
            var modelType = dataRecord.GetType();
            var properties = this.PropertiesMetaData.PropertiesPerDataType(modelType);
            foreach (var property in properties)
            {
                property.FillWithRecord(this, dataRecord);
            }
        }


        /// <summary>
        /// Sets properties on the data model that were set on this thing.
        /// </summary>
        /// <param name="dataRecord"></param>
        public object FillRecordWithProperties(object dataRecord)
        {
            //todo: put properties into dictionary by type...put that in PropertiesMetaData
            var modelType = dataRecord.GetType();
            var properties = this.PropertiesMetaData.PropertiesPerDataType(modelType);
            foreach (var property in properties)
            {
                property.FillRecord(this, dataRecord);
            }
            return dataRecord;
        }


        #endregion

        #region Select V2 

        
        public virtual void AddDataFillers(List<IThingAuxDataFiller> fillers, Dictionary<int, ThingBase> things, DataModel db)
        {
        }


        #endregion

        #region Insert

        /// <summary>
        /// Adds queued commands to the batcher.  Also includes callback code to set ID values (ID, ParentId, ContainerID, etc) once the command executes.
        /// Check out Things.Insert() for the actual function to insert the data into the DB.
        /// </summary>
        /// <remarks>Note you should call RemoveFromCache after the batcher is executed, unless this is a container then you should be safe.</remarks>
        /// <param name="batcher"></param>
        public virtual void QueueInsertData(CommandBatcher batcher, List<SubstituteParameter> substituteParams = null)
        {
            if (this.Id > 0)
                throw new InvalidOperationException("This has already been inserted into the DB");
            if (!CreatedOnProperty.IsSet(this) && Parent != null && Parent.CreatedOn != default(DateTime))
                this.CreatedOn = Parent.CreatedOn;
            else if (!CreatedOnProperty.IsSet(this))
                this.CreatedOn = DateTime.UtcNow;

            var thingData = new Data.Thing();
            thingData.Type = Things.ThingType(GetType());
            FillRecordWithProperties(thingData);
            //accountid, parentid, and containerid are a tad tricky.  default values aren't included in data right now

            var parentId = this.ParentId;
            if (parentId.HasValue)
            {
                thingData.ParentId = parentId.Value;
            }
            else if (Parent != null && Parent.InsertIdParam != null)
            {
                substituteParams = new List<SubstituteParameter>();
                substituteParams.Add(new SubstituteParameter
                {
                    ColumnName = "ParentId",
                    ValueParameter = Parent.InsertIdParam,
                    ApplyValue = (v) =>
                    {
                        this.ParentId = (int)v;
                    }
                });
            }
            var containerId = ContainerId;
            if (containerId.HasValue)
            {
                thingData.ContainerId = containerId.Value;
            }
            else if (Container != null && Container.InsertIdParam != null)
            {
                    if (substituteParams == null)
                        substituteParams = new List<SubstituteParameter>();
                    substituteParams.Add(new SubstituteParameter
                    {
                        ColumnName = "ContainerId",
                        ValueParameter = Container.InsertIdParam,
                        ApplyValue = (v) =>
                        {
                            this.ContainerId = (int)v;
                        }
                    });
            }

            //insert the Thing record 
            InsertIdParam = batcher.QueueInsertAutoId(thingData, substituteParams == null ? null : substituteParams.ToArray(), () =>
            {
                Id = (int)InsertIdParam.Value;
                if (substituteParams != null)
                {
                    foreach (var substitute in substituteParams.Where(s => s.ApplyValue != null && s.ValueParameter != null))
                    {
                        substitute.ApplyValue(substitute.ValueParameter.Value);
                    }
                }
            });

            QueueAuxillaryInsert(batcher);

            if (LogInsertActivity)
                QueueActivityInsert(batcher, ThingChanges.Add);

            foreach (var child in Children)
            {
                child.QueueInsertData(batcher);
            }

            batcher.Executed += (o, e) =>
            {
                OnInserted();
            };

        }

        /// <summary>
        /// Called after this thing has been inserted into the database.  ID will be set and it may have children already.
        /// </summary>
        protected virtual void OnInserted()
        {

        }
   
        protected virtual void QueueActivityInsert(CommandBatcher batcher, string action, int? userId = null, string data = null, DateTime? date = null)
        {
            if (!userId.HasValue)
                userId = Users.CurrUserId;
            Args.HasChars(action, "action");

            var creationDate = date.HasValue ? date.Value : DateTime.UtcNow;
            var activityData = new Takeoff.Models.Data.ActionSource
            {
                Action = action,
                Date = creationDate,
                UserId = userId.GetValueOrDefault(),
                Data = data,                
                ThingType = this.Type,
            };

            List<SubstituteParameter> substituteParams = new List<SubstituteParameter>(2);
            if (this.Id > 0)
            {
                activityData.ThingId = this.Id;
            }
            else if (InsertIdParam != null)
            {
                substituteParams.Add(new SubstituteParameter
                {
                    ColumnName = "ThingId",
                    ValueParameter = InsertIdParam
                });
            }

            if (ParentId.HasValue)
            {
                activityData.ThingParentId = ParentId.Value;
            }
            else if (Parent != null && Parent.InsertIdParam != null)
            {
                substituteParams.Add(new SubstituteParameter
                {
                    ColumnName = "ThingParentId",
                    ValueParameter = Parent.InsertIdParam
                });
            }
            var activityIdParam = batcher.QueueInsertAutoId(activityData, substituteParams.ToArray());

            //now we add Action records that basically act as a change bubbler.  starts with the current thing and works its way all the up to the container
            var toNotify = new List<ThingBase>();
            toNotify.Add(this);//add the thing as well 
            toNotify.AddRange(Ancestors());
            toNotify.Each(t =>
            {
                substituteParams.Clear();
                substituteParams.Add(new SubstituteParameter
                {
                    ColumnName = "ChangeDetailsId",
                    ValueParameter = activityIdParam
                });
                var notification = new Takeoff.Models.Data.Action();
                //if this thing is already inserted, use its ID.  Otherwise use the ID parameter
                if (t.Id > 0)
                {
                    notification.ThingId = t.Id;
                }
                else if (t.InsertIdParam != null)
                {
                    substituteParams.Add(new SubstituteParameter
                    {
                        ColumnName = "ThingId",
                        ValueParameter = t.InsertIdParam
                    });
                }
                else
                {
                    throw new InvalidOperationException("Couldn't bubble to thing because it was missing an ID and its parameter.");
                }
                batcher.QueueInsertAutoId(notification, substituteParams.ToArray(), () =>
                                                                                        {
                                                                                            t.IfType<ILatestChangeAware>
                                                                                                (tc =>
                                                                                                     {
                                                                                                         var changeId = (int)activityIdParam.Value;
                                                                                                         if (changeId > tc.LastChangeId.GetValueOrDefault())
                                                                                                         {
                                                                                                             tc.LastChangeId = changeId;
                                                                                                             tc.LastChangeDate = creationDate;
                                                                                                         }

                                                                                                     });

                    });
            });
        }

        protected virtual void QueueAuxillaryInsert(CommandBatcher batcher)
        {
        }

      
        #endregion

        #region Delete

        public ThingBase Delete()
        {
            return Delete(false);
        }

        public ThingBase Delete(DateTime deletedOn)
        {
            return Delete(false,deletedOn);
        }

        public ThingBase Delete(bool deleteDbRecord)
        {
            return Delete(deleteDbRecord, DateTime.UtcNow);
        }

        /// <summary>
        /// Marks the thing as deleted.  Things.Get will think it doesn't exist, but the records will still be available in the database.
        /// </summary>
        /// <param name="deleteDbRecord">If true, the database record will be deleted permanently.  Otherwise, the record will be marked as deleted.  When marked as deleted, the database record will still be there (for analysis) but Things.Get will return null when requesting the thing.</param>
        public ThingBase Delete(bool deleteDbRecord, DateTime deletedOn)
        {
            using (var batcher = new CommandBatcher())
            {
                Delete(deleteDbRecord, batcher, deletedOn);
                batcher.Execute();
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="verifyPermission"></param>
        /// <param name="deleteDbRecord">If true, the database record will be deleted permanently.  Otherwise, the record will be marked as deleted.  When marked as deleted, the database record will still be there (for analysis) but Things.Get will return null when requesting the thing.</param>
        /// <param name="batcher"></param>
        /// <returns></returns>
        public ThingBase Delete(bool deleteDbRecord, CommandBatcher batcher, DateTime deletedOn, bool deleteLinkedThings = true)
        {
            
            this.ThisAndDescendants().Each(d =>
            {
                if (d.Id == this.Id || !d.IsContainer)
                {
                    d.DeleteDataOrMarkAsDeleted(deleteDbRecord, batcher, deletedOn);
                }
                else
                {
                    //Thing trees will not build subtrees of descendants that are themselves a container.  A good example is Productions within an Account.  so if we took this thing here, we wouldn't delete any of its descendants.  so we get teh full thing and delete that
                    Things.Get(d.Id).Delete(deleteDbRecord, batcher, deletedOn, deleteLinkedThings);
                }

                if (deleteLinkedThings)
                {
                    var linkedThings = d.GetLinkedThings();
                    if (linkedThings.HasItems())
                    {
                        //note that we don't verify permission on linked things.  Otherwise security errors will occur, especially in something like the case of a user membership is deleted.
                        linkedThings.Each(t => t.Delete(deleteDbRecord, batcher, deletedOn, false));
                    }
                }
            }
            );

            QueueActivityInsert(batcher, ThingChanges.Delete, Users.CurrUserId);

            batcher.Executed += (o,e) =>
                {
                    ThisAndDescendants().Each(t => t.OnDeleted());
                    RemoveFromCache();
                    var parent = Parent;
                    if (parent != null)//so the current thing tree is in sync
                    {
                        var container = this.Container;
                        if (container != null)
                            container.DescendantsById.Remove(this.Id);

                        parent.Children.Remove(this);
                        parent.OnChildRemoved(this);
                    }
                };


            return this;
        }

        private void DeleteDataOrMarkAsDeleted(bool deleteDbRecord, CommandBatcher batcher, DateTime deletedOn)
        {
            if (deleteDbRecord)
            {
                QueueDeleteData(batcher);
            }
            else
            {
                batcher.QueueUpdate(this.Id, typeof(Data.Thing), new { DeletedOn = deletedOn });
            }
        }

        /// <summary>
        /// Called after this thing has been deleted (or marked as deleted) in the database.  Use this to delete files, invalidate related things, etc.
        /// </summary>
        protected virtual void OnDeleted()
        {

        }

        /// <summary>
        /// Deletes the data records this this thing, but not its descendants.
        /// </summary>
        /// <param name="db"></param>
        protected virtual void QueueDeleteData(CommandBatcher batcher)
        {
            batcher.QueueDeleteViaPrimaryKey(new Data.Thing
            {
                Id = Id
            });
        }


        #endregion

        #region Update

        //todo: provide an overload wtih change description and auto property updates
        public ThingBase Update(bool autoUpdate = true, string description = null, Dictionary<string,object> updatedProperties = null)
        {
            //add all the properties that have been changes
            if (this.IsTrackingPropertyChanges)
            {
                foreach (var property in PropertiesMetaData.AllProperties().Where(p => p.TrackChanges && p.IsChanged(this)))
                {
                    if (updatedProperties == null)
                    {
                        updatedProperties = new Dictionary<string, object>();
                    }
                    updatedProperties[property.Name] = property.GetValueAsObject(this);
                }
            }
            using (var db = new DataModel())
            {
                UpdateData(db);
                db.SubmitChanges();
            }

            string data = updatedProperties == null ? null : new JavaScriptSerializer().Serialize(updatedProperties);
            this.LogChange(ThingChanges.Update, null, data);

            RemoveFromCache();
            return this;
        }



        /// <summary>
        /// Updates the data records this this thing, but not its descendants.
        /// </summary>
        /// <param name="db"></param>
        private void UpdateData(DataModel db)
        {
            var thingData = (from t in db.Things where t.Id == Id select t).Single();
            FillRecordWithProperties(thingData);
            thingData.Type = Things.ThingType(GetType());
            UpdateAuxillaryData(db);
        }

        /// <summary>
        /// Override this to update any rows from tables beside Thing that this thing has data for.  Always call base.UpdateOnSubmit or else Thing record won't be removed.
        /// </summary>
        /// <param name="db"></param>
        protected virtual void UpdateAuxillaryData(DataModel db)
        {
        }

        #endregion

    }

  
}
