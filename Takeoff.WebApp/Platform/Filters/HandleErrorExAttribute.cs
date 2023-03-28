using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Takeoff.Controllers;
using System.Net.Mail;
using System.Configuration;
using System.Web.Script.Serialization;
using System.IO;
using Takeoff;
using Takeoff.Models;

namespace Takeoff
{

    /// <summary>
    /// An extended version of HandleError that gives nice error reports in ajax requests.
    /// </summary>
    /// <remarks>
    /// MVC attributes not good with ajax requests: http://stackoverflow.com/questions/1609677/asp-net-mvc-handleerror-authorize-with-jsonresult
    /// 
    /// This is also a nice place to create an exception report, log it, and notify us.
    /// 
    /// To get this working, in web config add:     <customErrors mode="On" />
    /// 
    /// To avoid wrapping exceptions just to get a different view, there is a trick:  Just set ViewData["ExceptionView"] and it'll use that view instead of the default one set in the metadata.
    /// 
    /// </remarks>
    public class HandleErrorExAttribute : HandleErrorAttribute
    {
        public HandleErrorExAttribute()
        {
            SendEmail = true;
            AddEventLogEntry = true;
        }

        public bool SendEmail { get; set; }
        public bool AddEventLogEntry { get; set; }

        /// <summary>
        /// The view that will be used if the request is an ajax one.
        /// </summary>
        public string ViewForAjax
        {
            get
            {
                if (string.IsNullOrEmpty(_viewForAjax))
                    return "ErrorAjax";
                else
                    return _viewForAjax;
            }
            set
            {
                _viewForAjax = value;
            }
        }
        private string _viewForAjax;

        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
                return;

            if (filterContext.Controller != null && filterContext.Controller.ViewData.ContainsKey("ExceptionView"))
            {
                View = (string)filterContext.Controller.ViewData["ExceptionView"];
            }

            //swap out the View property if we're in ajax, then return to normal
            string oldView = filterContext.HttpContext.Request.IsAjaxRequest() ? View : null;

            if (oldView != null)
                View = ViewForAjax;

            base.OnException(filterContext);

            if (oldView != null)
                View = oldView;

            if (filterContext.ExceptionHandled && (SendEmail || AddEventLogEntry))
            {
                string report;
                try
                {
                    report = GenerateReport(filterContext);
                }
                catch ( Exception ex )
                {
                    report = new JavaScriptSerializer().Serialize(new string[] { "Error", "Could not generate report: " + ex.Report() });
                }
                ReportException(report, SendEmail, AddEventLogEntry);
            }
        }

