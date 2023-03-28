using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Takeoff.Data
{
    /// <summary>
    /// Identity service that uses forms authentication and httpcontext to manage identity.  
    /// </summary>
    /// <remarks>
    /// Resolved identity gets saved in "identity" for fast lookups.  if the user is not logged in, string.Empty is a placeholder
    /// </remarks>
    public class IdentityService: IIdentityService
    {

        public Identity GetIdentity(HttpContextBase context)
        {
            var objIdentity = context.Items["identity"];
            if (objIdentity != null)
            {
                var asIdentity = objIdentity as Identity;
                if (asIdentity == null)
                    return null;
                else
                    return asIdentity;
            }

            //this will happen on first call to this method in a request.  simply parse out the identity
            string strIdentity = null;
            if (context.User != null && context.User.Identity != null)
            {
                strIdentity = context.User.Identity.Name;
            }
            Identity identity = null;
            if ( strIdentity.HasChars())
            {
                identity = ParseIdentity(strIdentity);
            }
            SetIdentityInContext(identity, context);//might be null
            return identity;
        }

        //user identity is the integer value (for backward compatitiblity).  semi-anonymous users have a guid id after "s-"
        private static Identity ParseIdentity(string strIdentity)
        {
            if (strIdentity[0] == 's')
            {
                var strId = strIdentity.After("-");
                Guid id;
                if ( !Guid.TryParse(strId, out id))//messed up cookie, should hardly ever happen
                {
                    return null;
                }
                var user = Repos.SemiAnonymousUsers.Get(id);
                if ( user == null)//shouldn't happen but you never know
                {
                    return null;
                }
                return new SemiAnonymousUserIdentity(user);
            }

            int? userId = strIdentity.ToIntTry();
            if (!userId.HasValue)
                return null;
            //a bum value will throw a parse exception
            return new UserIdentity
                            {
                                UserId = userId.Value
                            };
        }

        private void SetIdentityInContext(Identity value, HttpContextBase context)
        {
            object objIdentity;
            if (value == null)
                objIdentity = string.Empty;//placeholder for fast lookup
            else
                objIdentity = value;

            context.Items["identity"] = objIdentity;
            //setting context.User was necessary because it affected the anti-forgery token generated
            if (value == null)
            {
                context.User = new GenericPrincipal(new GenericIdentity(string.Empty),null);
            }
            else
            {
                context.User = new GenericPrincipal(new GenericIdentity(value.ToString()), null);                
            }
        }


        public void SetIdentity(Identity value, IdentityPeristance peristance, HttpContextBase context)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            SetIdentityInContext(value, context);

            if (peristance == IdentityPeristance.PermanentCookie || peristance == IdentityPeristance.TemporaryCookie)
            {
                string cookieValue = value.ToString();

                if ( peristance == IdentityPeristance.PermanentCookie)//this became the only way to keep people logged in.  for whatever reason forms auth doesn't support this.  pretty weak if you ask me but it works well
                {
                    var expires = DateTime.Now.AddYears(2);
                    FormsAuthentication.Initialize();
                    var cookie = FormsAuthentication.GetAuthCookie(cookieValue, true);
                    cookie.Expires = expires;
                    var ticket = FormsAuthentication.Decrypt(cookie.Value);
                    var updatedTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate, expires, true, ticket.UserData);
                    cookie.Value = FormsAuthentication.Encrypt(updatedTicket);
                    HttpContext.Current.Response.Cookies.Add(cookie);  
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(cookieValue, false);
                }
            }
        }


        public void ClearIdentity(HttpContextBase context)
        {
            var identity = GetIdentity(context);
            if (identity != null)//no need to call if nobody is signed in
            {
                SetIdentityInContext(null, context);
                FormsAuthentication.SignOut();
            }
        }

    }
}
