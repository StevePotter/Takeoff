using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Models.Data;

namespace Takeoff.Models
{

    /// <summary>
    /// System roles for Takeoff.
    /// </summary>
    public static class TakeoffRoles
    {
        public const string Owner = "Owner";
        public const string Staff = "Staff";
        public const string Client = "Client";
    }


    public static class Permissions
    {

        /// <summary>
        /// The user can view a list of the entities.  The views exposed by the list are up the UI.
        /// </summary>
        public const string List = "List";
        /// <summary>
        /// The user can view details of the entity.  This should include way more information than the List.
        /// </summary>
        public const string Details = "Details";
        /// <summary>
        /// The user can add an entity of EntityType. 
        /// </summary>
        public const string Add = "Add";
        /// <summary>
        /// The user can edit an entity of EntityType. 
        /// </summary>
        public const string Edit = "Edit";
        /// <summary>
        /// The user can delete an entity of EntityType. 
        /// </summary>
        public const string Delete = "Delete";


        /// <summary>
        /// A condition where the user must be a member of either the Account, the ContainerEntity, or the TargetEntity (depends on ConditionContext);
        /// </summary>
        public const string MemberCondition = "Member";
        /// <summary>
        /// A condition where the user must be the creator of either the Account, the ContainerEntity, or the TargetEntity (depends on ConditionContext);
        /// </summary>
        public const string CreatorCondition = "Creator";

        /// <summary>
        /// The condition is applied to the Target thing when checking a permission.
        /// </summary>
        public const string TargetConditionContext = "Target";
        /// <summary>
        /// The condition is applied to the Target's specified containing thing when checking a permission.
        /// </summary>
        public const string ContainerConditionContext = "Container";
        /// <summary>
        /// The condition is applied to the Target's account thing when checking a permission.
        /// </summary>
        public const string AccountConditionContext = "Account";


        #region Verify 

        /// <summary>
        /// Verifies that the user can create a thing of the given type underneath the contextThing.  If contextThing is not a container, its container will be used to check permissions.
        /// </summary>
        /// <param name="contextThing"></param>
        /// <param name="targetEntityType"></param>
        /// <returns></returns>
        public static ThingBase VerifyCreate(this ThingBase contextThing, Type newThingType, UserThing user)
        {
            return VerifyCreate(contextThing, Things.ThingType(newThingType), user);
        }

        public static bool CanCreate(this ThingBase contextThing, Type newThingType, UserThing user)
        {
            return CanCreate(contextThing, Things.ThingType(newThingType), user);
        }

        /// <summary>
        /// Throws an exception if the user doesn't have permission to create an entity of the given type within the contextEntity's entity tree.  Returns contextEntity for chaining.
        /// </summary>
        public static ThingBase VerifyCreate(this ThingBase contextThing, string newThingType, UserThing user)
        {
            Permissions.Verify((contextThing.IsContainer ? contextThing : contextThing.Container), newThingType, Permissions.Add, user);
            return contextThing;
        }

        public static bool CanCreate(this ThingBase contextThing, string newThingType, UserThing user)
        {
            return Permissions.HasPermission((contextThing.IsContainer ? contextThing : contextThing.Container), newThingType, Permissions.Add, user);
        }

        /// <summary>
        /// Throws an exception if the user doesn't have permission to perform the action on the given entity.  Returns entity for chaining.
        /// </summary>
        public static ThingBase Verify(this ThingBase target, string action, UserThing user)
        {
            if (!HasPermission(target, action, user))
                throw new NoPermissionException(target.Type);
            return target;
        }


        public static void Verify(ThingBase container, ThingBase target, string action, UserThing user)
        {
            if (!HasPermission(container, target, action, user))
                throw new NoPermissionException(target.Type);
        }

        public static void Verify(ThingBase container, string targetType, string action, UserThing user)
        {
            if (!HasPermission(container, targetType, action, user))
                throw new NoPermissionException(targetType);
        }

