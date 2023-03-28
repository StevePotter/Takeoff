using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.UI;
using System.Security.Policy;
using System.Web.Mvc;
using System.Text;

namespace Mediascend.Web
{
    /// <summary>
    /// Helps to include assets that are deployed locally or on a CDN (where they are minified, compressed, and even forever cached).
    /// </summary>
    public static class AssetUtil
    {
        public static string AssetVersion = ConfigurationManager.AppSettings["AssetVersion"];

        public static string CdnPathPrefix = ConfigurationManager.AppSettings["AssetCdnPathPrefix"].EndWith("/") + AssetVersion.EndWith("/");
        public static string CdnPathPrefixSecure = ConfigurationManager.AppSettings["AssetCdnPathPrefixSecure"].EndWith("/") + AssetVersion.EndWith("/");

        public static string GetCdnPathPrefix(HttpRequestBase request)
        {
            return request.IsSecureConnection ? CdnPathPrefixSecure : CdnPathPrefix;
        }

        public static bool UseLocalFiles
        {
            get
            {
                if (!_useLocalFiles.HasValue)
                {
                    var settingValue = ConfigurationManager.AppSettings["AssetLocation"];
                    switch (settingValue.ToLowerInvariant())
                    {
                        case "cdn":
                            _useLocalFiles = false;
                            break;
                        case "local":
                            _useLocalFiles = true;
                            break;
                        default:
                            throw new ArgumentException("AppSetting 'AssetLocation' had an invalid value.  Valid values are 'CDN' or 'Local'.");
                    }
                }
                return _useLocalFiles.Value;
            }
        }
        private static bool? _useLocalFiles;



        public static bool PreventAssetBrowserCaching
        {
            get
            {
                if (!_PreventAssetBrowserCaching.HasValue)
                {
                    var value = ConfigurationManager.AppSettings["PreventAssetBrowserCaching"];
                    if (string.IsNullOrEmpty(value))
                        throw new Exception("AppSetting  'PreventAssetBrowserCaching' was missing.");
                    _PreventAssetBrowserCaching = value.ConvertTo<bool>();
                }
                return _PreventAssetBrowserCaching.Value;
            }
        }
        private static bool? _PreventAssetBrowserCaching;


        public static string AssetDevJsSuffix
        {
            get
            {
                return _AssetDevJsSuffix.Value;
            }
        }
        private static Lazy<string> _AssetDevJsSuffix = new Lazy<string>(() =>
        {
            return ConfigurationManager.AppSettings["AssetDevJsSuffix"];
        });

        public static IHtmlString JsLib(this HtmlHelper html, string libraryName)
        {
            var useDevSuffix = html.ViewContext.HttpContext.Items["__devJs"] != null;//set in BasicController

            return AssetLib(html, libraryName, useDevSuffix ? AssetDevJsSuffix + ".js" : "js", null, (src) => "<script src=\"" + src + "\" language=\"javascript\" type=\"text/javascript\"></script>", AssetDevJsSuffix);
        }

        public static IHtmlString CssLib(this HtmlHelper html, string libraryName)
        {
            return AssetLib(html, libraryName, "css", "styles/", (src) => "<link href=\"" + src + "\" rel=\"stylesheet\" type=\"text/css\" />", null);
        }

        private static IHtmlString AssetLib(this HtmlHelper html, string libraryName, string libraryExt, string libraryFolder, Func<string, string> tagBuilder, string devSuffix)
        {
            if (UseLocalFiles)
            {
                //slow but this is for dev only
                var library = ((AssetLibariesConfiguration)ConfigurationManager.GetSection("assetLibraries")).Libraries.Cast<AssetLibraryConfigElement>().Where(l => l.Name == libraryName).First();
                StringBuilder sb = new StringBuilder();
                foreach (AssetConfigElement asset in library.Assets)
                {
                    var path = asset.Path;
                    path = path.StartsWith("~/") ? path : "~" + path.StartWith("/");
                    path = UrlHelper.GenerateContentUrl(path, html.ViewContext.HttpContext);
                    sb.AppendLine(tagBuilder(path));
                }
                return html.Raw(sb.ToString());
            }
            else
            {
                var pathPrefix = AssetUtil.GetCdnPathPrefix(html.ViewContext.HttpContext.Request) + (libraryFolder ?? String.Empty) + libraryName;
                return html.Raw(tagBuilder(pathPrefix + "." + libraryExt));
            }
        }

        public static string Asset(this UrlHelper url, string relativeUrl)
        {
            if (UseLocalFiles)
            {
                var toResolve = relativeUrl.StartsWith("~/") ? relativeUrl : "~" + relativeUrl.StartWith("/");

                if (PreventAssetBrowserCaching)
                {
                    if (toResolve.Contains("?"))
                    {
                        toResolve = toResolve.EndWith("&");
                    }
                    else
                    {
                        toResolve = toResolve.EndWith("?");
                    }
                    toResolve += "nobrowsercache=" + DateTime.Now.Ticks.ToString();
                }

                return url.Content(toResolve);
            }
            else
            {
                relativeUrl = relativeUrl.StartWithout("/");//the CdnPathPrefix has the "/" at the end
                return AssetUtil.GetCdnPathPrefix(url.RequestContext.HttpContext.Request) + relativeUrl;
            }
        }

    }


    public class AssetLibariesConfiguration : ConfigurationSection
    {
        [
        ConfigurationProperty("libraries", IsDefaultCollection = false),
        ConfigurationCollection(typeof(AssetLibraryConfigElement), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")
        ]
        public AssetLibraryConfigElementCollection Libraries
        {
            get
            {
                return this["libraries"] as AssetLibraryConfigElementCollection;
            }
        }

    }


    public class AssetLibraryConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("assets", IsDefaultCollection = false),
        ConfigurationCollection(typeof(AssetConfigElement), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public AssetConfigElementCollection Assets
        {
            get
            {
                return this["assets"] as AssetConfigElementCollection;
            }
        }

    }

    public class AssetConfigElement : ConfigurationElement
    {
        /// <summary>
        /// The relative path of the asset. 
        /// </summary>
        [ConfigurationProperty("path", IsRequired = true)]
        public string Path
        {
            get
            {
                return this["path"] as string;
            }
        }
    }

    public class AssetLibraryConfigElementCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        public AssetLibraryConfigElement this[int index]
        {
            get { return (AssetLibraryConfigElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public void Add(AssetLibraryConfigElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new AssetLibraryConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AssetLibraryConfigElement)element).Name;
        }

        public void Remove(AssetLibraryConfigElement element)
        {
            BaseRemove(element.Name);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }
    }

    public class AssetConfigElementCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        public AssetConfigElement this[int index]
        {
            get { return (AssetConfigElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public void Add(AssetConfigElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new AssetConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AssetConfigElement)element).Path;
        }

        public void Remove(AssetConfigElement element)
        {
            BaseRemove(element.Path);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }
    }


}
