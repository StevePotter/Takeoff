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
    /// <summary>
    /// Represents a video stream that comes from a progressively-downloaded video file.
    /// </summary>
    /// <remarks>
    /// TranscodeJobId was left out because transcode jobs are really meant for backend purposes.  So it has no real bearing for Things, since they aren't intended necessarily for backend stuff.
    /// </remarks>
    [ThingType("VideoStream")]
    [Serializable]
    public class VideoStreamThing : FileBaseThing, IVideoStream
    {
                #region Constructors

        public VideoStreamThing()
        {
        }

        protected VideoStreamThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {  
        }

        #endregion
        


        /// <summary>
        /// The bit rate, in KPS, of the video.
        /// </summary>
        public int? VideoBitRate
        {
            get
            {
                return VideoBitRateProperty.GetValue(this);
            }
            set
            {
                VideoBitRateProperty.SetValue(this, value);
            }
        }
        private int? _VideoBitRate;
        private static readonly ThingDataProperty<int?, VideoStreamThing, Data.VideoStream> VideoBitRateProperty = new ThingDataProperty<int?, VideoStreamThing, Data.VideoStream>()
        {
            Name = "VideoBitRate",
            SetField = (o, v) => o._VideoBitRate = v,
            GetField = o => o._VideoBitRate,
            ShouldGetData = r => r.VideoBitRate != null,
            GetData = o => o.VideoBitRate,
            SetData = (o, v) => o.VideoBitRate = v,
        }.Register();
	

        /// <summary>
        /// The bit rate, in KPS, of the audio.
        /// </summary>
        public int? AudioBitRate
        {
            get
            {
                return AudioBitRateProperty.GetValue(this);
            }
            set
            {
                AudioBitRateProperty.SetValue(this, value);
            }
        }
        private int? _AudioBitRate;
        private static readonly ThingDataProperty<int?, VideoStreamThing, Data.VideoStream> AudioBitRateProperty = new ThingDataProperty<int?, VideoStreamThing, Data.VideoStream>()
        {
            Name = "AudioBitRate",
            SetField = (o, v) => o._AudioBitRate = v,
            GetField = o => o._AudioBitRate,
            ShouldGetData = r => r.AudioBitRate != null,
            GetData = o => o.AudioBitRate,
            SetData = (o, v) => o.AudioBitRate = v,
        }.Register();
	

        /// <summary>
        /// The encoding profile that is used to select the proper stream for a given device or browser.
        /// </summary>
        /// <remarks>
        /// Allowed values:
        /// "Web"
        /// "Mobile"
        /// </remarks>
        public string Profile
        {
            get
            {
                return ProfileProperty.GetValue(this);
            }
            set
            {
                ProfileProperty.SetValue(this, value);
            }
        }
        private string _Profile;
        private static readonly ThingDataProperty<string, VideoStreamThing, Data.VideoStream> ProfileProperty = new ThingDataProperty<string, VideoStreamThing, Data.VideoStream>()
        {
            Name = "Profile",
            SetField = (o, v) => o._Profile = v,
            GetField = o => o._Profile,
            GetData = o => o.Profile,
            SetData = (o, v) => o.Profile = v,
        }.Register();
	

        protected internal override IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            foreach (var d in base.FillAuxillaryData(db))
                yield return d;
            yield return this.CreateAuxDataFiller<Data.VideoStream>(db);
        }

        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(FillRecordWithProperties(new VideoStream()), InsertIdParam);
        }

        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            var data = (from d in db.VideoStreams where d.ThingId == Id select d).Single();
            FillRecordWithProperties(data);
        }

        protected override void QueueDeleteData(CommandBatcher batcher)
        {
            base.QueueDeleteData(batcher);
            batcher.QueueDeleteViaPrimaryKey(new Data.VideoStream
            {
                ThingId = Id
            });
        }

    }

}