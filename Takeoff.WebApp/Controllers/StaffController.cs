using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mediascend.Web;
using System.Web.Script.Serialization;
using System.Collections;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Models.Data;
using System.Dynamic;
using MvcContrib;
using Takeoff.Transcoder;
using System.Configuration;
using Newtonsoft.Json.Linq;
using System.Web.UI.DataVisualization.Charting;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Text;
using Takeoff.Resources;
using Recurly;
using System.Xml.Linq;
using Takeoff.ViewModels;
using System.Linq.Dynamic;

namespace Takeoff.Controllers
{
    [SpecialRestriction(SpecialRestriction.Staff|SpecialRestriction.DeferredRequest)]
    public class StaffController : BasicController
    {
        public static bool IsCurrUserStaff(HttpContextBase httpContext)
        {
            return httpContext.IsLoggedIn() && StaffEmails.Contains(httpContext.UserThing().Email, StringComparer.OrdinalIgnoreCase);
        }

        public static HashSet<string> StaffEmails
        {
            get
            {
                if (_staffEmails == null)
                {
                    _staffEmails = new HashSet<string>(ConfigUtil.GetRequiredAppSetting("StaffUsers").Split(',').Select(e => e.Trim()).Where(e => e.HasChars()));
                }
                return _staffEmails;
            }
        }
        private static HashSet<string> _staffEmails;


        public ActionResult Index()
        {
            ViewData["SectionTitle"] = "Home";

            return View();
        }

        public ActionResult ClearCache()
        {
            ViewData["SectionTitle"] = "Clear Application Cache";

            if (Request.IsPost())
            {
                CacheUtil.ClearAppCache();
                return View("ClearCache.Success");
            }
            else
            {
                return View();
            }
        }

        public ActionResult FileSummary()
        {
            ViewData["SectionTitle"] = "File Summary";

            var now = this.RequestDate();
            using (var db = DataModel.ReadOnly)
            {
                //DeletePhysicalFile is for shared sample files, so exclude them from our report

                ViewData["FileCount"] = (from f in db.Files
                                         join t in db.Things on f.ThingId equals t.Id
                                         where t.Type == Things.ThingType(typeof(FileThing))
                                         select f).Count();
                ViewData["FileSize"] = new FileSize((from f in db.Files
                                                     join t in db.Things on f.ThingId equals t.Id
                                                     where t.Type == Things.ThingType(typeof(FileThing))
                                                     select (long?)f.Bytes).Sum().GetValueOrDefault()).ToString();
                ViewData["VideoCount"] = (from f in db.Files
                                          join t in db.Things on f.ThingId equals t.Id
                                          where f.DeletePhysicalFile && t.Type == Things.ThingType(typeof(VideoStreamThing))
                                          select f).Count();
                ViewData["VideoSize"] = new FileSize((from f in db.Files
                                                      join t in db.Things on f.ThingId equals t.Id
                                                      where f.DeletePhysicalFile && t.Type == Things.ThingType(typeof(VideoStreamThing))
                                                      select (long?)f.Bytes).Sum().GetValueOrDefault()).ToString();

                ViewData["total"] = FileTransferInfo.GetTransferInfo(db, new DateTime(2000, 1, 1), now);
                ViewData["1hr"] = FileTransferInfo.GetTransferInfo(db, now.AddHours(-1), now);
                ViewData["today"] = FileTransferInfo.GetTransferInfo(db, now.Date, now);
                ViewData["yesterday"] = FileTransferInfo.GetTransferInfo(db, now.Date.AddDays(-1), now.Date);
                ViewData["month"] = FileTransferInfo.GetTransferInfo(db, new DateTime(now.Year, now.Month, 1), now);

            }


            return View();
        }


        //public ActionResult ImpersonateAnonymousViewer(int productionId)
        //{
        //    IoC.Get<IIdentityService>().SetIdentity(new SemiAnonymousUserIdentity
        //                                                {
        //                                                    ProductionId = productionId,
        //                                                    UserId = Guid.NewGuid(),

