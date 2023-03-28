using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Net;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Web;
using System.IO;

namespace Takeoff.Transcoder.PandaStream
{
    /// <summary>
    /// Service Proxy for Panda Video API 
    /// </summary>
    public class PandaClient
    {
        #region Private Members
        private readonly PandaRequest _serviceRequest;
        private readonly string _cloudId;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _apiHost;
        private readonly int _apiPort = 80;
        private readonly int _apiVersion = 2;
        #endregion

        #region Ctors

        public PandaClient(string cloudId, string accessKey, string secretKey, string apiHost)
        {
            _cloudId = cloudId;
            _apiHost = apiHost;
            _secretKey = secretKey;
            _accessKey = accessKey;
            _serviceRequest = new PandaRequest();
        }

        #endregion

        #region Properties
        /// <summary>
        /// API Url and port (port number appended only if it is not port 80)
        /// </summary>
        private string ApiHostAndPort
        {
            get
            {
                return (_apiPort == 80) ? _apiHost : string.Format("{0}:{1}", _apiHost, _apiPort);
            }
        }

        /// <summary>
        /// API base path
        /// </summary>
        private string ApiBasePath
        {
            get { return "/v" + _apiVersion; }
        }

        /// <summary>
        /// Full API Url including protocol, host, port and base path
        /// </summary>
        public string ApiUrl
        {
            get { return string.Format("http://{0}{1}", ApiHostAndPort, ApiBasePath); }
        }

        /// <summary>
        /// Gets the underlying ServiceRequest object responsible for creating the HTTP request that will
        /// be sent to the Panda Service
        /// </summary>
        public PandaRequest ServiceRequest
        {
            get
            {
                return _serviceRequest;
            }
        }
        #endregion

        #region Request Methods

        public HttpWebResponse Get(string path, Dictionary<string, string> parameters)
        {
            return SendRequest("GET", path, parameters);
        }

        public HttpWebResponse Post(string path, Dictionary<string, string> parameters)
        {
            return Post(path, parameters, null, null);
        }

        public HttpWebResponse Post(string path, Dictionary<string, string> parameters,
            byte[] file, string fileName)
        {
            return SendRequest("POST", path, parameters, file, fileName);
        }

        public HttpWebResponse Put(string path, Dictionary<string, string> parameters)
        {
            return SendRequest("PUT", path, parameters);
        }

        public HttpWebResponse Delete(string path, Dictionary<string, string> parameters)
        {
            return SendRequest("DELETE", path, parameters);
        }

        #endregion

        #region JSON Request Methods

        public string GetJson(string path, Dictionary<string, string> parameters)
        {
            try
            {
                var response = Get(path, parameters);
                var json = GetResponseString(response);
                return json;
            }
            catch (WebException ex)
            {
                LogDebugInfo("Web Exception", ex.Message);
                var handler = OnWebException;
                if (handler != null) handler(ex);
            }
            return string.Empty;
        }

        public string PutJson(string path, Dictionary<string, string> parameters)
        {
            try
            {
                var response = Put(path, parameters);
                return GetResponseString(response);
            }
            catch (WebException ex)
            {
                LogDebugInfo("Web Exception", ex.Message);
                var handler = OnWebException;
                if (handler != null) handler(ex);
            }
            return string.Empty;
        }

        public string PostJson(string path, Dictionary<string, string> parameters)
        {
            return PostJson(path, parameters, null, null);
        }

        public string PostJson(string path, Dictionary<string, string> parameters,
            byte[] file, string fileName)
        {
            try
            {
                var response = Post(path, parameters, file, fileName);
                return GetResponseString(response);
            }
            catch (WebException ex)
            {
                LogDebugInfo("Web Exception", ex.Message);
                var handler = OnWebException;
                if (handler != null) handler(ex);
            }
            return string.Empty;
        }

        public string DeleteJson(string path, Dictionary<string, string> parameters)
        {
            try
            {
                var response = Delete(path, parameters);
                return GetResponseString(response);
            }
            catch (WebException ex)
            {
                LogDebugInfo("Web Exception", ex.Message);
                var handler = OnWebException;
                if (handler != null) handler(ex);
            }
            return string.Empty;
        }


        private static string GetResponseString(WebResponse response)
        {
            using (var stIn = new StreamReader(response.GetResponseStream()))
                return stIn.ReadToEnd();
        }

        #endregion

