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
using System.ComponentModel;
using AutoMapper;

namespace Takeoff.Controllers
{
    [SpecialRestriction(SpecialRestriction.Staff)]
    [SubController("/staff/encodes", true)]
    public class Staff_EncodesController : BasicController
    {


        public ActionResult Index(DataTableParams dataTableParams)
        {
            using (var db = DataModel.ReadOnly)
            {
                var encodes = (from encodeLog in db.EncodeLogs
                               join videoThing in db.Things on encodeLog.InputId equals videoThing.Id
                               join video in db.Videos on videoThing.Id equals video.ThingId
                               join userThing in db.Things on encodeLog.UserId equals userThing.Id
                               join user in db.Users on userThing.Id equals user.ThingId
                               join productionThing in db.Things on videoThing.ParentId equals productionThing.Id
                               join production in db.Projects on productionThing.Id equals production.ThingId
                               select new
                               {
                                   encodeLog.AccountId,
                                   encodeLog.UserId,
                                   encodeLog.InputOriginalFileName,
                                   user.Email,
                                   encodeLog.UploadCompleted,
                                   encodeLog.EncodingCompleted,
                                   encodeLog.ErrorCode,
                                   encodeLog.JobCompleted,
                                   ProductionTitle = production.Title,
                                   ProductionId = production.ThingId,
                                   VideoTitle = video.Title,
                                   VideoId = video.ThingId,
                               });

                if (this.Request.IsAjaxRequest())
                {
                    var response = new DataTableResponse();
                    if (dataTableParams.SortBy.HasItems())
                    {
                        encodes = encodes.OrderBy(string.Join(",", dataTableParams.SortBy));
                    }

                    response.iTotalRecords = encodes.Count();
                    response.iTotalDisplayRecords = response.iTotalRecords;

                    if (dataTableParams.DisplayLength > 0)
                    {
                        encodes = encodes.Skip(dataTableParams.DisplayStart).Take(dataTableParams.DisplayLength);
                    }
                    var data = encodes.ToArray();

                    response.sEcho = dataTableParams.Echo;

                    response.aaData = data.Select(record =>
                    {
                        var location = new S3FileLocation { Bucket = ConfigUtil.GetRequiredAppSetting("TranscodeReportsBucket"), FileName = string.Format("{0}.xml", record.VideoId) };
                        
                        return new
                        {
                            record.AccountId,
                            record.UserId,
                            record.Email,
                            LogUrl = location.GetAuthorizedUrl(TimeSpan.FromHours(2)),
                            record.InputOriginalFileName,
                            UploadCompleted = record.UploadCompleted.GetValueOrDefault().ForJavascript(),
                            EncodingCompleted = record.EncodingCompleted.GetValueOrDefault().ForJavascript(),
                            Error = record.ErrorCode.HasValue ? true : false,
                            JobCompleted = record.JobCompleted.GetValueOrDefault().ForJavascript(),
                            record.ProductionTitle,
                            record.ProductionId,
                            record.VideoTitle,
                            record.VideoId,
                        };
                    }).ToArray();
                    return Json(response);
                }
            }
            return View();
        }



    }


}