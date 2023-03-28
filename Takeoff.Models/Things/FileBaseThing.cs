using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Takeoff.Models;



using Takeoff.Models.Data;
using System.Text;
using System.Dynamic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using Takeoff.Data;

namespace Takeoff.Models
{

    [Serializable]
    public abstract class FileBaseThing : ThingBase, IFileBase
    {
        
        #region Constructors

        public FileBaseThing()
        {
        }

        protected FileBaseThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {  
        }

        #endregion
        
        /// <summary>
        /// The named of the file in Takeoff's S3 file system.
        /// </summary>
        public string FileName
        {
            get
            {
                return FileNameProperty.GetValue(this);
            }
            set
            {
                FileNameProperty.SetValue(this, value);
            }
        }
        private string _FileName;
        private static readonly ThingDataProperty<string, FileBaseThing, Data.File> FileNameProperty = new ThingDataProperty<string, FileBaseThing, Data.File>()
        {
            Name = "FileName",
            SetField = (o, v) => o._FileName = v,
            GetField = o => o._FileName,
            GetData = o => o.FileName,
            SetData = (o, v) => o.FileName = v,
        }.Register();
	

        /// <summary>
        /// The name of the file when it was uploaded.
        /// </summary>
        public string OriginalFileName
        {
            get
            {
                return OriginalFileNameProperty.GetValue(this);
            }
            set
            {
                OriginalFileNameProperty.SetValue(this, value);
            }
        }
        private string _OriginalFileName;
        private static readonly ThingDataProperty<string, FileBaseThing, Data.File> OriginalFileNameProperty = new ThingDataProperty<string, FileBaseThing, Data.File>()
        {
            Name = "OriginalFileName",
            SetField = (o, v) => o._OriginalFileName = v,
            GetField = o => o._OriginalFileName,
            GetData = o => o.OriginalFileName,
            SetData = (o, v) => o.OriginalFileName = v,
        }.Register();

        /// <summary>
        /// The s3 bucket and folder of this file.  Example: to-d-projects/11195
        /// </summary>
        public string Location
        {
            get
            {
                return LocationProperty.GetValue(this);
            }
            set
            {
                LocationProperty.SetValue(this, value);
            }
        }
        private string _Location;
        private static readonly ThingDataProperty<string, FileBaseThing, Data.File> LocationProperty = new ThingDataProperty<string, FileBaseThing, Data.File>()
        {
            Name = "Location",
            SetField = (o, v) => o._Location = v,
            GetField = o => o._Location,
            GetData = o => o.Location,
            SetData = (o, v) => o.Location = v,
        }.Register();

        /// <summary>
        /// The file size, in bytes.
        /// </summary>
        public int? Bytes
        {
            get
            {
                return BytesProperty.GetValue(this);
            }
            set
            {
                BytesProperty.SetValue(this, value);
            }
        }
        private int? _Bytes;
        private static readonly ThingDataProperty<int?, FileBaseThing, Data.File> BytesProperty = new ThingDataProperty<int?, FileBaseThing, Data.File>()
        {
            Name = "Bytes",
            SetField = (o, v) => o._Bytes = v,
            GetField = o => o._Bytes,
            ShouldGetData = r => r.Bytes != null,
            GetData = o => o.Bytes,
            SetData = (o, v) => o.Bytes = v,
        }.Register();
	

        /// <summary>
        /// Indicates whether this is a sample file created when the user signed up or whatever.  This doesn't count toward plan usage.
        /// </summary>
        public bool IsSample
        {
            get
            {
                return IsSampleProperty.GetValue(this);
            }
            set
            {
                IsSampleProperty.SetValue(this, value);
            }
        }
        private bool _IsSample;
        private static readonly ThingDataProperty<bool, FileBaseThing, Data.File> IsSampleProperty = new ThingDataProperty<bool, FileBaseThing, Data.File>()
        {
            Name = "IsSample",
            SetField = (o, v) => o._IsSample = v,
            GetField = o => o._IsSample,
            ShouldGetData = r => r.IsSample != null,
            GetData = o => (bool)o.IsSample,
            SetData = (o, v) => o.IsSample = v,
        }.Register();


