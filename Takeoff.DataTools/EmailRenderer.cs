//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.IO;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Security.Principal;
//using System.Text;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Routing;
//using Moq;
//using Takeoff.App_Start;
//using Takeoff.Controllers;
//using Takeoff.Data;
//using Takeoff.Models;

//namespace Takeoff
//{
//    public class EmailRenderer
//    {
//        public EmailRenderer()
//        {
//        }

//        public string View
//        {
//            get;
//            set;
//        }

//        public object Model
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// Includes the entire rendered view, including html, plain body, and subject.  Parsed elsewhere.
//        /// </summary>
//        public string ViewSource
//        {
//            get;
//            set;
//        }


//        public virtual OutgoingMessage Render()
//        {
//            var messageId = Guid.NewGuid();
//            var controller = new EmailController();
//            controller.SetFakeControllerContext();
//            controller.ControllerContext.RouteData.Values["controller"] = "email";

//            var viewData = controller.ViewData;
//            viewData["IncludeOpenTracking"] = true;
//            viewData["MessageId"] = messageId;

//            if (Model != null)
//                viewData.Model = Model;
//            var result = new ViewResult { ViewName = View, ViewData = viewData };
//            result.ExecuteResult(controller.ControllerContext);
//            var writer = (StringWriter)controller.ControllerContext.HttpContext.Response.Output;
//            this.ViewSource = writer.GetStringBuilder().ToString();
//            var message = OutgoingMail.ParseViewOutput(messageId, ViewSource);
//            message.IncludedTrackingImage = true;
//            return message;
//        }

//    }


//    public static class MvcMockHelpers
//    {
//        public static HttpContextBase FakeHttpContext()
//        {
//            var context = new Mock<HttpContextBase>();
//            var request = new Mock<HttpRequestBase>();

//            request.SetupGet(x => x.ApplicationPath).Returns("/");
//            request.SetupGet(r => r.HttpMethod).Returns("GET");
//            request.SetupGet(x => x.Url).Returns(new Uri("http://localhost/", UriKind.Absolute));
//            request.SetupGet(x => x.ServerVariables).Returns(new System.Collections.Specialized.NameValueCollection());
//            request.SetupGet(x => x.Cookies).Returns(new HttpCookieCollection());

//            var browser = new Mock<HttpBrowserCapabilitiesBase>();
//            request.SetupGet(x => x.Browser).Returns(browser.Object);
//            browser.SetupGet(x => x.Browser).Returns("FireFox");
//            browser.SetupGet(x => x.IsMobileDevice).Returns(false);

//            var response = new Mock<HttpResponseBase>();
//            response.SetupGet(x => x.Cookies).Returns(new HttpCookieCollection());
//            var session = new Mock<HttpSessionStateBase>();
//            var server = new Mock<HttpServerUtilityBase>();
//            var items = new Hashtable();

//            TextWriter output = new StringWriter(new StringBuilder());

//            response.Setup(r => r.Output).Returns(output);
//            response.Setup(x => x.ApplyAppPathModifier(It.IsAny<string>())).Returns((string s) =>
//            {
//                return s.StartWithout(@"/");//quick fix to get relative urls so a web server doesn't ened to run to view the files
//            });

//            context.Setup(ctx => ctx.Request).Returns(request.Object);
//            context.Setup(ctx => ctx.Response).Returns(response.Object);
//            context.Setup(ctx => ctx.Session).Returns(session.Object);
//            context.Setup(ctx => ctx.Server).Returns(server.Object);
//            context.Setup(ctx => ctx.Items).Returns(items);
//            IPrincipal user = null;
//            context.Setup(ctx => ctx.User).Returns(user);//Mock.Of<IPrincipal>(u => u.Identity == Mock.Of<IIdentity>(i => i.Name == "1234")));
//            return context.Object;
//        }


//        //public static void SetFakeControllerContext(this Controller controller)
//        //{
//        //    var httpContext = FakeHttpContext();
//        //    var context = new ControllerContext(new RequestContext(httpContext, new RouteData()), controller);
//        //    controller.ControllerContext = context;
//        //}

//        /// <summary>
//        /// Sets the necessary stuff to fake a request url.
//        /// </summary>
//        /// <param name="request"></param>
//        /// <param name="url"></param>
//        public static void SetupRequestUrl(this HttpRequestBase request, string url)
//        {
//            if (url == null)
//                throw new ArgumentNullException("url");
//            if (!url.StartsWith("~/"))
//                throw new ArgumentException("Sorry, we expect a virtual url starting with \"~/\".");

//            var mock = Mock.Get(request);

//            mock.Setup(req => req.QueryString).Returns(GetQueryStringParameters(url));
//            mock.Setup(req => req.AppRelativeCurrentExecutionFilePath).Returns(GetUrlFileName(url));
//            mock.Setup(req => req.PathInfo).Returns(string.Empty);
//        }

//        static string GetUrlFileName(string url)
//        {
//            if (url.Contains("?"))
//                return url.Substring(0, url.IndexOf("?"));
//            else
//                return url;
//        }

//        static NameValueCollection GetQueryStringParameters(string url)
//        {
//            if (url.Contains("?"))
//            {
//                var parameters = new NameValueCollection();

//                string[] parts = url.Split("?".ToCharArray());
//                string[] keys = parts[1].Split("&".ToCharArray());

//                foreach (string key in keys)
//                {
//                    string[] part = key.Split("=".ToCharArray());
//                    parameters.Add(part[0], part[1]);
//                }

//                return parameters;
//            }
//            else
//            {
//                return null;
//            }
//        }



//    }



//    public class EmailUrlPrefixes : AppUrlPrefixes    
//    {
//        public EmailUrlPrefixes()
//        {
//            AbsoluteHttp = ApplicationEx.AppUrlPrefix.EndWith("/");
//            AbsoluteHttps = ApplicationEx.AppUrlPrefixSecure.EndWith("/");
//        }
//    }
  
//}
