using System;
using System.Collections.Generic;

namespace Takeoff.Data
{
    public interface IUser : ITypicalEntity
    {
        string Email { get; set; }

        /// <summary>
        /// The full display name for this user.  This will, in Takeoff 2, default to first name + last name.  But it should still be customizable by users.
        /// </summary>
        string DisplayName { get; set; }

        string FirstName { get; set; }
        string LastName { get; set; }
        bool IsVerified { get; set; }
        string VerificationKey { get; set; }
        string Password { get; set; }
        string PasswordSalt { get; set; }
        string PasswordResetKey { get; set; }

        /// <summary>
        /// The value from javascript's getTimezoneOffset, which is difference between local and UTC in minutes.  This will eventually be set for all users, but can be null for historical users.  Null uses the server's timezone.
        /// </summary>
        int? TimezoneOffset { get; set; }

        /// <summary>
        /// A json-encoded string that indicates where they came from.  If they were invited or requested access, it indicates that.  
        /// </summary>
        dynamic SignupSource { get; set; }

        List<ISetting> Settings { get; }
        List<IPrompt> PendingPrompts { get; }

        /// <summary>
        /// Gets a table of account memberships for this user, indexed by account Id.  Note the AccountMembershipThing are isolated and not part of a Thing tree (no Parent or children)
        /// </summary>
        Dictionary<int, IAccountMembership> AccountMemberships { get; }

        /// <summary>
        /// Gets a table of things that the user is a member of, indexed by thing Id.  This includes accounts. Note the MembershipThing are isolated and not part of a Thing tree (no Parent or children)
        /// </summary>
        Dictionary<int, IMembership> EntityMemberships { get; }

        /// <summary>
        /// Gets the account owned by this user.  If they don't own an account, null is returned.
        /// </summary>
        /// <returns></returns>
        IAccount Account { get; }

        /// <summary>
        /// Call this to track property-specific changes to an object.  Used for updating.
        /// </summary>
        void TrackChanges();
    }

}