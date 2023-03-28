using System.Web.Mvc;
using System.Web.WebPages;
using PrecompiledMvcViewEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using System.IO;
using System.Configuration;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Takeoff.App_Start.PrecompiledMvcViewEngineStart), "Start")]

namespace Takeoff.App_Start
{
    public static class PrecompiledMvcViewEngineStart
    {
        public static void Start()
        {
            if (ApplicationSettings.UsePrecompiledViewEngine)
            {
                var engine = new PrecompiledMvcEngine2(typeof(PrecompiledMvcViewEngineStart).Assembly);
                ViewEngines.Engines.Insert(0, engine);

                // StartPage lookups are done by WebPages. 
                VirtualPathFactoryManager.RegisterVirtualPathFactory(engine);
            }
        }
    }


    public class PrecompiledMvcEngine2 : VirtualPathProviderViewEngine, IVirtualPathFactory
    {
        private readonly IDictionary<string, Type> _mappings;

        public PrecompiledMvcEngine2(Assembly assembly)
        {
            base.AreaViewLocationFormats = new string[] { "~/Areas/{2}/Views/{1}/{0}.cshtml", "~/Areas/{2}/Views/{1}/{0}.vbhtml", "~/Areas/{2}/Views/Shared/{0}.cshtml", "~/Areas/{2}/Views/Shared/{0}.vbhtml" };
            base.AreaMasterLocationFormats = new string[] { "~/Areas/{2}/Views/{1}/{0}.cshtml", "~/Areas/{2}/Views/{1}/{0}.vbhtml", "~/Areas/{2}/Views/Shared/{0}.cshtml", "~/Areas/{2}/Views/Shared/{0}.vbhtml" };
            base.AreaPartialViewLocationFormats = new string[] { "~/Areas/{2}/Views/{1}/{0}.cshtml", "~/Areas/{2}/Views/{1}/{0}.vbhtml", "~/Areas/{2}/Views/Shared/{0}.cshtml", "~/Areas/{2}/Views/Shared/{0}.vbhtml" };
            base.ViewLocationFormats = new string[] { "~/Views/{1}/{0}.cshtml", "~/Views/{1}/{0}.vbhtml", "~/Views/Shared/{0}.cshtml", "~/Views/Shared/{0}.vbhtml" };
            base.MasterLocationFormats = new string[] { "~/Views/{1}/{0}.cshtml", "~/Views/{1}/{0}.vbhtml", "~/Views/Shared/{0}.cshtml", "~/Views/Shared/{0}.vbhtml" };
            base.PartialViewLocationFormats = new string[] { "~/Views/{1}/{0}.cshtml", "~/Views/{1}/{0}.vbhtml", "~/Views/Shared/{0}.cshtml", "~/Views/Shared/{0}.vbhtml" };
            base.FileExtensions = new string[] { "cshtml", "vbhtml" };
            this._mappings = (from type in assembly.GetTypes()
                              where typeof(WebPageRenderingBase).IsAssignableFrom(type)
                              let pageVirtualPath = type.GetCustomAttributes(false).OfType<PageVirtualPathAttribute>().FirstOrDefault<PageVirtualPathAttribute>()
                              where pageVirtualPath != null
                              select new KeyValuePair<string, Type>(pageVirtualPath.VirtualPath, type)).ToDictionary<KeyValuePair<string, Type>, string, Type>(t => t.Key, t => t.Value, StringComparer.OrdinalIgnoreCase);
        }

        public object CreateInstance(string virtualPath)
        {
            Type type;
            if (this._mappings.TryGetValue(virtualPath, out type))
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            Type type;
            if (this._mappings.TryGetValue(partialPath, out type))
            {
                return new PrecompiledMvcView2(partialPath, type, false, base.FileExtensions);
            }
            return null;
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            Type type;
            if (this._mappings.TryGetValue(viewPath, out type))
            {
                return new PrecompiledMvcView2(viewPath, type, true, base.FileExtensions);
            }
            return null;
        }

        public bool Exists(string virtualPath)
        {
            return this._mappings.ContainsKey(virtualPath);
        }

        protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            return this.Exists(virtualPath);
        }
    }

    public class PrecompiledMvcView2 : IView
    {
        private readonly Type _type;
        private readonly string _virtualPath;

        public PrecompiledMvcView2(string virtualPath, Type type, bool runViewStartPages, IEnumerable<string> fileExtension)
        {
            this._type = type;
            this._virtualPath = virtualPath;
            this.RunViewStartPages = runViewStartPages;
            this.ViewStartFileExtensions = fileExtension;
        }

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            WebViewPage page = Activator.CreateInstance(this._type) as WebViewPage;
            if (page == null)
            {
                throw new InvalidOperationException("Invalid view type");
            }
            page.VirtualPath = this._virtualPath;
            page.ViewContext = viewContext;
            page.ViewData = viewContext.ViewData;
            page.InitHelpers();
            WebPageRenderingBase startPage = null;
            if (this.RunViewStartPages)
            {
                try
                {
                    startPage = StartPage.GetStartPage(page, "_ViewStart", this.ViewStartFileExtensions);
                }
                catch
                { }
            }
            WebPageContext pageContext = new WebPageContext(viewContext.HttpContext, page, null);
            page.ExecutePageHierarchy(pageContext, writer, startPage);
        }

        public bool RunViewStartPages { get; private set; }

        public IEnumerable<string> ViewStartFileExtensions { get; private set; }
    }
}
