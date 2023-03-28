using System;
using System.Collections.Generic;
using System.Linq;
using Takeoff.Data;
using Takeoff.Models.Data;
using System.Runtime.Serialization;

namespace Takeoff.Models
{
    [ThingType("VideoComment")]
    [Serializable]
    public class VideoCommentThing : CommentThing, IVideoComment
    {
        #region Constructors

        public VideoCommentThing()
        {
        }

        protected VideoCommentThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        public double StartTime
        {
            get { return StartTimeProperty.GetValue(this); }
            set { StartTimeProperty.SetValue(this, value); }
        }

        private double _StartTime;

        private static readonly ThingDataProperty<double, VideoCommentThing, Data.VideoComment> StartTimeProperty =
            new ThingDataProperty<double, VideoCommentThing, Data.VideoComment>()
                {
                    Name = "StartTime",
                    SetField = (o, v) => o._StartTime = v,
                    GetField = o => o._StartTime,
                    GetData = o => o.StartTime,
                    SetData = (o, v) => o.StartTime = v,
                }.Register();

        protected internal override IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            foreach (var d in base.FillAuxillaryData(db))
                yield return d;
            yield return this.CreateAuxDataFiller<Data.VideoComment>(db);
        }

        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(FillRecordWithProperties(new VideoComment()), InsertIdParam);
        }

        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            var data = (from d in db.VideoComments where d.ThingId == Id select d).Single();
            FillRecordWithProperties(data);
        }

        protected override void QueueDeleteData(CommandBatcher batcher)
        {
            base.QueueDeleteData(batcher);
            batcher.QueueDeleteViaPrimaryKey(new Data.VideoComment
                                                 {
                                                     ThingId = Id
                                                 });
        }

        protected override ThingModelView CreateViewInstance(string viewName)
        {
            return new VideoCommentThingView();
        }

        protected override void FillView(string viewName, ThingModelView view, Identity identity)
        {
            base.FillView(viewName, view, identity);
            var typedView = (VideoCommentThingView) view;
            typedView.StartTime = this.StartTime;
        }
    }

    public class VideoCommentThingView : CommentThingView
    {
        public double StartTime { get; set; }
    }
}