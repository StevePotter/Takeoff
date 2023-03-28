using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

using System.Web.Script.Serialization;

namespace Takeoff.Transcoder
{
    /// <summary>
    /// Created initially for executing server-side ASP.NET MVC actions.  This has built-in retry abilities and can automatically post and serialize back and forth with JSON.
    /// </summary>
    public class JsonPoster
    {
        public JsonPoster()
        {
        }

        public JsonPoster(string urlPrefix)
        {
            UrlPrefix = urlPrefix.EndWithout("/");
        }

        string UrlPrefix;

        public event EventHandler WebRequestCreated;

        public bool IsExecutingRequest { get; private set; }

        public WebRequest CurrentRequest
        {
            get;
            private set;
        }


        public string ResponseText
        {
            get;
            private set;
        }

        /// <summary>
        /// Indicates whether the requests are synchronous.
        /// </summary>
        private bool Asynchronous { get; set; }

        ///// <summary>
        ///// Gets the deserialized data returned from the request.  Available when Completed event fires.
        ///// </summary>
        //public object Data
        //{
        //    get;
        //    set; 
        //}

        /// <summary>
        /// Max time for a request is 2 minutes.
        /// </summary>
        private static TimeSpan Timeout = TimeSpan.FromMinutes(2);

        /// <summary>
        /// If the Response is a JSON string, this deserializes it into the target type.
        /// </summary>
        /// <returns></returns>
        public T DeserializeResponseJson<T>()
        {
            if (ResponseText.HasChars())
                return (new JavaScriptSerializer().Deserialize<T>(ResponseText));
            else
                return default(T);
        }


        public object DeserializeResponseJson()
        {
            if (ResponseText.HasChars())
                return (new JavaScriptSerializer().DeserializeObject(ResponseText));
            else
                return null;
        }

        public void PostJson(string relativeUrl)
        {
            PostJson(relativeUrl, null);
        }

        public void PostJson(string relativeUrl, object args)
        {
            PostJson(relativeUrl, args == null ? null : new JavaScriptSerializer().Serialize(args));
        }

        public void PostJson(string relativeUrl, string json)
        {
            ExecuteRequest(relativeUrl, "POST", "application/json;", json, null);
        }

        public T PostJson<T>(string relativeUrl)
        {
            return PostJson<T>(relativeUrl, null);
        }

        /// <summary>
        /// Does a POST to the given url with the data provided.  Data will be converted to JSON and included in the post body.  The result from the web request will be deserialized from JSON.
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public T PostJson<T>(string relativeUrl, object args)
        {
            string postData = args == null ? null : new JavaScriptSerializer { MaxJsonLength = int.MaxValue }.Serialize(args);
            ExecuteRequest(relativeUrl, "POST", "application/json;", postData, null);
            return DeserializeResponseJson<T>();
        }

 

        
        /// <summary>
        /// Does a GET request and returns the json-serialied object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="relativeUrl"></param>
        /// <returns></returns>
        public T GetJson<T>(string relativeUrl)
        {
            ExecuteRequest(relativeUrl, "GET", "application/json;", null, null);
            return DeserializeResponseJson<T>();
        }

        public void PostJsonAsync<T>(string relativeUrl, object args, Action<T> completedCallback)
        {
            Asynchronous = true;
            string postData = args == null ? null : new JavaScriptSerializer().Serialize(args);
            ExecuteRequest(relativeUrl, "POST", "application/json;", postData,
                () =>
                {
                    if (completedCallback != null)
                    {
                        completedCallback(DeserializeResponseJson<T>());
                    }
                });
        }


        public void Cancel()
        {
            if (CurrentRequest != null)
            {
                CurrentRequest.Abort();
            }
        }

        private string ExecuteRequest(string relativeUrl, string httpMethod, string contentType, string postData, Action successCallback)
        {
            if (IsExecutingRequest)
                throw new InvalidOperationException("Only one request at a time.");

            string url;
            if (UrlPrefix.HasChars())
            {
                if (relativeUrl.HasChars())
                    url = UrlPrefix + relativeUrl.StartWith(@"/");
                else
                    url = UrlPrefix;
            }
            else
            {
                url = relativeUrl;
            }

            var isPost = httpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase);
            IsExecutingRequest = true;

            try
            {
                CurrentRequest = WebRequest.Create(url);
                CurrentRequest.ContentType = contentType;
                CurrentRequest.Method = httpMethod;
                CurrentRequest.Timeout = (int)Timeout.TotalMilliseconds;
                if (WebRequestCreated != null)
                    WebRequestCreated(this, EventArgs.Empty);

                if (isPost)
                {
                    if (postData == null)
                    {
                        CurrentRequest.ContentLength = 0;
                    }
                    else
                    {
                        CurrentRequest.ContentLength = postData.Length;
                        StreamWriter stOut = new StreamWriter(CurrentRequest.GetRequestStream(), System.Text.Encoding.ASCII);
                        stOut.Write(postData);
                        stOut.Close();
                    }
                }
                if (Asynchronous)
                {
                    CurrentRequest.BeginGetResponse((result) =>
                    {
                        OnRequestCompleted(result);
                        if (successCallback != null)
                            successCallback();
                    }, null);
                    return null;
                }
                else
                {
                    var response = CurrentRequest.GetResponse();
                    ProcessResponse(response);
                    if (successCallback != null)
                        successCallback();
                    return ResponseText;
                }

            }
            catch (Exception ex)
            {
                IsExecutingRequest = false;
                CurrentRequest = null;
                throw new ApplicationException("Error while communicating with server.  This can be caused if:  you have a bad internet connection, don't have permission to perform an action, the server is undergoing maintainence, or there is a bug in this program.", ex);
            }
            finally
            {
            }
        }

        private void ProcessResponse(WebResponse response)
        {
            var body = new StringBuilder();
            string line;
            var webstream = new StreamReader(response.GetResponseStream());
            while ((line = webstream.ReadLine()) != null)
            {
                body.AppendLine(line);
            }
            CurrentRequest = null;
            IsExecutingRequest = false;
            ResponseText = body.ToString();
        }


        private void OnRequestCompleted(IAsyncResult result)
        {
            var response = CurrentRequest.EndGetResponse(result);
            ProcessResponse(response);
        }

    }

}