        //                                                }, IdentityPeristance.TemporaryCookie, this.HttpContext);
        //    return this.RedirectToAction<ProductionsController>(c => c.Details(productionId, null, null, null, null));
        //}


        [HttpGet]
        public ActionResult ZencoderRequest()
        {
            ViewData["SectionTitle"] = "Make an API Request to Zencoder";

            var apiSkeleton = new
            {
                input = "http://bucket.s3.amazonaws.com/filekey",
                region = "us",
                output = new
                {
                    quality = 2,
                    audio_quality = 3,
                    keyframe_interval = 6,
                    width = 960,
                    url = "s3://bucket/key"
                }
            };
            ViewData["json"] = Newtonsoft.Json.JsonConvert.SerializeObject(apiSkeleton, Newtonsoft.Json.Formatting.Indented);

            return View();
        }

        [HttpPost]
        public ActionResult ZencoderRequest(string apiParams)
        {
            JObject o = JObject.Parse(apiParams);
            var apiKey = o.Property("api_key");
            if (apiKey == null)
            {
                o["api_key"] = ConfigurationManager.AppSettings["ZencoderApiKey"];
            }
            var poster = new JsonPoster(ZencoderClient.ApiBaseUrl);
            poster.PostJson("/jobs", o.ToString());

            var response = (Dictionary<string, object>)poster.DeserializeResponseJson();
            var jobId = response["id"];
            ViewData["JobId"] = jobId;
            return View();

        }


