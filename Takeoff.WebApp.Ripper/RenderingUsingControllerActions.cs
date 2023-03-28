//this will help you when you decide to run actual controller actions and let them return the proper
//result, which can then be rendered.  this is best because it also uses the action's limited biz logic
//however it requires full repo mocking which we don't have right now
//





//public static class MvcMockHelpers
//{
//    public static HttpContextBase FakeHttpContext()
//    {
//        var context = new Mock<HttpContextBase>();
//        var request = new Mock<HttpRequestBase>();

//        request.SetupGet(x => x.ApplicationPath).Returns("/");
//        request.SetupGet(r => r.HttpMethod).Returns("GET");
//        request.SetupGet(x => x.Url).Returns(new Uri("http://localhost/a", UriKind.Absolute));
//        request.SetupGet(x => x.ServerVariables).Returns(new System.Collections.Specialized.NameValueCollection());
//        request.SetupGet(x => x.Cookies).Returns(new HttpCookieCollection());

//        var browser = new Mock<HttpBrowserCapabilitiesBase>();
//        request.SetupGet(x => x.Browser).Returns(browser.Object);
//        browser.SetupGet(x => x.Browser).Returns("FireFox");
//        browser.SetupGet(x => x.IsMobileDevice).Returns(false);

//        var response = new Mock<HttpResponseBase>();
//        response.SetupGet(x => x.Cookies).Returns(new HttpCookieCollection());
//        var session = new Mock<HttpSessionStateBase>();
//        var server = new Mock<HttpServerUtilityBase>();
//        var items = new Hashtable();

//        TextWriter output = new StringWriter(new StringBuilder());

//        response.Setup(r => r.Output).Returns(output);
//        response.Setup(x => x.ApplyAppPathModifier(It.IsAny<string>())).Returns((string s) =>
//        {
//            return Program.RunInWebServer ? s : s.StartWithout(@"/");//quick fix to get relative urls so a web server doesn't ened to run to view the files
//        });

//        context.Setup(ctx => ctx.Request).Returns(request.Object);
//        context.Setup(ctx => ctx.Response).Returns(response.Object);
//        context.Setup(ctx => ctx.Session).Returns(session.Object);
//        context.Setup(ctx => ctx.Server).Returns(server.Object);
//        context.Setup(ctx => ctx.Items).Returns(items);
//        IPrincipal user = null;
//        context.Setup(ctx => ctx.User).Returns(user);//Mock.Of<IPrincipal>(u => u.Identity == Mock.Of<IIdentity>(i => i.Name == "1234")));
//        return context.Object;
//    }


//    public static void SetFakeControllerContext(this Controller controller)
//    {
//        var httpContext = FakeHttpContext();
//        var context = new ControllerContext(new RequestContext(httpContext, new RouteData()), controller);
//        controller.ControllerContext = context;
//    }

//    /// <summary>
//    /// Sets the necessary stuff to fake a request url.
//    /// </summary>
//    /// <param name="request"></param>
//    /// <param name="url"></param>
//    public static void SetupRequestUrl(this HttpRequestBase request, string url)
//    {
//        if (url == null)
//            throw new ArgumentNullException("url");
//        if (!url.StartsWith("~/"))
//            throw new ArgumentException("Sorry, we expect a virtual url starting with \"~/\".");

//        var mock = Mock.Get(request);

//        mock.Setup(req => req.QueryString).Returns(GetQueryStringParameters(url));
//        mock.Setup(req => req.AppRelativeCurrentExecutionFilePath).Returns(GetUrlFileName(url));
//        mock.Setup(req => req.PathInfo).Returns(string.Empty);
//    }

//    static string GetUrlFileName(string url)
//    {
//        if (url.Contains("?"))
//            return url.Substring(0, url.IndexOf("?"));
//        else
//            return url;
//    }

//    static NameValueCollection GetQueryStringParameters(string url)
//    {
//        if (url.Contains("?"))
//        {
//            var parameters = new NameValueCollection();

//            string[] parts = url.Split("?".ToCharArray());
//            string[] keys = parts[1].Split("&".ToCharArray());

//            foreach (string key in keys)
//            {
//                string[] part = key.Split("=".ToCharArray());
//                parameters.Add(part[0], part[1]);
//            }

//            return parameters;
//        }
//        else
//        {
//            return null;
//        }
//    }



//}

  








//        public static void RenderView<TController>(string action, string view, object model) where TController : Controller
//        {
//            RenderView<TController>(new RenderViewParams<TController> { ActionName = action, Model = model, View = view });
//        }

//        public static void RenderView<TController>(RenderViewParams<TController> args) where TController : Controller
//        {
//            if (args.ActionExpr != null && !args.ActionName.HasChars())
//            {
//                var methodCall = (MethodCallExpression)args.ActionExpr.Body;
//                args.ActionName = ActionName(methodCall.Method);
//            }
//            var controllerName = ControllerName(typeof(TController));
//            ViewRenderingActionInvoker.Model = args.Model;
//            ViewRenderingActionInvoker.ViewName = args.View;
//            ViewRenderingActionInvoker.Result = null;
//            ViewRenderingActionInvoker.ActionName = args.ActionName;
//            ViewRenderingActionInvoker.ControllerName = controllerName;
//            ViewRenderingActionInvoker.User = args.User;

