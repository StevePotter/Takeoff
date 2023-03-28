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
    [ThingType("Image")]
    [Serializable]
    public class ImageThing : FileBaseThing, IImage
    {
                #region Constructors

        public ImageThing()
        {
        }

        protected ImageThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {  
        }

        #endregion
        

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
        private static readonly ThingDataProperty<int, ImageThing, Data.Image> WidthProperty = new ThingDataProperty<int, ImageThing, Data.Image>()
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
        private static readonly ThingDataProperty<int, ImageThing, Data.Image> HeightProperty = new ThingDataProperty<int, ImageThing, Data.Image>()
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
            yield return this.CreateAuxDataFiller<Data.Image>(db);
        }


        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(FillRecordWithProperties(new Data.Image()), InsertIdParam);
        }


        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            FillRecordWithProperties((from d in db.Images where d.ThingId == Id select d).Single());
        }

        protected override void QueueDeleteData(CommandBatcher batcher)
        {
            base.QueueDeleteData(batcher);
            batcher.QueueDeleteViaPrimaryKey(new Data.Image
            {
                ThingId = Id
            });
        }

        protected override ThingModelView CreateViewInstance(string viewName)
        {
            return new ImageThingView();
        }

        protected override void FillView(string viewName, ThingModelView view, Identity identity)
        {
            base.FillView(viewName, view, identity);
            var typedView = (ImageThingView)view;
            typedView.Width = this.Width;
            typedView.Height = this.Height;
            typedView.Url = GetUrl();
        }

    }


    public class ImageThingView : ThingModelView
    {
        /// <summary>
        /// The fully url (S3) to the thumbnail
        /// </summary>
        public string Url { get; set; }

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