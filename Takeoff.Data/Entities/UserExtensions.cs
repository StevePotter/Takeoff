using System;
using System.IO;
using System.Linq;
using Takeoff.Models;

namespace Takeoff.Data
{
    /// <summary>
    /// Helper extensions for the IUser interface.
    /// </summary>
    public static class UserExtensions
    {
        /// <summary>
        /// Indicates whether the user was created during the Takeoff beta period.
        /// </summary>
        public static bool CreatedDuringBeta(this IUser account)
        {
            return account.ConvertedFromBetaOn.HasValue;
        }


        public static bool IsMemberOf(this IUser user, ITypicalEntity entity)
        {
            return Repos.Users.IsMemberOf(user, entity);
        }

        public static IUser Update(this IUser entity, Action<IUser> setPropertiesToUpdate)
        {
            entity.TrackChanges();
            setPropertiesToUpdate(entity);
            Repos.Users.Update(entity);
            return entity;
        }

        public static IUser Update(this IUser entity)
        {
            Repos.Users.Update(entity);
            return entity;
        }

         
        public static IUser Insert(this IUser entity)
        {
            Repos.Users.Insert(entity);
            return entity;
        }

        public static object GetSettingValue(this IUser entity, string name)
        {
            var settingThing = Repos.Users.GetSetting(entity, name);
            if (settingThing == null)
            {
                return SettingDefinitions.Definitions[name].Default;
            }
            else
            {
                return settingThing.Value;
            }
        }


        public static object GetSettingValue(this IUser entity, SettingDefinition definition)
        {
            var settingThing = Repos.Users.GetSetting(entity, definition.Name);
            if (settingThing == null)
            {
                return definition.Default;
            }
            else
            {
                return settingThing.Value;
            }
        }

        /// <summary>
        /// Takes the UTC date passed and converts it to a local time in the user's time zone.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime UtcToLocal(this IUser user, DateTime date)
        {
            if (date.Kind == DateTimeKind.Local)//database returned unspecified so treat that as utc
                throw new ArgumentException("Date must be UTC");

            if (user.TimezoneOffset.HasValue)
            {
                return new DateTime(date.Ticks - TimeSpan.FromMinutes(user.TimezoneOffset.Value).Ticks, DateTimeKind.Local);
            }
            else
            {
                return date.ToLocalTime();
            }
        }

        /// <summary>
        /// Takes the local date in the user's time zone and converts it to UTC.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime LocalToUtc(this IUser user, DateTime date)
        {
            if (date.Kind != DateTimeKind.Local)
                throw new ArgumentException("Date must be Local");

            if (user.TimezoneOffset.HasValue)
            {
                return new DateTime(date.Ticks + TimeSpan.FromMinutes(user.TimezoneOffset.Value).Ticks, DateTimeKind.Utc);
            }
            else
            {
                return date.ToUniversalTime();
            }
        }

    }
}