using System;
using System.Reflection;
using System.Configuration;
using System.ComponentModel;
using Takeoff.Models;

namespace Takeoff.Resources
{

    /// <summary>
    /// Shortcut class name for the Strings class.  Allows for more concise markup.
    /// </summary>
    public class S : Strings
    {
    }


    public class TraceableResourceManager : System.Resources.ResourceManager
    {
        public TraceableResourceManager(string baseName, Assembly assembly, ResourceTraceLevel resourceTraceLevel)
            : base(baseName, assembly)
                {
                    this._defaultResourceTraceLevel = resourceTraceLevel;
                }

        private readonly ResourceTraceLevel _defaultResourceTraceLevel;

        public const string UserResourceTracingCookieName = "ResourceTracing";

        public override string GetString(string name, System.Globalization.CultureInfo culture)
        {
            //this mess checks for a user setting for specific resource tracking.  Some care was taken to optimize it so there is only one setting lookup per request
            var currentContext = HttpContextFactory.Current;
            ResourceTraceLevel userResourceTracing = ResourceTraceLevel.NotSet;
            if (currentContext != null)
            {
                var cookie = currentContext.Request.Cookies[UserResourceTracingCookieName];
                if (cookie != null)
                {//lots of parsing will occur but it's not so important because it's only for QA and stuff.
                    userResourceTracing = (ResourceTraceLevel)Enum.Parse(typeof(ResourceTraceLevel), (string)cookie.Value);
                }
            }

            var traceLevel = userResourceTracing == ResourceTraceLevel.NotSet ? _defaultResourceTraceLevel : userResourceTracing;

            string translation = base.GetString(name, culture);

            if (Takeoff.Global.StealthUI && translation.HasChars())
            {
                translation = translation.Replace("Takeoff", "Lipstick");
            }

            //this bit will add "--" on each side of the translation if the resource didn't exist in the resource database.  this is useful when checking for missing resource strings.
            if (traceLevel != ResourceTraceLevel.NotSet)
            {
                translation = Object.ReferenceEquals(translation, name) ? "--" + translation + "--" : translation;
            }

            switch (traceLevel)
            {
                case ResourceTraceLevel.NotSet:
                    return translation;
                case ResourceTraceLevel.Translation:
                    return string.Format("R[{0}]", translation);
                case ResourceTraceLevel.Name:
                    return string.Format("R[{0}]", name);//ex: "R[Cancel]".  R stands for resource
                case ResourceTraceLevel.NameAndTranslation:
                    return string.Format("R[{0}: {1}]", name, translation);//ex: "R[Agree: I Agree to {0}]".  R stands for resource
                default:
                    throw new InvalidEnumValueException(traceLevel);
            }
        }

        
    }


    /// <summary>
    /// Indicates information that will be output instead of or in addition to resource translations.
    /// </summary>
    public enum ResourceTraceLevel
    {
        /// <summary>
        /// The translation is shown alone.
        /// </summary>
        NotSet,
        /// <summary>
        /// The translation is surrounded by somethign that indicates it came from the resource db.  For example, a regular page resource could return "R[I agree to the terms]" or a module resource would return "MR[That's cool]".  This is useful when looking for actual text with the bare minimum of id crap.
        /// </summary>
        Translation,
        /// <summary>
        /// Instead of text the name of the resource is returned.  Ex:  R[Cancel], MR[HomePageButtonText]
        /// </summary>
        Name,
        /// <summary>
        /// Same as short but includes the actual translated text as well. Ex:  MR[HomePageButtonText: Videos you'll like!]
        /// </summary>
        NameAndTranslation,
    }


}