        #endregion

        #region HasPermission

        public static bool HasPermission(this ThingBase target, string action, UserThing user)
        {
            var permissions = GetPermissonsOnAccount(user.Id, target.AccountId);
            return permissions == null ? false : permissions.HasPermission(target, action, user);
        }

        public static bool HasPermission(ThingBase container, ThingBase target, string action, UserThing user)
        {
            Args.NotNull(container, "container");
            Args.NotNull(target, "target");

            var accountId = target.AccountId;
            if (accountId != container.AccountId)
                throw new ArgumentException("Both things should have same AccountId");

            var permissions = GetPermissonsOnAccount(user.Id, target.AccountId);
            return permissions == null ? false : permissions.HasPermission(container, target, action, user);
        }

        public static bool HasPermission(ThingBase container, Type targetType, string action, UserThing user)
        {
            return HasPermission(container, Things.ThingType(targetType), action, user);
        }

        public static bool HasPermission(ThingBase container, string targetType, string action, UserThing user)
        {
            var permissions = GetPermissonsOnAccount(user.Id, container.AccountId);
            return permissions == null ? false : permissions.HasPermission(container, targetType, action, user);
        }

        #endregion


        /// <summary>
        /// Gets the PermissionsOnAccount (using fast cached version) for the user on the account.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public static PermissionSet GetPermissonsOnAccount(int userId, int accountId)
        {
            var user = Things.GetOrNull<UserThing>(userId).EnsureExists(userId);
            IAccountMembership membership;
            if (!user.AccountMemberships.TryGetValue(accountId, out membership))
            {
                return null;
            }
            return GetPermissionSet(membership.Role);
        }
      
        /// <summary>
        /// Gets the that anyone in the given role on this application will have.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public static PermissionSet GetPermissionSet(string role)
        {
            var cacheKey = "ps." + role;
            return (PermissionSet)CacheUtil.GetCachedWithStringSerialization(cacheKey, true, () =>
                {
                    using (var db = DataModel.ReadOnly)
                    {
                        var rolePermissions = (from p in db.Permissions where p.ApplicationId == ApplicationEx.AppName && p.RoleName == role select p).ToArray();
                        return new PermissionSet(rolePermissions);
                    }
                }, (permissions) =>
                {
                    return Json.Serialize(permissions.GetPermissions());
                }, (json) =>
                {
                    var permissions = Json.Deserialize<Permission[]>(json);
                    return new PermissionSet(permissions);
                });
        }


    }

    /// <summary>
    /// Helps determine permissions for a single user on a single account.  Created by Permissions.  Held in cache per role.
    /// </summary>
    public class PermissionSet
    {

        public PermissionSet(Permission[] permissions)
        {
            permissionLookup = new Dictionary<string, Permission>();

            foreach (var permission in permissions)
            {
                permissionLookup.Add(GetPermissionKey(permission.ContainerThingType, permission.TargetThingType, permission.Action), permission);
            }
        }

        /// <summary>
        /// Provides a fast way to look up permissions.
        /// </summary>
        private Dictionary<string, Permission> permissionLookup;

        /// <summary>
        /// Creates a key for storing in the permissionLookup table.
        /// </summary>
        /// <param name="rootEntityType"></param>
        /// <param name="targetEntityType"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        static string GetPermissionKey(string containerType, string targetThing, string action)
        {
            return (containerType ?? String.Empty) + "_" + (targetThing ?? String.Empty) + "_" + action ?? string.Empty;
        }

        public Permission[] GetPermissions()
        {
            return permissionLookup.Values.ToArray();
        }

        /// <summary>
        /// Gets the permission for the specified criteria.  If user doesn't have the permission, null is returned.
        /// NOTE: This does NOT evaluate the conditions, so while the user might have the permission defined, it may evaluate to false for Things.
        /// </summary>
        /// <param name="rootEntityType"></param>
        /// <param name="targetEntityType"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public Permission GetPermission(string containerType, string targetThing, string action)
        {
            Permission permission;
            if (permissionLookup.TryGetValue(GetPermissionKey(containerType, targetThing, action), out permission))
            {
                return permission;
            }
            return null;
        }