        #region Proxy Configuration Methods
        /// <summary>
        /// Will create a network proxy assigned to the underlying HTTP request using the supplied
        /// host, port and network credentials.
        /// </summary>
        /// <param name="webProxyHost">The proxy host</param>
        /// <param name="webProxyPort">The proxy port</param>
        /// <param name="username">The user name associated with the network credentials</param>
        /// <param name="password">The password associated with the network credentials</param>
        public void SetWebProxyCredentials(string webProxyHost, int webProxyPort,
            string username, string password)
        {
            // create the web proxy used by the http request when submitted
            _serviceRequest.Proxy =
                new WebProxy(webProxyHost, webProxyPort)
                {
                    Credentials = new NetworkCredential(username, password)
                };

            // The .NET framework will, by default, add headers to PUT and POST requests to expect a
            // 100 response code from the responding server. The combination of this header with the use
            // of a web proxy may cause Panda's servers to return status code 417 - Expectation Failed.
            // In order to prevent this, the additional header is removed.
            ServicePointManager.Expect100Continue = false;
        }
        #endregion

        #region Request Construction Methods

        /// <summary>
        /// Creates the HTTP request that will be sent to the Panda service.
        /// </summary>
        /// <param name="verb">HTTP request verb</param>
        /// <param name="path">HTTP request path</param>
        /// <param name="parameters">Parameters passed to the Panda service</param>
        /// <returns>Web request</returns>
        public PandaRequest BuildRequest(string verb, string path, Dictionary<string, string> parameters)
        {
            verb = verb.ToUpper();
            path = CanonicalPath(path);
            var timeStamp = DateTime.UtcNow;

            _serviceRequest.BaseUrl = ApiUrl + path;
            _serviceRequest.Verb = verb;
            _serviceRequest.SignedParameters = SignedQuery(verb, path, parameters, timeStamp);

            return _serviceRequest;
        }

        /// <summary>
        /// Generates a signature used for Panda service authentication. The request verb, host, 
        /// path and additional parameters are HMACSHA256 encoded using the supplied secret key
        /// </summary>
        /// <param name="verb">HTTP request verb</param>
        /// <param name="requestPath">HTTP request path</param>
        /// <param name="host">HTTP request host</param>
        /// <param name="secretKey">The secret key supplied by Panda when an account is first created</param>
        /// <param name="parameters">Parameters passed to the Panda service</param>
        /// <returns>The generated signature</returns>
        public string SignatureGenerator(string verb, string requestPath, string host, string secretKey, Dictionary<string, string> parameters)
        {
            var stringToSign = StringToSign(verb, requestPath, host, parameters);
            return EncodeStringToHMACSHA256(stringToSign, secretKey);
        }

        /// <summary>
        /// Generates the string value used to create the signature passed to the Panda services for 
        /// authentication.
        /// </summary>
        /// <param name="verb">HTTP request verb</param>
        /// <param name="requestPath">HTTP request path</param>
        /// <param name="host">HTTP request host</param>
        /// <param name="parameters">Parameters passed to the Panda service</param>
        /// <returns>The string used to generate the authetication signature</returns>
        public string StringToSign(string verb, string requestPath, string host, Dictionary<string, string> parameters)
        {
            // sort parameters alaphabetically and remove the 'file' parameter if supplied
            var sortedParameters = from item in parameters
                                   where item.Key != "file"
                                   orderby item.Key ascending
                                   select new KeyValuePair<string, string>(item.Key, item.Value);

            // encode parameters
            var querystring = EncodeToQuery(sortedParameters.ToDictionary(x => x.Key, x => x.Value));

            // build the string used to create the signature
            var stringToSign = string.Format("{0}\n{1}\n{2}\n{3}", verb.ToUpper(), host.ToLower(),
                requestPath, querystring);

            LogDebugInfo("StringToSign", stringToSign);
            return stringToSign;
        }

        public HttpWebResponse SendRequest(string verb, string path, Dictionary<string, string> parameters)
        {
            return SendRequest(verb, path, parameters, null, null);
        }

        public HttpWebResponse SendRequest(string verb, string path, Dictionary<string, string> parameters,
            byte[] file, string fileName)
        {
            var request = BuildRequest(verb, path, parameters);
            request.File = file;
            request.FileName = fileName;
            var response = request.Send();
            return response;
        }

        private string GenerateSignature(string verb, string requestPath, Dictionary<string, string> parameters)
        {
            var signature = SignatureGenerator(verb, requestPath, _apiHost, _secretKey, parameters);
            LogDebugInfo("Signature", signature);
            return signature;
        }

