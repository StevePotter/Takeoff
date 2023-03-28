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
    [SubController("/staff/outgoingmail", true)]
    public class Staff_OutgoingMailController : BasicController 
    {


        public ActionResult Index(DataTableParams dataTableParams)
        {
            using (var db = DataModel.ReadOnly)
            {
                var query = (from mail in db.OutgoingEmailLogs
                               select mail);

                if (this.Request.IsAjaxRequest())
                {
                    var response = new DataTableResponse();
                    if (dataTableParams.SortBy.HasItems())
                    {
                        query = query.OrderBy(string.Join(",", dataTableParams.SortBy));
                    }

                    response.iTotalRecords = query.Count();
                    response.iTotalDisplayRecords = response.iTotalRecords;

                    if (dataTableParams.DisplayLength > 0)
                    {
                        query = query.Skip(dataTableParams.DisplayStart).Take(dataTableParams.DisplayLength);
                    }
                    var data = query.ToArray();

                    response.sEcho = dataTableParams.Echo;

                    response.aaData = data.Select(record =>
                    {
                        
                        return new
                        {
                            record.Id,
                            record.ToAddress,
                            record.ToUserId,
                            record.Template,
                            SentOn = record.SentOn.GetValueOrDefault().ForJavascript(),
                            record.IncludedTrackingImage,
                            TimesOpened = record.OpenCount.GetValueOrDefault(0),
                            record.JobId,
                        };
                    }).ToArray();
                    return Json(response);
                }
            }
            return View();
        }

        public ActionResult Details(Guid id, string what)
        {
            return this.Redirect(OutgoingMail.MessageLogUrl(id, what.CharsOr("json")));
        }

    }


}