//            var controller = Activator.CreateInstance<TController>();
//            controller.SetFakeControllerContext();
//            controller.ControllerContext.RouteData.Values["controller"] = controllerName;
//            if ( args.ActionName.HasChars())
//                controller.ControllerContext.RouteData.Values["action"] = args.ActionName;

//            var url = new UrlHelper(controller.ControllerContext.RequestContext).Action(args.ActionName, ControllerName(controller.GetType())).StartWith(@"/").StartWith("~");
//            controller.ControllerContext.HttpContext.Request.SetupRequestUrl(url);
//            controller.Url = new UrlHelper(controller.ControllerContext.RequestContext);
//            controller.ActionInvoker.InvokeAction(controller.ControllerContext, args.ActionName.CharsOr("N/A"));

//            var writer = (StringWriter)controller.ControllerContext.HttpContext.Response.Output;
//            var html = writer.GetStringBuilder().ToString();

//            if ( args.FileName.HasNoChars())
//            {
//                if (args.IsSharedView)
//                    args.FileName = "Shared";
//                else
//                    args.FileName = controllerName;

//                if ( args.ActionName.HasChars())
//                    args.FileName += "_" + args.ActionName;

//                if (args.View.HasChars())
//                    args.FileName += "_" + args.View;

//                if (args.Scenario.HasChars())
//                    args.FileName += "_" + args.Scenario;

//                args.FileName += ".html";
//            }

//            File.WriteAllText(Path.Combine(TempHtmlDirectory, args.FileName), html);
//        }

//        public static void RenderAction<TController>(Expression<Action<TController>> expression, params object[] args) where TController : Controller
//        {
//            RenderAction(expression, null, args);
//        }

//        public static void RenderAction<TController>(Expression<Action<TController>> expression, string fileNameNoExt, params object[] args) where TController : Controller
//        {
//            var controller = RunAction<TController>(expression, args);
//            var writer = (StringWriter)controller.ControllerContext.HttpContext.Response.Output;
//            var html = writer.GetStringBuilder().ToString();

//            if (!fileNameNoExt.HasChars())
//            {
//                fileNameNoExt = controller.Url.Action<TController>(expression);
//                if (fileNameNoExt.Contains("?"))
//                    fileNameNoExt = fileNameNoExt.Before("?");
//                fileNameNoExt = fileNameNoExt.StartWithout(@"/").Replace(@"/", "_").CharsOr("index");
//            }

//            File.WriteAllText(Path.Combine(OutputPath, fileNameNoExt + ".html"), html, Encoding.UTF8);
//        }

//        /// <summary>
//        /// Creates a controller context, invokes the action with the parameters specified, and executes the action.
//        /// </summary>
//        /// <typeparam name="TController"></typeparam>
//        /// <param name="expression"></param>
//        /// <param name="args"></param>
//        /// <returns></returns>
//        public static TController RunAction<TController>(Expression<Action<TController>> expression, params object[] args) where TController : Controller
//        {
//            var controller = Activator.CreateInstance<TController>();
//            controller.SetFakeControllerContext();
//            controller.ControllerContext.RouteData.Values["controller"] = ControllerName(controller.GetType());
//            var methodCall = (MethodCallExpression)expression.Body;
//            controller.ControllerContext.RouteData.Values["action"] = ActionName(methodCall.Method);

//            var url = new UrlHelper(controller.ControllerContext.RequestContext).Action(ActionName(methodCall.Method), ControllerName(controller.GetType())).StartWith(@"/").StartWith("~");
//            controller.ControllerContext.HttpContext.Request.SetupRequestUrl(url);
//            controller.Url = new UrlHelper(controller.ControllerContext.RequestContext);

//            //var validator = new DataAnnotationsModelValidator()

//            Dictionary<string, object> arguments = new Dictionary<string, object>();
//            if (args != null && args.Length > 0)
//            {
//                for (var i = 0; i < args.Length; i++)
//                {
//                    arguments[methodCall.Method.GetParameters()[i].Name] = args[i];
//                }
//            }
//            ActionInvoker.ActionParams = arguments;
//            //add null params so the invoke doesn't fail
//            var diff = (methodCall.Method.GetParameters().Length - args.Length);
//            if (diff > 0)
//            {
//                var newArgs = new ArrayList(args);
//                diff.Times(() => newArgs.Add(null));
//                args = newArgs.ToArray();
//            }
//            ActionInvoker.InvokeActionCallback = () => (ActionResult)methodCall.Method.Invoke(controller, args);

//            controller.ActionInvoker.InvokeAction(controller.ControllerContext, controller.RouteData.GetRequiredString("action"));

//            return controller;
//        }