        private string SignedQuery(string verb, string requestPath, Dictionary<string, string> parameters, DateTime timeStamp)
        {
            var signedQuery = EncodeToQuery(SignedParams(verb, requestPath, parameters, timeStamp));
            LogDebugInfo("signedQuery", signedQuery);
            return signedQuery;
        }

        private Dictionary<string, string> SignedParams(string verb, string requestPath, Dictionary<string, string> parameters, DateTime timeStamp)
        {
            var authparams = new Dictionary<string, string>(parameters)
                                 {
                                     {"access_key", _accessKey},
                                     {"cloud_id", _cloudId},
                                     {"timestamp", GetPandaTimestamp(timeStamp)}
                                 };

            LogDebugInfo("timestamp", authparams["timestamp"]);
            authparams.Add("signature", GenerateSignature(verb, requestPath, authparams));

            return authparams;
        }

        private static string CanonicalPath(string path)
        {
            return "/" + path.Trim(' ', '\t', '\n', '\r', '\0');
        }

        private static string EncodeToQuery(Dictionary<string, string> parameters)
        {
            var items = new Collection<string>();
            if (parameters != null)
                foreach (var keyValuePair in parameters)
                {
                    if (keyValuePair.Key == "timestamp")
                    {
                        items.Add(keyValuePair.Key + "=" + HttpUtility.UrlEncode(keyValuePair.Value).ToUpper());
                        continue;
                    }
                    if (keyValuePair.Key == "file")
                    {
                        items.Add(keyValuePair.Key + "=" + keyValuePair.Value);
                        continue;
                    }
                    items.Add(keyValuePair.Key + "=" + UpperCaseUrlEncode(keyValuePair.Value));
                }
            return string.Join("&", items.ToArray());
        }

        private static string UpperCaseUrlEncode(string s)
        {
            char[] temp = HttpUtility.UrlEncode(s).ToCharArray();
            for (int i = 0; i < temp.Length - 2; i++)
            {
                if (temp[i] == '%')
                {
                    temp[i + 1] = char.ToUpper(temp[i + 1]);
                    temp[i + 2] = char.ToUpper(temp[i + 2]);
                }
            }
            return new string(temp);
        }

        #endregion

        #region Debugging Events

        public delegate void DebugEventHandler(string title, string message);
        public event DebugEventHandler OnDebugEvent;

        public delegate void WebExceptionHandler(WebException exception);
        public event WebExceptionHandler OnWebException;

        private void LogDebugInfo(string title, string info)
        {
            var handler = OnDebugEvent;
            if (handler != null) handler(title, info);
        }

        #endregion

        #region Util

        /// <summary>
        /// Encodes a string into a hash using HMACSHA256
        /// </summary>
        /// <param name="stringToSign"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public string EncodeStringToHMACSHA256(string stringToSign, string secretKey)
        {
            var encoding = new ASCIIEncoding();

            var keyByte = encoding.GetBytes(secretKey);
            var hmacsha256 = new HMACSHA256(keyByte);
            var messageBytes = encoding.GetBytes(stringToSign);
            var hashmessage = hmacsha256.ComputeHash(messageBytes);
            var signature = Convert.ToBase64String(hashmessage);

            return signature;
        }

        /// <summary>
        /// Gets the current time formatted as required for Panda Video signing
        /// </summary>
        /// <returns></returns>
        public string GetPandaTimestamp()
        {
            return GetPandaTimestamp(DateTime.Now);
        }

        /// <summary>
        /// Returns the supplied time formatted as required for Panda Video signing
        /// </summary>
        /// <returns></returns>
        public string GetPandaTimestamp(DateTime time)
        {
            return time.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00");
        }

        #endregion
    }

    public class PandaRequest
    {
        /// <summary>
        /// The base url path of the panda service
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// The HTTP request verb
        /// </summary>
        public string Verb { get; set; }

        /// <summary>
        /// The signed parameters passed to the Panda service. Please ensure that all required parameters
        /// (i.e. access_key, signature...) are supplied.
        /// </summary>
        public string SignedParameters { get; set; }

        /// <summary>
        /// The name of the file to upload to the Panda service
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The content (in bytes) of the file to upload to the Panda service
        /// </summary>
        public byte[] File { get; set; }

        /// <summary>
        /// A flag indicating whether or not a file will be posted to the panda service
        /// </summary>
        public bool IsFilePosted
        {
            get
            {
                return File != null && !string.IsNullOrEmpty(FileName);
            }
        }