        public ActionResult UsageInfo()
        {
            ViewData["SectionTitle"] = "Usage Info";
            //Numbers
            //Avg video length & size per account
            //Avg videos per account
            //Avg views
            //Avg team size

            using (var db = DataModel.ReadOnly)
            {
                ViewData["UserCount"] = (from u in db.Users select u).Count();
                ViewData["UserVerifiedCount"] = (from u in db.Users where u.IsVerified select u).Count();
                ViewData["AccountCount"] = (from a in db.Accounts select a).Count();
                ViewData["AccountVerifiedCount"] = (from t in db.Things
                                                    join a in db.Accounts on t.Id equals a.ThingId
                                                    join u in db.Users on t.CreatedByUserId equals u.ThingId
                                                    where u.IsVerified
                                                    select u).Count();

                var videoBytesQuery = from t in db.Things
                                      join f in db.Files on t.Id equals f.ThingId
                                      where t.Type == Things.ThingType(typeof(VideoStreamThing)) && f.DeletePhysicalFile && f.Bytes.GetValueOrDefault() > 0
                                      select (System.Int64)f.Bytes;

                ViewData["VideoFileBytesAvg"] = new FileSize((long)videoBytesQuery.Average());
                ViewData["VideoFileBytesMin"] = new FileSize(videoBytesQuery.Min());
                ViewData["VideoFileBytesMax"] = new FileSize(videoBytesQuery.Max());
                ViewData["VideoFileBytesCount"] = videoBytesQuery.Count();
                ViewData["VideoFileBytesSum"] = new FileSize(videoBytesQuery.Sum());

                var videoDurationQuery = from t in db.Things
                                         join v in db.Videos on t.Id equals v.ThingId
                                         where v.Duration.GetValueOrDefault() > 0 && (from ft in db.Things
                                                                                      join f in db.Files on ft.Id equals f.ThingId
                                                                                      where ft.Type == Things.ThingType(typeof(VideoStreamThing)) && f.DeletePhysicalFile && (int)ft.ParentId == t.Id
                                                                                      select (int)ft.ParentId).Count() > 0
                                         select (System.Int64)v.Duration;

                ViewData["VideoDurationAvg"] = TimeSpan.FromSeconds(videoDurationQuery.Average());
                ViewData["VideoDurationMin"] = TimeSpan.FromSeconds(videoDurationQuery.Min());
                ViewData["VideoDurationMax"] = TimeSpan.FromSeconds(videoDurationQuery.Max());
                ViewData["VideoDurationSum"] = TimeSpan.FromSeconds(videoDurationQuery.Sum());
                ViewData["VideoCount"] = videoDurationQuery.Count();
                ViewData["VideoCountWithSample"] = (from v in db.Videos select v).Count();


                var productionCount = (from p in db.Projects select p).Count();
                var productionMembers = from p in db.Projects
                                        select (from mt in db.Things where mt.Type == Things.ThingType(typeof(MembershipThing)) && (int)mt.ParentId == p.ThingId select mt).Count();

                ViewData["ProductionMemberAvg"] = productionMembers.Average();
                ViewData["ProductionMemberMin"] = productionMembers.Min();
                ViewData["ProductionMemberMax"] = productionMembers.Max();
                ViewData["ProductionMemberSum"] = productionMembers.Sum();

                var videoViews = (from f in db.VideoWatchLogs
                                  where f.Bytes.GetValueOrDefault() > 0
                                  select (long)f.Bytes);

                ViewData["VideoDownloadBytesAvg"] = new FileSize((long)videoViews.Average());
                ViewData["VideoDownloadCount"] = videoViews.Count();
                ViewData["VideoDownloadBytesSum"] = new FileSize(videoViews.Sum());

                var videoDownloadsPerVideo = (from f in db.VideoWatchLogs
                                              group f by f.VideoId into g
                                              select g.Count());

                ViewData["VideoDownloadPerVidAvg"] = videoDownloadsPerVideo.Average();
                ViewData["VideoDownloadPerVidMax"] = videoDownloadsPerVideo.Max();


                var assetBytesQuery = from t in db.Things
                                      join f in db.Files on t.Id equals f.ThingId
                                      where t.Type == Things.ThingType(typeof(FileThing)) && f.DeletePhysicalFile && f.Bytes.GetValueOrDefault() > 0
                                      select (System.Int64)f.Bytes;

                ViewData["AssetFileBytesAvg"] = new FileSize((long)assetBytesQuery.Average());
                ViewData["AssetFileBytesMin"] = new FileSize(assetBytesQuery.Min());
                ViewData["AssetFileBytesMax"] = new FileSize(assetBytesQuery.Max());
                ViewData["AssetFileBytesCount"] = assetBytesQuery.Count();
                ViewData["AssetFileBytesSum"] = new FileSize(assetBytesQuery.Sum());


                var assetDownloads = (from f in db.FileDownloadLogs
                                      where f.FileThingType == Things.ThingType(typeof(FileThing))
                                      select (long)f.Bytes);

                ViewData["AssetDownloadBytesAvg"] = new FileSize((long)assetDownloads.Average());
                ViewData["AssetDownloadCount"] = assetDownloads.Count();
                ViewData["AssetDownloadBytesSum"] = new FileSize(assetDownloads.Sum());

                var assetDownloadsPerFile = (from f in db.FileDownloadLogs
                                             where f.FileThingType == Things.ThingType(typeof(FileThing))
                                             group f by f.FileThingId into g
                                             select g.Count());

                ViewData["AssetDownloadPerFileAvg"] = assetDownloadsPerFile.Average();
                ViewData["AssetDownloadPerFileMax"] = assetDownloadsPerFile.Max();




                ViewData["FileCount"] = (from f in db.Files
                                         join t in db.Things on f.ThingId equals t.Id
                                         where t.Type == Things.ThingType(typeof(FileThing))
                                         select f).Count();
                ViewData["FileSize"] = new FileSize((from f in db.Files
                                                     join t in db.Things on f.ThingId equals t.Id
                                                     where t.Type == Things.ThingType(typeof(FileThing))
                                                     select (long?)f.Bytes).Sum().GetValueOrDefault()).ToString();
                ViewData["VideoCount"] = (from f in db.Files
                                          join t in db.Things on f.ThingId equals t.Id
                                          where f.DeletePhysicalFile && t.Type == Things.ThingType(typeof(VideoStreamThing))
                                          select f).Count();
                ViewData["VideoSize"] = new FileSize((from f in db.Files
                                                      join t in db.Things on f.ThingId equals t.Id
                                                      where f.DeletePhysicalFile && t.Type == Things.ThingType(typeof(VideoStreamThing))
                                                      select (long?)f.Bytes).Sum().GetValueOrDefault()).ToString();

                var now = this.RequestDate();
                ViewData["total"] = FileTransferInfo.GetTransferInfo(db, new DateTime(2000, 1, 1), now);
                ViewData["1hr"] = FileTransferInfo.GetTransferInfo(db, now.AddHours(-1), now);
                ViewData["today"] = FileTransferInfo.GetTransferInfo(db, now.Date, now);
                ViewData["yesterday"] = FileTransferInfo.GetTransferInfo(db, now.Date.AddDays(-1), now.Date);
                ViewData["month"] = FileTransferInfo.GetTransferInfo(db, new DateTime(now.Year, now.Month, 1), now);



            }
            return View();
        }



