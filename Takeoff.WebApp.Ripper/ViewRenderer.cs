using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using Takeoff.Controllers;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.WebApp.Ripper
{
    public class ViewRenderer
    {
        public ViewRenderer()
        {
        }

        public string ActionName
        {
            get;
            set;
        }

        public Type ControllerType { get; set;  }

        public string View
        {
            get;
            set;
        }

        public bool? AddViewNameToFileName { get; set; }

        public object Model
        {
            get;
            set;
        }

        /// <summary>
        /// Will add this to file name so you can have different flavors of a view, such as no data vs lots of data.
        /// </summary>
        public string Scenario
        {
            get;
            set;
        }

        public string FileName
        {
            get;
            set;
        }

        public string Description { get; set; }

        public bool IsSharedView
        {
            get;
            set;
        }

        /// <summary>
        /// When true, the file name will be controller_action.html.  
        /// </summary>
        public bool IsDefaultViewForAction { get; set; }

        public IUser User { get; set; }
        
        public Action<ViewDataDictionary> FillViewData { get; set; }

        public Action<Controller> OnRendering { get; set; }


        public virtual ViewRenderer Render() 
        {
            var controllerName = ControllerType.Name.EndWithout("Controller");
            ViewRenderingActionInvoker.Model = Model;
            ViewRenderingActionInvoker.ViewName = View;
            ViewRenderingActionInvoker.Result = null;
            ViewRenderingActionInvoker.ActionName = ActionName;
            ViewRenderingActionInvoker.ControllerName = controllerName;
            ViewRenderingActionInvoker.User = User;

            RouteTable.Routes.Insert(0,new GeneratedFileRoute(string.Empty, null));

            var controller = (Controller)Activator.CreateInstance(ControllerType);
            controller.SetFakeControllerContext();
            controller.ControllerContext.RouteData.Values["controller"] = controllerName;
            if (ActionName.HasChars())
                controller.ControllerContext.RouteData.Values["action"] = ActionName;

            var url = new UrlHelper(controller.ControllerContext.RequestContext).Action(ActionName, controllerName).StartWith(@"/").StartWith("~");
            controller.ControllerContext.HttpContext.Request.SetupRequestUrl(url);
            controller.Url = new UrlHelper(controller.ControllerContext.RequestContext);

            if (FillViewData != null)
                FillViewData(controller.ViewData);

            if (OnRendering != null)
                OnRendering(controller);

            controller.ActionInvoker.InvokeAction(controller.ControllerContext, ActionName.CharsOr("N/A"));

            var writer = (StringWriter)controller.ControllerContext.HttpContext.Response.Output;
            var html = writer.GetStringBuilder().ToString();

            RouteTable.Routes.RemoveAll(r => r is GeneratedFileRoute);

            html += "<!-- " + Environment.NewLine;
            string actionUrl;
            if ( ActionName.HasChars())
            {
                actionUrl =
                    new UrlHelper(controller.ControllerContext.RequestContext).Action(ActionName, controllerName).
                        StartWith(@"/");
                html += "Action Url: " + actionUrl + Environment.NewLine;
            }
            else
            {
                actionUrl = string.Empty;
            }
            html += "-->" + Environment.NewLine;

            string fileName = FileName;//don't set FileName so you can reuse renderers
            if (fileName.HasNoChars())
            {
                if (IsSharedView)
                    fileName = "Shared";
                else
                    fileName = controllerName;

                if (ActionName.HasChars())
                    fileName += "_" + ActionName;

                if (!AddViewNameToFileName.HasValue)
                    AddViewNameToFileName = !Scenario.HasChars();//a scenarion gets appended to the end of the file, so adding a view, by default, is unnecessary
                if (View.HasChars() && AddViewNameToFileName.Value && !IsDefaultViewForAction)
                    fileName += "_" + View;

                if (Scenario.HasChars())
                    fileName += "_" + Scenario;

                fileName += ".html";
            }

            SaveFile(html, Program.TempHtmlDirectory, fileName);

            return this;
        }

        protected virtual void SaveFile(string viewResult, string folder, string fileName)
        {
            File.WriteAllText(Path.Combine(folder, fileName), viewResult);
        }
    }

    public class ViewRenderer<TController>: ViewRenderer where TController : Controller
    {

        /// <summary>
        /// Use this or ActionName
        /// </summary>
        public Expression<Action<TController>> ActionExpr
        {
            get;
            set;
        }


        public override ViewRenderer Render()
        {
            if (ActionExpr != null && !ActionName.HasChars())
            {
                var methodCall = (MethodCallExpression)ActionExpr.Body;
                ActionName = GetActionName(methodCall.Method);
            }

            ControllerType = typeof (TController);

            return base.Render();
        }

        private static string GetActionName(MethodInfo action)
        {
            var customName = (ActionNameAttribute)Attribute.GetCustomAttribute(action, typeof(ActionNameAttribute));
            return customName == null ? action.Name : customName.Name;
        }

    }

    public class EmailRenderer: ViewRenderer<EmailController>
    {
        public EmailRenderer()
        {
        }

        protected override void SaveFile(string viewResult, string folder, string fileName)
        {
            var message = OutgoingMail.ParseViewOutput(Guid.NewGuid(), viewResult);
            var fileNoExt = Path.GetFileNameWithoutExtension(fileName).EndWithout(".");
            if (message.HtmlBody.HasChars())
                File.WriteAllText(Path.Combine(folder, fileNoExt + ".html"), message.HtmlBody);
            if (message.TextBody.HasChars())
                File.WriteAllText(Path.Combine(folder, fileNoExt + ".txt"), message.TextBody);
        }
    }

    public static class MvcMockHelpers
    {
        public static HttpContextBase FakeHttpContext()
        {
            Stopwatch st = Stopwatch.StartNew();
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();

            request.SetupGet(x => x.ApplicationPath).Returns("/");
            request.SetupGet(r => r.HttpMethod).Returns("GET");
            request.SetupGet(x => x.Url).Returns(new Uri("http://localhost/", UriKind.Absolute));
            request.SetupGet(x => x.ServerVariables).Returns(new System.Collections.Specialized.NameValueCollection());
            request.SetupGet(x => x.Cookies).Returns(new HttpCookieCollection());

            var browser = new Mock<HttpBrowserCapabilitiesBase>();
            request.SetupGet(x => x.Browser).Returns(browser.Object);
            browser.SetupGet(x => x.Browser).Returns("FireFox");
            browser.SetupGet(x => x.IsMobileDevice).Returns(false);

            var response = new Mock<HttpResponseBase>();
            response.SetupGet(x => x.Cookies).Returns(new HttpCookieCollection());
            var session = new Mock<HttpSessionStateBase>();
            var server = new Mock<HttpServerUtilityBase>();
            var items = new Hashtable();

            TextWriter output = new StringWriter(new StringBuilder());

            response.Setup(r => r.Output).Returns(output);
            response.Setup(x => x.ApplyAppPathModifier(It.IsAny<string>())).Returns((string s) =>
            {
                s = s.StartWithout(@"/");
                if (s.EndsWith(".html"))//for generated route urls.  this was necessary for RenderedViews in the takeoff.webapp.  Otherwise you could have action links like "../account_privacy.html", which ain't good
                {
                    return s;
                }

                //            request.SetupGet(x => x.ApplicationPath).Returns("../../Takeoff Dev/Code/trunk/Takeoff.WebApp/Assets/App3/");

                //                return Program.RunInWebServer ? s : s.StartWithout(@"/");//quick fix to get relative urls so a web server doesn't ened to run to view the files
                return s.StartWith(Program.AssetPrefix);
            });

            context.Setup(ctx => ctx.Request).Returns(request.Object);
            context.Setup(ctx => ctx.Response).Returns(response.Object);
            context.Setup(ctx => ctx.Session).Returns(session.Object);
            context.Setup(ctx => ctx.Server).Returns(server.Object);
            context.Setup(ctx => ctx.Items).Returns(items);
            IPrincipal user = null;
            context.Setup(ctx => ctx.User).Returns(user);//Mock.Of<IPrincipal>(u => u.Identity == Mock.Of<IIdentity>(i => i.Name == "1234")));

            st.Stop();
            return context.Object;
        }


        public static void SetFakeControllerContext(this Controller controller)
        {
            var httpContext = FakeHttpContext();
            var context = new ControllerContext(new RequestContext(httpContext, new RouteData()), controller);
            controller.ControllerContext = context;
        }

        /// <summary>
        /// Sets the necessary stuff to fake a request url.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="url"></param>
        public static void SetupRequestUrl(this HttpRequestBase request, string url)
        {
            if (url == null)
                throw new ArgumentNullException("url");
            if (!url.StartsWith("~/"))
                throw new ArgumentException("Sorry, we expect a virtual url starting with \"~/\".");

            var mock = Mock.Get(request);

            mock.Setup(req => req.QueryString).Returns(GetQueryStringParameters(url));
            mock.Setup(req => req.AppRelativeCurrentExecutionFilePath).Returns(GetUrlFileName(url));
            mock.Setup(req => req.PathInfo).Returns(string.Empty);
        }

        static string GetUrlFileName(string url)
        {
            if (url.Contains("?"))
                return url.Substring(0, url.IndexOf("?"));
            else
                return url;
        }

        static NameValueCollection GetQueryStringParameters(string url)
        {
            if (url.Contains("?"))
            {
                var parameters = new NameValueCollection();

                string[] parts = url.Split("?".ToCharArray());
                string[] keys = parts[1].Split("&".ToCharArray());

                foreach (string key in keys)
                {
                    string[] part = key.Split("=".ToCharArray());
                    parameters.Add(part[0], part[1]);
                }

                return parameters;
            }
            else
            {
                return null;
            }
        }



    }

  
}