        /// <summary>
        /// Will validate the supplied data to ensure that all required data for a panda
        /// service call was supplied.
        /// </summary>
        /// <param name="brokenRulesMessage">Messages associated with any and all broken rules</param>
        /// <returns>A flag indicating whther or not the request is valid for submission</returns>
        public bool IsValidForSubmission(out string brokenRulesMessage)
        {
            brokenRulesMessage = string.Empty;

            if (string.IsNullOrEmpty(BaseUrl))
                brokenRulesMessage += "A BaseUrl is required to submit a panda service request. ";
            if (string.IsNullOrEmpty(Verb))
                brokenRulesMessage += "A Verb is required to submit a panda service request. ";
            if (string.IsNullOrEmpty(SignedParameters))
                brokenRulesMessage += "SignedParameters are required to submit a panda service request. ";

            // if any file related data is supplied, ensure that all required file-related data exists
            if (File != null || !string.IsNullOrEmpty(FileName))
            {
                if (File == null || string.IsNullOrEmpty(FileName))
                    brokenRulesMessage += "Both File and FileName are required to submit a file to a panda service request. ";
            }

            return string.IsNullOrEmpty(brokenRulesMessage);
        }

        /// <summary>
        /// A flag indicating whther or not the request submits associated data (such as an HTTP POST or PUT)
        /// </summary>
        public bool HasDataToSend
        {
            get { return (Verb == "POST" || Verb == "PUT"); }
        }

        /// <summary>
        /// The full url of the Panda service
        /// </summary>
        public string Url
        {
            get
            {
                // if request does not send data, append signed parameters to the url
                return (this.HasDataToSend) ? BaseUrl : string.Format("{0}?{1}", BaseUrl, SignedParameters);
            }
        }

        /// <summary>
        /// If a network proxy is defined, it will be assigned to the underlying HTTP request. This property
        /// is usually utilized for allow for network credentials to be passed along with the HTTP request
        /// to the panda service.
        /// </summary>
        public IWebProxy Proxy { get; set; }

        private NameValueCollection _parameters;
        private NameValueCollection Parameters
        {
            get
            {
                if (_parameters == null)
                    _parameters = System.Web.HttpUtility.ParseQueryString(SignedParameters);
                return _parameters;
            }
        }

        /// <summary>
        /// Creates the HTTP request that will be sent to the Panda service.
        /// </summary>
        /// <returns>Web request</returns>
        public WebRequest CreateWebRequest()
        {
            string brokenRulesMessage;
            if (!this.IsValidForSubmission(out brokenRulesMessage))
                throw new ArgumentException(brokenRulesMessage);

            if (this.IsFilePosted)
                return CreateMultipartFormRequest();
            else
                return CreateFormUrlEncodedRequest();
        }

        private WebRequest CreateFormUrlEncodedRequest()
        {
            var request = WebRequest.Create(new Uri(Url));
            if (Proxy != null)
                request.Proxy = Proxy;
            request.Method = Verb;

            // if request must data to send, write the data to the request's content stream
            if (this.HasDataToSend)
            {
                var byteArray = Encoding.UTF8.GetBytes(SignedParameters);
                request.ContentLength = byteArray.Length;
                request.ContentType = "application/x-www-form-urlencoded";

                var requestStream = request.GetRequestStream();
                requestStream.Write(byteArray, 0, byteArray.Length);
                requestStream.Close();
            }

            return request;
        }

        private WebRequest CreateMultipartFormRequest()
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            // instantiate the request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            if (Proxy != null)
                request.Proxy = Proxy;
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = System.Net.CredentialCache.DefaultCredentials;

            System.IO.Stream requestStream = request.GetRequestStream();

            // write each of the supplied parameters to the request's stream
            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in Parameters.Keys)
            {
                requestStream.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, Parameters[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                requestStream.Write(formitembytes, 0, formitembytes.Length);
            }
            requestStream.Write(boundarybytes, 0, boundarybytes.Length);

            // write the file contents and meta to the request's stream
            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, "file", FileName, "application/octet-stream");
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            requestStream.Write(headerbytes, 0, headerbytes.Length);
            requestStream.Write(File, 0, File.Length);

            // close and return request
            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            requestStream.Write(trailer, 0, trailer.Length);
            requestStream.Close();
            return request;
        }

        /// <summary>
        /// Submits a request to the Panda service
        /// </summary>
        /// <returns>The HttpWebResponse from the Panda service</returns>
        public HttpWebResponse Send()
        {
            var request = CreateWebRequest();
            var response = (HttpWebResponse)request.GetResponse();
            return response;
        }
    }


}