        private static string GenerateReport(ExceptionContext filterContext)
        {
            List<string> vals = new List<string>();
            Action<string, string> addField = (name, val) =>
            {
                vals.Add(name);
                vals.Add(val);
            };
            var request = filterContext.HttpContext.Request;
            addField("Date (utc)", DateTime.UtcNow.ToString(DateTimeFormat.LongDateTime));
            addField("Date (server local)", DateTime.Now.ToString(DateTimeFormat.LongDateTime));
            try
            {
                var user = filterContext.HttpContext.UserThing();
                if (user == null)
                {

                    addField("User", "None");
                }
                else
                {
                    addField("User", user.DisplayName.CharsOrEmpty() + ", " + user.Email.CharsOrEmpty() + ", " + user.Id.ToInvariant());
                }
            }
            catch
            {
                addField("User", "EXCEPTION WHILE GETTING USER");
            }


            var parameters = new StringBuilder();
            for (int i = 0; i < request.QueryString.Count; i++)
            {
                parameters.AppendFormat("Key={0}, Value={1}  \n", request.QueryString.Keys[i].CharsOrEmpty(), request.QueryString[i].CharsOrEmpty());
            }
            addField("Query string params", parameters.ToString());
            parameters.Clear();
            for (int i = 0; i < request.Form.Count; i++)
            {
                parameters.AppendFormat("Key={0}, Value={1}  \n", request.Form.Keys[i].CharsOrEmpty(), request.Form[i].CharsOrEmpty());
            }
            addField("Form params", parameters.ToString());

            if (filterContext.RouteData.Values.Count > 0)
            {
                parameters.Clear();
                foreach (KeyValuePair<string, object> pair in filterContext.RouteData.Values)
                {
                    parameters.AppendFormat("Key={0}, Value={1}   \n", pair.Key.CharsOrEmpty(), pair.Value == null ? "NULL" : pair.Value.ToString().CharsOrEmpty());
                }
                addField("Route Data", parameters.ToString());
            }

            addField("Url", filterContext.HttpContext.Request.Url.ToString());
            addField("HttpMethod", request.HttpMethod);
            addField("IP", request.UserHostAddress);
            addField("DNS", request.UserHostName);
            addField("HTTP Headers", request.ServerVariables["ALL_HTTP"]);
            try//this started happening occasionally, so had to put try/catch to know more
            {
                addField("Referrer", request.UrlReferrer == null ? "None" : request.UrlReferrer.ToString());
            }
            catch (UriFormatException ex)
            {
                //Message: Invalid URI: The Authority/Host could not be parsed.
                //Stack Trace:    at System.Uri.CreateThis(String uri, Boolean dontEscape, UriKind uriKind)
                //kept happening 
                addField("Referrer", "UNABLE TO PARSE!");
            }
            addField("Exception", filterContext.Exception.Report());

            if (request.InputStream.Length > 0)
            {
                var originalPosition = request.InputStream.Position;
                request.InputStream.Position = 0;
                var sr = new StreamReader(request.InputStream);//don't dispose or else input stream will be disposed too
                string body = sr.ReadToEnd().CharsOrEmpty().Truncate(5000, StringTruncating.Character);//take only the first 5000 letters so we don't have a massive email
                addField("Request Body", body);
                request.InputStream.Position = originalPosition;
            }

            var serialized = new JavaScriptSerializer().Serialize(vals.ToArray());
            return serialized;
        }


        /// <summary>
        /// Sends an email and/or writes an event log entry that details the exception report.  Note that this method is designed NOT to throw an exception, or else it could cause an infinite loop (causing endless mule requests)
        /// </summary>
        /// <param name="reportInJson"></param>
        /// <param name="email"></param>
        /// <param name="eventLog"></param>
        public static void ReportException(string reportInJson, bool email, bool eventLog)
        {
            string[] fields;
            try
            {
                fields = ((object[])new JavaScriptSerializer().DeserializeObject(reportInJson)).Select(f => f == null ? String.Empty : Convert.ToString(f)).ToArray();//avoid null references
                if (!fields.HasItems())
                    return;
            }
            catch
            {
                return;//lame but if the input is bad, an infinite exception could occur.
            }

            if (email)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    for (var i = 0; i < fields.Length - 1; i += 2)
                    {
                        sb.AppendLine("--------------" + fields[i] + "--------------").AppendLine(fields[i + 1]).AppendLine();
                    }
                    var message = new OutgoingMessage
                                      {
                                          To = ConfigUtil.GetRequiredAppSetting("SendErrorEmailsTo"),
                                          Subject = "Takeoff Exception",
                                          TextBody = sb.ToString(),
                                          Template = "Exception"
                                      };
                    OutgoingMail.SendNow(message);
                }
                catch
                {
                }
            }

            if (eventLog)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    for (var i = 0; i < fields.Length - 1; i += 2)
                    {
                        sb.AppendLine("--------------" + fields[i] + "--------------").AppendLine(fields[i + 1]).AppendLine();
                    }
                    if ( !System.Diagnostics.EventLog.SourceExists("Takeoff"))
                    {
                        System.Diagnostics.EventLog.CreateEventSource("Takeoff","Application");
                    }
                    System.Diagnostics.EventLog.WriteEntry("Takeoff",sb.ToString());
                }
                catch
                {
                }
            }

        }
    
    }
}