        /// <summary>
        /// Shows data related to the videos that are watched.
        /// </summary>
        /// <returns></returns>
        public ActionResult VideoWatches()
        {
            ViewData["SectionTitle"] = "Video Watches";
            return View();
        }

        /// <summary>
        /// Streams a chart to graph data of videos viewed.  Used by VideoWatches
        /// </summary>
        /// <param name="dataToGraph">The type of data that should be graphed.  Supported values are: "watches","bandwidth","duration","users","accounts","productions".</param>
        /// <param name="from">The starting date to plot from. </param>
        /// <param name="to">The ending date to plot to.</param>
        /// <returns></returns>
        public ActionResult VideoWatchesChart(string dataToGraph, DateTime? from, DateTime? to)
        {
            if (!dataToGraph.HasChars())
                dataToGraph = "watches";
            dataToGraph = dataToGraph.Trim().ToLowerInvariant();
            if (!from.HasValue)
                from = DateTime.Now.Date;
            if (!to.HasValue || to.Value > DateTime.Now.Date)
                to = DateTime.Now.Date;

            from = from.Value.Date;
            using (var db = DataModel.ReadOnly)
            {
                var queryBase = db.VideoWatchLogs.Where(w => w.Bytes.GetValueOrDefault() > 0 && w.Duration.GetValueOrDefault() > 0 && w.Date >= from.Value && w.Date < to.Value.Date.AddDays(1)).GroupBy(w => w.Date.Date).OrderBy(g => g.Key);
                IEnumerable data;

                switch (dataToGraph)
                {
                    case "watches":
                        data = queryBase.Select(g => new { Date = g.Key, Value = (long)g.Count() }).ToArray();
                        break;
                    case "bandwidth":
                        data = queryBase.Select(g => new { Date = g.Key, Value = g.Sum(f => (long)f.Bytes) }).ToArray();
                        break;
                    case "duration":
                        data = queryBase.Select(g => new { Date = g.Key, Value = g.Sum(f => (long)f.Duration) }).ToArray();
                        break;
                    case "users":
                        data = queryBase.Select(g => new { Date = g.Key, Value = (long)g.Select(w => w.UserId).Distinct().Count() }).ToArray();
                        break;
                    case "accounts":
                        data = queryBase.Select(g => new { Date = g.Key, Value = (long)g.Select(w => w.AccountId).Distinct().Count() }).ToArray();
                        break;
                    case "productions":
                        data = queryBase.Select(g => new { Date = g.Key, Value = (long)g.Select(w => w.ProductionId).Distinct().Count() }).ToArray();
                        break;
                    default:
                        throw new ArgumentException("Invalid dataField parameter.");
                }

                var result = new ChartResult();
                result.TemplateXml = ChartTemplates.SkyBlue;
                var chart = result.Chart;
                chart.Width = 600;
                chart.Height = 400;
                var area = chart.ChartAreas.Add("Series1");
                area.AxisX.Minimum = from.Value.ToOADate() - 0.5;
                area.AxisX.Maximum = to.Value.Date.ToOADate() + 0.5;
                area.AxisX.MajorGrid.Enabled = false;
                area.AxisX.IntervalType = DateTimeIntervalType.Days;
                if ((to.Value - from.Value).TotalDays <= 10)//this will force labels for every day, but after 10 (which during testing seemed the most), we can skip days.  without this, there would be duplicate labels per day
                    area.AxisX.Interval = 1;
                area.AxisX.LabelStyle.TruncatedLabels = true;
                var series = chart.Series.Add("Series1");
                series.ChartType = (to.Value - from.Value).TotalDays < 7 ? System.Web.UI.DataVisualization.Charting.SeriesChartType.Column : System.Web.UI.DataVisualization.Charting.SeriesChartType.Line;
                series.Points.DataBind(data, "Date", "Value", null);
                chart.Customize += (o, e) =>
                    {
                        foreach (var label in chart.ChartAreas[0].AxisY.CustomLabels)
                        {
                            if (dataToGraph.EqualsCaseSensitive("bandwidth"))
                                label.Text = new FileSize(label.Text).ToString();
                            else if (dataToGraph.EqualsCaseSensitive("duration"))
                            {
                                UpdateChartLabelsForTime(label);
                            }
                        }
                    };
                return result;
            }
        }



