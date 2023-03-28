using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Newtonsoft.Json;
using Takeoff.Models;

namespace Takeoff
{
    public static class LogServerRequestsHelper
    {
        public static Guid? RequestLogId(this Controller controller)
        {
            if (controller == null || controller.HttpContext == null)
                return new Guid?();
            return LogId(controller.HttpContext);
        }

        ///// <summary>
        ///// When called, this returns the id of the current request log.  If the requ
        ///// </summary>
        ///// <param name="controller"></param>
        ///// <returns></returns>
        //public static Guid RequestLogIdEnsured(this Controller controller)
        //{
        //    if (controller == null || controller.HttpContext == null)
        //        return new Guid?();
        //    return LogId(controller.HttpContext);
        //}

        
        public static Guid? LogId(this HttpContextBase context)
        {
            var value = context.Items[ContextRequestLogIdKey];
            if (value == null)
                return new Guid?();
            return (Guid)value;
        }

        /// <summary>
        /// Marks the request as being logged.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Guid StartLogging(this HttpContextBase context, RouteData routeData, bool logRequest, bool queueRequestReportSaveNow, bool logResponse)
        {
            if (context.Items.Contains(ContextRequestLogIdKey))
                throw new InvalidOperationException("Already logging.");
            var response = context.Response;
            var id = Guid.NewGuid();

            context.Defer(() => ServerRequestLogging.SaveRequestToDb(id, context.RequestDate(), logRequest, logResponse, context.UserIdOrDefault(), context.Request.Url.LocalPath));
            
            context.Items[ContextRequestLogIdKey] = id;
            response.AddHeader("x-to-logid", id.ToString());
            if (logRequest)
            {
                var report = CreateRequestReport(context, routeData);
                context.Items[ContextRequestReportKey] = report;
                if (queueRequestReportSaveNow)
                {
                    context.Defer(() => ServerRequestLogging.SaveRequestDetails(id, JsonConvert.SerializeObject(report), @"application/json"));
                }
            }
            if (logResponse)
            {
                var responseReport = new Dictionary<string, object>();
                context.Items[ContextResponseReportKey] = responseReport;
                response.Filter = new ResponseLoggingFilter(response.Filter, id, response, responseReport);
            }
            return id;
        }

        private const string ContextRequestLogIdKey = "_requestLogId";
        private const string ContextRequestReportKey = "_requestReportId";
        private const string ContextResponseReportKey = "_responseReportId";

        public static Dictionary<string, object> RequestReport(this HttpContextBase context)
        {
            return (Dictionary<string, object>)context.Items[ContextRequestReportKey];
        }

        private static Dictionary<string, object> CreateRequestReport(HttpContextBase context, RouteData routeData)
        {
            var request = context.Request;
            var report = new Dictionary<string, object>();
            report["DateUtc"] = context.RequestDate();
            report["DateLocal"] = context.RequestDate().ToLocalTime();
            try
            {
                var user = context.UserThing2();
                if (user != null)
                {
                    report["UserId"] = user.Id;
                    report["UserEmail"] = user.Email;
                    report["UserName"] = user.DisplayName;
                }
            }
            catch //just in case
            {
            }

            if (routeData != null)
            {
                var routeDataReport = new Dictionary<string, object>();
                report["RouteData"] = routeDataReport;
                foreach (var pair in routeData.Values)
                {
                    routeDataReport.Add(pair.Key.CharsOrEmpty(), pair.Value ?? "NULL");
                }
            }

            report["Url"] = context.Request.Url.MapIfNotNull(u => u.AbsoluteUri);
            report["UserAgent"] = context.Request.UserAgent;
            report["HttpMethod"] = request.HttpMethod;
            report["IP"] = request.UserHostAddress;
            report["DNS"] = request.UserHostName;
            report["Headers"] = request.ServerVariables["ALL_RAW"];
            report["Referrer"] = request.UrlReferrer == null ? "None" : request.UrlReferrer.ToString();

            if (request.InputStream.Length > 0)
            {
                var originalPosition = request.InputStream.Position;
                request.InputStream.Position = 0;
                var sr = new StreamReader(request.InputStream); //don't dispose or else input stream will be disposed too
                string body = sr.ReadToEnd().CharsOrEmpty().Truncate(1000000, StringTruncating.Character);
                    //just in case it's super long
                request.InputStream.Position = originalPosition;
                report["Body"] = body;
            }
            return report;
        }

        public static Dictionary<string, object> ResponseReport(this HttpContextBase context)
        {
            return (Dictionary<string, object>)context.Items[ContextResponseReportKey];
        }

        public class ResponseLoggingFilter : MemoryStream
        {
            private Stream underlyingStream;
            private HttpResponseBase response;
            private Guid logId;
            private Dictionary<string, object> report;

            public ResponseLoggingFilter(Stream s, Guid logId, HttpResponseBase response, Dictionary<string, object> report)
            {
                this.response = response;
                this.underlyingStream = s;
                this.logId = logId;
                this.report = report;
            }

            public override bool CanRead
            {
                get { return underlyingStream.CanRead; }
            }

            public override bool CanSeek
            {
                get { return underlyingStream.CanSeek; }
            }

            public override bool CanWrite
            {
                get { return underlyingStream.CanWrite; }
            }

            public override void Flush()
            {
                underlyingStream.Flush();
                base.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                underlyingStream.Read(buffer, offset, count);
                return base.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                underlyingStream.Seek(offset, origin);
                return base.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                underlyingStream.SetLength(value);
                base.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                underlyingStream.Write(buffer, offset, count);
                base.Write(buffer, offset, count);
            }

            public override void Close()
            {
                try
                {
                    FillReport();

                    //send msmq message (action filter might have already sent them on resultexecuted)
                    Expression<Action> invokeParams = () => ServerRequestLogging.SaveResponseDetails(logId, JsonConvert.SerializeObject(report), @"application/json");
                    QueueDeferredMethodInvokesAttribute.SendMessagesToInvoke(new[] { invokeParams });
                }
                catch (Exception)//just in case, don't crash the response
                {
                }
                underlyingStream.Close();
                base.Close();
            }


            private void FillReport()
            {
                report["StatusCode"] = response.StatusCode;
                report["StatusDescription"] = response.StatusDescription;
                //headers cannot be accessed in dev server, hence the try catch.   iis handles it fine
                try
                {
                    report["Headers"] = string.Join(Environment.NewLine, response.Headers.AllKeys.Select(header => "{0}:{1}".FormatString(header, response.Headers[header].CharsOrEmpty())));
                }
                catch (Exception)
                {
                }
                var bodyBytes = this.ToArray();
                if ( bodyBytes.HasItems())
                {
                    report["Body"] = System.Text.Encoding.UTF8.GetString(this.ToArray());
                }
            }

            protected override void Dispose(bool disposing)
            {
                underlyingStream.Dispose();
                base.Dispose(disposing);
                this.response = null;
                this.underlyingStream = null;
            }
        }

    }

}