        public bool HasPermission(ThingBase target, string action, UserThing user)
        {
            if (target.IsContainer)
            {
                return HasPermission(null, target, action, user);
            }
            else
            {
                return HasPermission(target.Container, target, action, user);
            }
        }

        public bool HasPermission(ThingBase container, ThingBase target, string action, UserThing user)
        {
            return HasPermission(container, target, null, action, user);
        }

        public bool HasPermission(ThingBase container, Type targetType, string action, UserThing user)
        {
            return HasPermission(container, Things.ThingType(targetType), action, user);
        }

        public bool HasPermission(ThingBase container, string targetType, string action, UserThing user)
        {
            return HasPermission(container, null, targetType, action, user);
        }


        /// <summary>
        /// Core HasPermission function.
        /// </summary>
        /// <param name="containerEntity">The container entity to check.  Its Type must match the ContainerThingType in Permission table.</param>
        /// <param name="targetEntity">The target entity to check.  Its Type must match the TargetThingType in Permission table.  If null, targetType parameter can be used.</param>
        /// <param name="targetType">Used when checking permission on a target that doesn't exist yet.  Example is checking whether user can create a Production under an Account.  Account entity exists, but Production doesn't.  When using this, conditions with Target context obviously won't work.</param>
        /// <param name="action">The action being performed.</param>
        /// <param name="user">The user performing the action.</param>
        /// <returns></returns>
        private bool HasPermission(ThingBase containerEntity, ThingBase targetEntity, string targetType, string action, UserThing user)
        {
            user.ValidateAsArg("user");
            if ( containerEntity == null && targetEntity == null )
                throw new InvalidOperationException("At least one entity must be provided.");
            if (containerEntity != null && !containerEntity.IsContainer)
                throw new ArgumentException("Container entity must be a container.");
            if (targetEntity != null && targetType == null)
                targetType = targetEntity.Type;

            var permission = GetPermission(containerEntity == null ? null : containerEntity.Type, targetType, action);
            if (permission == null)
                return false;

            switch (permission.Condition)
            {
                case Permissions.MemberCondition:
                    switch (permission.ConditionContext)
                    {
                        case Permissions.TargetConditionContext:
                            if (targetEntity == null)
                                throw new ArgumentException("No target specified.");
                            return user.IsMemberOf(targetEntity);
                        case Permissions.ContainerConditionContext:
                            if (containerEntity == null)
                                throw new ArgumentException("No target specified.");
                            return user.IsMemberOf(containerEntity);
                        case Permissions.AccountConditionContext:
                            var accountId = targetEntity == null ? containerEntity.AccountId : targetEntity.AccountId;
                            return user.IsMemberOf(accountId);
                    }
                    break;
                case Permissions.CreatorCondition:
                    switch (permission.ConditionContext)
                    {
                        case Permissions.TargetConditionContext:
                            if (targetEntity == null)
                                throw new ArgumentException("No target specified.");
                            return user.IsCreatorOf(targetEntity);
                        case Permissions.ContainerConditionContext:
                            if (containerEntity == null)
                                throw new ArgumentException("No target specified.");
                            return user.IsCreatorOf(containerEntity);
                        case Permissions.AccountConditionContext:
                            var accountId = targetEntity == null ? containerEntity.AccountId : targetEntity.AccountId;
                            var account = Things.GetOrNull<AccountThing>(accountId).EnsureExists(accountId);
                            return user.IsCreatorOf(account);
                    }
                    break;
            }

            throw new InvalidOperationException("Invalid permission check: " + permission.Condition ?? string.Empty + " " + permission.ConditionContext ?? string.Empty);

        }


    }


}


