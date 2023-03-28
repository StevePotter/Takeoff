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
    }
}