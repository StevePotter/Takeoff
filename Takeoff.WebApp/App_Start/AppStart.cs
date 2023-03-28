using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml.Linq;
using Mediascend.Web;
using Ninject;
using Takeoff.Data;
using Takeoff.Models;
using System.Configuration;
using System.Web.Routing;
using System.Web.Mvc;
using System.Security;
using System.IO;
using Takeoff.Platform;
using Takeoff.Resources;
using Takeoff.Controllers;
using Takeoff.ThingRepositories;
using Ninject.Web.Common;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Takeoff.App_Start.AppStart), "Start")]

namespace Takeoff.App_Start {

    /// <summary>
    /// Does the work that is typically done in global.asax Application_Start, but uses the cool WebActivator framework.  This makes it easy for other nuget packages to setup start code, and makes app setup simple for unit testing.
    /// </summary>
    public static partial class AppStart {

        public static void Start() {
            IoC();
            Repositories();
            Routes();
            GlobalFilters();
            Things();
            ModelBinders();
            ResourceManager();
            UserSettings.Init();
        }

        private static void Things()
        {
            Models.Things.Init();
        }

        private static void ResourceManager()
        {
//sets the resource manager used by Strings to an instance of TraceableResourceManager, which makes it possible to get tracing output on strings that are taken from resources.  It's a read only property with private field, so we use reflection to set it
            ResourceTraceLevel resourceTraceLevel = string.IsNullOrEmpty(ConfigurationManager.AppSettings["ResourceTraceLevel"])
                                                        ? ResourceTraceLevel.NotSet
                                                        : (ResourceTraceLevel)
                                                          Enum.Parse(typeof (ResourceTraceLevel),
                                                                     ConfigurationManager.AppSettings["ResourceTraceLevel"]);
            typeof (Strings).GetField("resourceMan",
                                      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue
                (null, new TraceableResourceManager("Takeoff.Resources.Strings", typeof (Strings).Assembly, resourceTraceLevel));
                //            private static global::System.Resources.ResourceManager resourceMan;
        }

        private static void ModelBinders()
        {
            System.Web.Mvc.ModelBinders.Binders.Add(typeof (FileSize), new FileSizeModelBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof (FileSize?), new FileSizeModelBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof (XDocument), new XDocumentBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof (double?), new NullableDoubleModelBinder());
        }

        private static void Repositories()
        {
            Takeoff.IoC.Bind<ITweetsRepository>().To<TweetsRepository>().InSingletonScope();
            Takeoff.IoC.Bind<IBlogEntriesRepository>().To<BlogItemRssRepository>().InSingletonScope();
            Takeoff.IoC.Bind<IUsersRepository>().To<UsersThingRepository>().InSingletonScope();
            Takeoff.IoC.Bind<IPromptRepository>().To<PromptsThingRepository>().InSingletonScope();
            Takeoff.IoC.Bind<IPlansRepository>().To<PlansL2SRepository>().InSingletonScope();
            Takeoff.IoC.Bind<ISemiAnonymousUsersRepository>().To<SemiAnonymousUsers2SRepository>().InSingletonScope();
            Takeoff.IoC.Bind<IAccountsRepository>().To<AccountsThingRepository>().InSingletonScope();
            Takeoff.IoC.Bind<IVideosRepository>().To<VideosThingRepository>().InSingletonScope();
            Takeoff.IoC.Bind<IProductionsRepository>().To<ProductionsThingRepository>().InSingletonScope();
            Takeoff.IoC.Bind<IBusinessEventsRepository>().To<BusinessEventsL2SRepository>().InSingletonScope();
        }

        private static void IoC()
        {
            if (Takeoff.IoC.Current == null)
            {
                Takeoff.IoC.Current = new StandardKernel();
                Takeoff.IoC.Bind<IActionInvoker>().To<ControllerActionInvokerWithExcludeFilter>();
                Takeoff.IoC.Bind<IThreadCacheService>().To<HttpContextThreadCache>().InSingletonScope();
                Takeoff.IoC.Bind<IIdentityService>().To<IdentityService>().InSingletonScope();
                Takeoff.IoC.Bind<IAppUrlPrefixes>().To<AppUrlPrefixes>().InRequestScope();
            }
        }

        private static void GlobalFilters()
        {
            System.Web.Mvc.GlobalFilters.Filters.Add(new ProfilerAttribute { Order = 0 });
            System.Web.Mvc.GlobalFilters.Filters.Add(new LogEmailLinkAttribute());            
            System.Web.Mvc.GlobalFilters.Filters.Add(new SetViewDataAttribute());//this should go before any filters that might return a view
            System.Web.Mvc.GlobalFilters.Filters.Add(new JsonParametersAttribute());
            System.Web.Mvc.GlobalFilters.Filters.Add(new QueueDeferredWebRequestAttribute());
            System.Web.Mvc.GlobalFilters.Filters.Add(new QueueDeferredMethodInvokesAttribute());
            System.Web.Mvc.GlobalFilters.Filters.Add(new WarnAboutIE6Attribute());
            System.Web.Mvc.GlobalFilters.Filters.Add(new AjaxRedirectAttribute());
            System.Web.Mvc.GlobalFilters.Filters.Add(new HandleErrorExAttribute { ExceptionType = typeof(SecurityException), SendEmail = false });
            System.Web.Mvc.GlobalFilters.Filters.Add(new HandleErrorExAttribute { ExceptionType = typeof(NoPermissionException), View = "Error.NoPermission", SendEmail = false });
            System.Web.Mvc.GlobalFilters.Filters.Add(new HandleErrorExAttribute { ExceptionType = typeof(ThingNotFoundException), View = "Error.ThingNotFound", SendEmail = false });
            System.Web.Mvc.GlobalFilters.Filters.Add(new HandleErrorExAttribute { Order = 400 });
            System.Web.Mvc.GlobalFilters.Filters.Add(new EnsureNecessaryUserInfoAttribute());
            System.Web.Mvc.GlobalFilters.Filters.Add(new CheckSuspendedAccountAttribute());
            System.Web.Mvc.GlobalFilters.Filters.Add(new CheckPromptAttribute());
            System.Web.Mvc.GlobalFilters.Filters.Add(new CheckCookiesAttribute());
        }

        private static void Routes()
        {
            var routes = RouteTable.Routes;
            routes.IgnoreRoute("crossdomain.xml");
            routes.IgnoreRoute("Assets/{*pathInfo}");
            routes.IgnoreRoute("scripts/{*pathInfo}");
            //            routes.MapRouteLowerCase("incomingmail", "incomingmail", new { controller = "IncomingMail", action = "Index" });//had to set up route so action could take a post and be called Create
            routes.MapRouteLowerCase("trackmailopen", "mail/track/{id}", new { controller = typeof(EmailController).Name.EndWithout("Controller"), action = "Track" });//had to set up route so action could take a post and be called Create
            // Mediascend.Web.RouteHelper.RegisterRoutes(routes, new System.Reflection.Assembly[]{typeof(Global).Assembly});
            Mediascend.Web.RouteHelper.RegisterRoutes(routes, typeof(Global), null, false);
            routes.MapRouteLowerCase("vanityurls", "{vanityUrl}", new { controller = "Productions", action = "Details" },
                                     new { vanityUrl = new VanityUrlConstraint() });
            RouteHelper.RegisterGenericRoute(routes, null);
            //            routes.Add("catchall", new CatchAllRouteHandler("{controller}/{action}/{id}", new RouteValueDictionary(new { action = "Index", id = UrlParameter.Optional }), new MvcRouteHandler()));
        }


    }
}