//        private static string ControllerName(Type controllerType)
//        {
//            return controllerType.Name.EndWithout("Controller");
//        }

//        private static string ActionName(MethodInfo action)
//        {
//            var customName = (ActionNameAttribute)Attribute.GetCustomAttribute(action, typeof(ActionNameAttribute));
//            return customName == null ? action.Name : customName.Name;
//        }

//        private static HttpRuntime GetTheRuntimeInstance()
//        {
//            return typeof(HttpRuntime).InvokeMember("_theRuntime", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Static, null, null, null) as HttpRuntime;
//        }

//        private static void SetHttpRuntimePath(HttpRuntime theRuntimeInstance, string absolutePath)
//        {
//            typeof(HttpRuntime).InvokeMember("_appDomainAppPath", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance, null, theRuntimeInstance, new object[] { absolutePath });
//            var vpath = typeof(HttpRuntime).Assembly.GetType("System.Web.VirtualPath");
//            var path = vpath.GetMethod("Create", BindingFlags.Public | BindingFlags.Static, System.Type.DefaultBinder, CallingConventions.Any, new Type[] { typeof(string) }, new ParameterModifier[] { });//.Invoke(null, "/");
//            var s = path.Invoke(null, new object[] { @"/" });
//            typeof(HttpRuntime).InvokeMember("_appDomainAppVPath", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance, null, theRuntimeInstance, new object[] { s });
//        }
//    }


//    public class RenderViewParams<TController> where TController : Controller
//    {

//        /// <summary>
//        /// Use this or ActionName
//        /// </summary>
//        public Expression<Action<TController>> ActionExpr
//        {
//            get;
//            set;
//        }

//        public string ActionName
//        {
//            get;
//            set;
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

//        public string Scenario
//        {
//            get;
//            set;
//        }

//        public string FileName
//        {
//            get;
//            set;
//        }

//        public bool IsSharedView
//        {
//            get;
//            set;
//        }

//        public IUser User { get; set; }
//    }


//    public static class MvcMockHelpers
//    {
//        public static HttpContextBase FakeHttpContext()
//        {
//            var context = new Mock<HttpContextBase>();
//            var request = new Mock<HttpRequestBase>();

//            request.SetupGet(x => x.ApplicationPath).Returns("/");
//            request.SetupGet(r => r.HttpMethod).Returns("GET");
//            request.SetupGet(x => x.Url).Returns(new Uri("http://localhost/a", UriKind.Absolute));
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
//                {
//                    return Program.RunInWebServer ? s : s.StartWithout(@"/");//quick fix to get relative urls so a web server doesn't ened to run to view the files
//                });

//            context.Setup(ctx => ctx.Request).Returns(request.Object);
//            context.Setup(ctx => ctx.Response).Returns(response.Object);
//            context.Setup(ctx => ctx.Session).Returns(session.Object);
//            context.Setup(ctx => ctx.Server).Returns(server.Object);
//            context.Setup(ctx => ctx.Items).Returns(items);
//            IPrincipal user = null;
//            context.Setup(ctx => ctx.User).Returns(user);//Mock.Of<IPrincipal>(u => u.Identity == Mock.Of<IIdentity>(i => i.Name == "1234")));
//            return context.Object;
//        }


//        public static void SetFakeControllerContext(this Controller controller)
//        {
//            var httpContext = FakeHttpContext();
//            var context = new ControllerContext(new RequestContext(httpContext, new RouteData()), controller);
//            controller.ControllerContext = context;
//        }

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

    
//    class ThreadCache : IThreadCacheService
//    {

//        public IDictionary Items
//        {
//            get { return items; }
//        }

//        private IDictionary items = new Hashtable();
//    }

//    ////this crap is used so a nullreferenceexception doesn't occur in System.Web.WebPages.FileExistenceCache.FileExists(String virtualPath)
//    //HttpRuntime theRuntimeInstance = GetTheRuntimeInstance();
//    //SetHttpRuntimePath(theRuntimeInstance, @"c:\dev\ProjectFolder\");
//    //VirtualPathFactoryManager virtualPathFactoryManager = (VirtualPathFactoryManager)typeof(VirtualPathFactoryManager).GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null, null);
//    //List<IVirtualPathFactory> vps = (List<IVirtualPathFactory>)virtualPathFactoryManager.GetType().GetField("_virtualPathFactories", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(virtualPathFactoryManager);
//    //var virtualPathProviderMock = new Mock<VirtualPathProvider>();
//    //var existanceCacheType = typeof(IVirtualPathFactory).Assembly.GetType("System.Web.WebPages.FileExistenceCache");
//    //virtualPathProviderMock.Setup(p => p.FileExists(null)).Returns<string>((p) => false);
//    //virtualPathFactoryManager.GetType().GetField("_vpp", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(virtualPathFactoryManager, virtualPathProviderMock.Object);
//    //var cacheInstance = existanceCacheType.GetMethod("GetInstance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, null);
//    //existanceCacheType.GetField("_virtualPathProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(cacheInstance, virtualPathProviderMock.Object);


//}
