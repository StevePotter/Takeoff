using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Security.Policy;

namespace System.Configuration
{
    /// <summary>
    /// </summary>
    public static class ConfigUtil
    {
        public static string AppSetting(string setting)
        {
            return ConfigurationManager.AppSettings[setting];
        }


        #region Appsettings

        /// <summary>
        /// Gets the appsetting with the given key and throws an exception if it doesn't exist.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static string GetRequiredAppSetting(string setting)
        {
            return GetRequiredAppSetting<string>(setting);
        }

        public static T AppSettingOrDefault<T>(string settingName, T defaultValue)
        {
            var value = ConfigurationManager.AppSettings[settingName];
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            try
            {
                return value.ConvertTo<T>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while converting app setting '" + settingName + "'.", ex);
            }
        }


        /// <summary>
        /// Gets the appsetting with the given key and throws an exception if it doesn't exist.  Converts it to the desired data type.
        /// </summary>
        public static T GetRequiredAppSetting<T>(string setting)
        {
            var value = ConfigurationManager.AppSettings[setting];
            if (string.IsNullOrEmpty(value))
                throw new Exception("AppSetting '" + setting + "' was missing.");
            try
            {
                return value.ConvertTo<T>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while converting app setting '" + setting + "'.", ex);
            }
        }


        #endregion

    }
}