        /// <summary>
        /// Shows data related to videos that are uploaded and transcoded.
        /// </summary>
        /// <returns></returns>
        public ActionResult VideoUploads()
        {
            ViewData["SectionTitle"] = "Video Uploads/Encodes";
            return View();
        }

        /// <summary>
        /// Meant for testing error pages and error reporting.
        /// </summary>
        /// <returns></returns>
        public ActionResult ThrowException()
        {
            throw new Exception("Test exception.");
        }

        public ActionResult ResourceTracing()
        {
            return View(new Staff_ResourceTracing
            {
                ResourceTraceLevel = Request.Cookies[TraceableResourceManager.UserResourceTracingCookieName].MapIfNotNull(
                    c => (ResourceTraceLevel) Enum.Parse(typeof (ResourceTraceLevel), c.Value),
                    ResourceTraceLevel.NotSet)
            });
        }

        [HttpPost]
        public ActionResult ResourceTracing(Staff_ResourceTracing model)
        {
            var cookie = HttpContext.Request.Cookies[TraceableResourceManager.UserResourceTracingCookieName] ?? new HttpCookie(TraceableResourceManager.UserResourceTracingCookieName);
            cookie.Value = model.ResourceTraceLevel.ToString();
            cookie.Expires = DateTime.Now.AddYears(1);
            HttpContext.Response.Cookies.Set(cookie);
            HttpContext.Request.Cookies.Set(cookie);
            return View(model);
        }



        /// <summary>
        /// Gets an array of the available enum names, strongly typed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static T[] GetEnumValues<T>()
        {
            List<T> types = new List<T>();
            foreach (object currVal in System.Enum.GetValues(typeof(T)))
            {
                T enumVal = (T)currVal;
                types.Add(enumVal);
            }
            return types.ToArray();
        }


