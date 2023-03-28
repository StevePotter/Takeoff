using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Takeoff.Data
{
    /// <summary>
    /// Gets and sets the identity 
    /// </summary>
    public interface IIdentityService
    {
        /// <summary>
        /// Called just about every request, to confirm user's identity.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Identity GetIdentity(HttpContextBase context);

        /// <summary>
        /// Generally called after a user logs in or out.
        /// </summary>
        void SetIdentity(Identity value, IdentityPeristance peristance, HttpContextBase context);

        /// <summary>
        /// For logging people out.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        void ClearIdentity(HttpContextBase context);
    }

    /// <summary>
    /// Determines how an identity is persisted.
    /// </summary>
    public enum IdentityPeristance
    {
        /// <summary>
        /// Identity will be lost at end of request.
        /// </summary>
        None,
        /// <summary>
        /// Meant for sliding expirations in FormsAuthentication.
        /// </summary>
        TemporaryCookie,
        /// <summary>
        /// Persisted forever.  Used for "remember me" option.
        /// </summary>
        PermanentCookie,
    }

    public abstract class Identity
    {   
    }

    /// <summary>
    /// Identity for a user that has a record in our system.
    /// </summary>
    public class UserIdentity: Identity
    {
        public int UserId { get; set; }

        public override string ToString()
        {
            return UserId.ToInvariant();//persisted in cookies and iprinciple identity
        }
    }

    /// <summary>
    /// Identity for a person who has anonymous access to a particular production.
    /// </summary>
    public class SemiAnonymousUserIdentity : Identity
    {
        public SemiAnonymousUserIdentity(ISemiAnonymousUser user)
        {
            _user = user;
        }

        public ISemiAnonymousUser User
        {
            get { return _user; }
        }
        private readonly ISemiAnonymousUser _user;

        public int TargetId
        {
            get { return User.TargetId.GetValueOrDefault(); }
        }

        public override string ToString()
        {
            if ( User == null)
                return string.Empty;
            return "s-" + User.Id.ToString();
        }
    }

}