        /// <summary>
        /// Indicates whether this file counts toward an account's limit.
        /// </summary>
        public bool IsComplimentary
        {
            get
            {
                return IsComplimentaryProperty.GetValue(this);
            }
            set
            {
                IsComplimentaryProperty.SetValue(this, value);
            }
        }
        private bool _IsComplimentary;
        private static readonly ThingDataProperty<bool, FileBaseThing, Data.File> IsComplimentaryProperty = new ThingDataProperty<bool, FileBaseThing, Data.File>()
        {
            Name = "IsComplimentary",
            SetField = (o, v) => o._IsComplimentary = v,
            GetField = o => o._IsComplimentary,
            ShouldGetData = r => r.IsComplimentary != null,
            GetData = o => (bool)o.IsComplimentary,
            SetData = (o, v) => o.IsComplimentary = v,
        }.Register();
	

        /// <summary>
        /// When true, the file will be deleted when this thing is deleted.  It is set to false for shared files like for sample projects.  This is not really necessary since IsSample arrived.  todo: remove this
        /// </summary>
        public bool DeletePhysicalFile
        {
            get
            {
                return DeletePhysicalFileProperty.GetValue(this);
            }
            set
            {
                DeletePhysicalFileProperty.SetValue(this, value);
            }
        }
        private bool _DeletePhysicalFile;
        private static readonly ThingDataProperty<bool, FileBaseThing, Data.File> DeletePhysicalFileProperty = new ThingDataProperty<bool, FileBaseThing, Data.File>()
        {
            Name = "DeletePhysicalFile",
            SetField = (o, v) => o._DeletePhysicalFile = v,
            GetField = o => o._DeletePhysicalFile,
            GetData = o => o.DeletePhysicalFile,
            SetData = (o, v) => o.DeletePhysicalFile = v,
        }.Register();
	


        protected internal override IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            foreach (var d in base.FillAuxillaryData(db))
                yield return d;
            yield return this.CreateAuxDataFiller<Data.File>(db);
        }

        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(FillRecordWithProperties(new File()), InsertIdParam);
        }

        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            var data = (from d in db.Files where d.ThingId == Id select d).Single();
            FillRecordWithProperties(data);
        }

        protected override void QueueDeleteData(CommandBatcher batcher)
        {
            base.QueueDeleteData(batcher);
            batcher.QueueDeleteViaPrimaryKey(new Data.File
            {
                ThingId = Id
            });
        }

        protected override ThingModelView CreateViewInstance(string viewName)
        {
            return new FileThingView();
        }
        protected override void FillView(string viewName, ThingModelView view, Identity identity)
        {
            base.FillView(viewName, view, identity);
            var typedView = view as FileThingView;
            if (typedView != null)//videothumbnailview doesn't inherit from this
            {
                typedView.FileName = this.OriginalFileName.CharsOrEmpty();//jquery datatables was choking on null
                typedView.SizeBytes = this.Bytes.GetValueOrDefault();
                typedView.SizeFormatted = this.Bytes.HasValue ? new System.IO.FileSize(Bytes.Value).ToString() : "N/A";
            }
        }

        /// <summary>
        /// Gets a url for a file that is public.  If there is a current http request and it's secure, this will use https to prevent.
        /// </summary>
        /// <returns></returns>
        public string GetUrl(bool useHttpsIfCurrentRequestIsHttps = true)
        {
            Amazon.S3.Protocol protocol;
            var current = HttpContextFactory.Current;
            if (useHttpsIfCurrentRequestIsHttps && current != null && current.Request != null && current.Request.IsSecureConnection)
            {
                protocol = Amazon.S3.Protocol.HTTPS;
            }
            else
            {
                protocol = Amazon.S3.Protocol.HTTP;
            }
            return new S3FileLocation { Location = Location, FileName = FileName }.GetUrl(protocol);
        }

        public string GetUrlHttps()
        {
            return new S3FileLocation { Location = Location, FileName = FileName }.GetUrl(Amazon.S3.Protocol.HTTPS);
        }


        /// <summary>
        /// Eventually we will merge the FileName and Location properties.  Until then, this is an easy temporary way to handle it.
        /// </summary>
        /// <param name="url"></param>
        public string Url
        {
            set
            {
                var location = S3FileLocation.FromUrl(value);
                this.FileName = location.FileName;
                this.Location = location.Location;
            }
        }


    }

}