        /// <summary>
        /// Streams a chart to graph data of videos viewed.  Used by VideoWatches
        /// </summary>
        /// <param name="dataToGraph">The type of data that should be graphed.  Supported values are: "count","bandwidth","duration","users","accounts","inputAvgSize".</param>
        /// <param name="from">The starting date to plot from. </param>
        /// <param name="to">The ending date to plot to.</param>
        /// <returns></returns>
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.None)]
        public ActionResult VideoUploadsChart(string dataToGraph, DateTime? from, DateTime? to)
        {
            if (!dataToGraph.HasChars())
                dataToGraph = "count";
            dataToGraph = dataToGraph.Trim().ToLowerInvariant();
            if (!from.HasValue)
                from = DateTime.Now.Date;
            if (!to.HasValue || to.Value > DateTime.Now.Date)
                to = DateTime.Now.Date;

            from = from.Value.Date;
            using (var db = DataModel.ReadOnly)
            {
                var queryBase = db.EncodeLogs.Where(w => w.UploadCompleted != null && w.UploadCompleted.Value >= from.Value && w.UploadCompleted.Value < to.Value.Date.AddDays(1));
                switch (dataToGraph)
                {
                    case "inputavgsize":
                        queryBase = queryBase.Where(f => f.InputBytes > 0);
                        break;
                    case "inputavgduration":
                        queryBase = queryBase.Where(f => f.InputDuration != null && f.InputDuration > 0);
                        break;
                    case "inputavguploadduration":
                        queryBase = queryBase.Where(f => f.UploadDuration != null && f.UploadDuration.Value > 0);
                        break;
                }
                var query = queryBase.GroupBy(w => w.UploadCompleted.Value.Date).OrderBy(g => g.Key);
                IEnumerable data;

                switch (dataToGraph)
                {
                    case "count":
                        data = query.Select(g => new { Date = g.Key, Value = (long)g.Count() }).ToArray();
                        break;
                    case "bandwidth":
                        data = query.Select(g => new { Date = g.Key, Value = g.Sum(f => (long)f.InputBytes) }).ToArray();
                        break;
                    case "duration":
                        data = query.Select(g => new { Date = g.Key, Value = g.Sum(f => (long)f.InputDuration) }).ToArray();
                        break;
                    case "users":
                        data = query.Select(g => new { Date = g.Key, Value = (long)g.Select(w => w.UserId).Distinct().Count() }).ToArray();
                        break;
                    case "accounts":
                        data = query.Select(g => new { Date = g.Key, Value = (long)g.Select(w => w.AccountId).Distinct().Count() }).ToArray();
                        break;
                    case "inputavgsize":
                        data = query.Select(g => new { Date = g.Key, Value = g.Average(f => (long)f.InputBytes) }).ToArray();
                        break;
                    case "inputavgduration":
                        data = query.Select(g => new { Date = g.Key, Value = g.Average(f => (long)Math.Ceiling((double)f.InputDuration)) }).ToArray();
                        break;
                    case "inputavguploadduration":
                        data = query.Select(g => new { Date = g.Key, Value = g.Average(f => f.UploadDuration.GetValueOrDefault()) }).ToArray();
                        break;

                    default:
                        throw new ArgumentException("Invalid dataField parameter.");
                }

                var result = new ChartResult();
                result.TemplateXml = ChartTemplates.SkyBlue;
                var chart = result.Chart;
                chart.Width = 600;
                chart.Height = 400;
                var area = chart.ChartAreas.Add("Series1");
                area.AxisX.Minimum = from.Value.ToOADate() - 0.5;
                area.AxisX.Maximum = to.Value.Date.ToOADate() + 0.5;
                area.AxisX.MajorGrid.Enabled = false;
                area.AxisX.IntervalType = DateTimeIntervalType.Days;
                if ((to.Value - from.Value).TotalDays <= 10)//this will force labels for every day, but after 10 (which during testing seemed the most), we can skip days.  without this, there would be duplicate labels per day
                    area.AxisX.Interval = 1;
                area.AxisX.LabelStyle.TruncatedLabels = true;
                var series = chart.Series.Add("Series1");
                series.ChartType = (to.Value - from.Value).TotalDays < 7 ? System.Web.UI.DataVisualization.Charting.SeriesChartType.Column : System.Web.UI.DataVisualization.Charting.SeriesChartType.Line;
                series.Points.DataBind(data, "Date", "Value", null);
                chart.Customize += (o, e) =>
                {
                    foreach (var label in chart.ChartAreas[0].AxisY.CustomLabels)
                    {
                        if (dataToGraph.EqualsAny("bandwidth", "inputavgsize"))
                            label.Text = new FileSize(label.Text).ToString();
                        else if (dataToGraph.EqualsAny("duration", "inputavgduration", "inputavguploadduration"))
                        {
                            UpdateChartLabelsForTime(label);
                        }
                    }
                };
                return result;
            }
        }

        private static void UpdateChartLabelsForTime(CustomLabel label)
        {
            var time = TimeSpan.FromSeconds(double.Parse(label.Text));
            if (time.TotalHours >= 1)
                label.Text = Math.Round(time.TotalHours, 1).ToInvariant() + " hrs";
            else if (time.TotalMinutes >= 1)
                label.Text = Math.Round(time.TotalMinutes, 1).ToInvariant() + " min";
            else if (time.TotalSeconds >= 1)
                label.Text = Math.Round(time.TotalSeconds, 1).ToInvariant() + " sec";
        }


    }
}

