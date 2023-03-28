using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Takeoff.Data;
using Takeoff.Models;



using Takeoff.Models.Data;
using System.Text;
using System.Dynamic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace Takeoff.Models
{
    public interface IVideoThumbnailThing: IFileBase
    {
        /// <summary>
        /// The position, in seconds, within the video that this thumbnail was generated from.
        /// </summary>
        double Time { get; set; }

        /// <summary>
        /// The width, in pixels, of the thumbnail.
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// The height, in pixels, of the thumbnail.
        /// </summary>
        int Height { get; set; }
    }

    [ThingType("VideoThumbnail")]
    [Serializable]
    public class VideoThumbnailThing : FileBaseThing, IVideoThumbnailThing
    {
                
        #region Constructors

        public VideoThumbnailThing()
        {
        }

        protected VideoThumbnailThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {  
        }

        #endregion
        



        /// <summary>
        /// The position, in seconds, within the video that this thumbnail was generated from.
        /// </summary>
        public double Time
        {
            get
            {
                return TimeProperty.GetValue(this);
            }
            set
            {
                TimeProperty.SetValue(this, value);
            }
        }
        private double _Time;
        private static readonly ThingDataProperty<double, VideoThumbnailThing, Data.VideoThumbnail> TimeProperty = new ThingDataProperty<double, VideoThumbnailThing, Data.VideoThumbnail>()
        {
            Name = "Time",
            SetField = (o, v) => o._Time = v,
            GetField = o => o._Time,
            GetData = o => o.Time,
            SetData = (o, v) => o.Time = v,
        }.Register();
	
        /// <summary>
        /// The width, in pixels, of the thumbnail.
        /// </summary>
        public int Width
        {
            get
            {
                return WidthProperty.GetValue(this);
            }
            set
            {
                WidthProperty.SetValue(this, value);
            }
        }
        private int _Width;
        private static readonly ThingDataProperty<int, VideoThumbnailThing, Data.VideoThumbnail> WidthProperty = new ThingDataProperty<int, VideoThumbnailThing, Data.VideoThumbnail>()
        {
            Name = "Width",
            SetField = (o, v) => o._Width = v,
            GetField = o => o._Width,
            GetData = o => o.Width,
            SetData = (o, v) => o.Width = v,
        }.Register();
	
        /// <summary>
        /// The height, in pixels, of the thumbnail.
        /// </summary>
        public int Height
        {
            get
            {
                return HeightProperty.GetValue(this);
            }
            set
            {
                HeightProperty.SetValue(this, value);
            }
        }
        private int _Height;
        private static readonly ThingDataProperty<int, VideoThumbnailThing, Data.VideoThumbnail> HeightProperty = new ThingDataProperty<int, VideoThumbnailThing, Data.VideoThumbnail>()
        {
            Name = "Height",
            SetField = (o, v) => o._Height = v,
            GetField = o => o._Height,
            GetData = o => o.Height,
            SetData = (o, v) => o.Height = v,
        }.Register();
	

        protected internal override IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            foreach (var d in base.FillAuxillaryData(db))
                yield return d;
            yield return this.CreateAuxDataFiller<Data.VideoThumbnail>(db);
        }


        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(FillRecordWithProperties(new Data.VideoThumbnail()), InsertIdParam);
        }


        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            FillRecordWithProperties((from d in db.VideoThumbnails where d.ThingId == Id select d).Single());
        }


        protected override void QueueDeleteData(CommandBatcher batcher)
        {
            base.QueueDeleteData(batcher);
            batcher.QueueDeleteViaPrimaryKey(new Data.VideoThumbnail
            {
                ThingId = Id
            });
        }


        protected override ThingModelView CreateViewInstance(string viewName)
        {
            return new VideoThumbnailThingView();
        }

        protected override void FillView(string viewName, ThingModelView view, Identity identity)
        {
            base.FillView(viewName, view, identity);
            var typedView = (VideoThumbnailThingView)view;
            typedView.Time = this.Time;
            typedView.Width = this.Width;
            typedView.Height = this.Height;
            typedView.Url = GetUrl();
        }

    }


    public class VideoThumbnailThingView : ThingModelView
    {
        /// <summary>
        /// The fully url (S3) to the thumbnail
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The position, in seconds, within the video that this thumbnail was generated from.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// The width, in pixels, of the thumbnail.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height, in pixels, of the thumbnail.
        /// </summary>
        public int Height { get; set; }
    }


}