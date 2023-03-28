using System;
using System.Web;
using System.Web.Security;
using System.Globalization;
using System.Security.Cryptography;
using Takeoff.Data;

namespace Takeoff.Models
{
    public static class Users
    {

        public const string UserNotFoundStatus = "NotFound";
        public const string UserVerifiedStatus = "Verified";
        public const string UserNotVerifiedStatus = "NotVerified";
        public const string WrongPasswordStatus = "WrongPassword";

        private static readonly IIdentityService IdentityService = IoC.GetOrNull<IIdentityService>();

        #region Properties


        public static Random Random = new Random();


        /// <summary>
        /// Gets the current user's ID.  If 0, that means there is no user logged in.  
        /// </summary>
        /// <returns></returns>
        /// <remarks>Only set UserId when you want to switch the UserThing.Current.  You better know what you're doing.  A good example of this is when sending out activity emails.  But don't mess with this normally.</remarks>
        public static int CurrUserId
        {
            get
            {
                if (IdentityService == null)
                    return 0;
                var context = HttpContextFactory.Current;
                if (context == null)
                    return 0; //todo:ICurrentUserService

                var userIdentity = IdentityService.GetIdentity(context) as UserIdentity;
                if (userIdentity == null)
                {
                    return 0;
                }
                else
                {
                    return userIdentity.UserId;
                }
            }
        }


        ///// <summary>
        ///// Gets the current user's ID.  If 0, that means there is no user logged in.  
        ///// </summary>
        //[Obsolete]
        //public static int UserId(this HttpContextBase context)
        //{
        //    if (context == null)
        //        throw new InvalidOperationException("UserId can only be accessed in an http request.");

        //    var objUserId = context.Items["cuid"];
        //    if (objUserId != null)
        //    {
        //        return (int)objUserId;
        //    }

        //    string strUserId = null;
        //    if (context.User != null && context.User.Identity != null)
        //    {
        //        strUserId = context.User.Identity.Name;
        //    }
        //    int id = 0;
        //    if (strUserId.HasChars())
        //        int.TryParse(strUserId, out id);

        //    context.UserId(id);
        //    return id;
        //}

        //public static void UserId(this HttpContextBase context, int value)
        //{
        //    context.Items["cuid"] = value;
        //}

        #endregion

        #region Methods



        /// <summary>
        /// Signs the user in.  Only to be used for existing verified users.
        /// </summary>
        /// <returns>
        /// String indicating the results:
        /// null - user was logged in successfully
        /// "NotFound" - email was not found in the system
        /// "NotVerified" - the user is not verified with a pw
        /// "WrongPassword" - password doesn't match
        /// </returns>
        public static LoginResult Login(string email, string password, int? timezoneOffset, bool rememberMe, bool ignorePassword)
        {
            email = Args.HasCharsLower(email);
            password = Args.HasCharsLower(password);

            var user = Repos.Users.GetByEmail(email);

            if (user == null)
                return LoginResult.NotFound;

            //users without a password generally come from being invited.  if they try to log in, they must first enter their email
            if ( !user.Password.HasChars() )
            {
                return LoginResult.NoPassword;
            }
            if (!ignorePassword && !IsPasswordCorrect(user, password))
                return LoginResult.WrongPassword;

            if (user.TimezoneOffset != timezoneOffset)
            {
                user.Update(u => u.TimezoneOffset = timezoneOffset);
            }

            IdentityService.SetIdentity(new UserIdentity
            {
                UserId = user.Id
            }, rememberMe ? IdentityPeristance.PermanentCookie : IdentityPeristance.TemporaryCookie, HttpContextFactory.Current);
            return LoginResult.Success;
        }

        public enum LoginResult
        {
            NotFound,
            NoPassword,
            WrongPassword,
            Success
        }

        /// <summary>
        /// Creates a new password reset key for the user and sends them an email on how to reset their password.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>
        /// String indicating the results:
        /// null - success
        /// "NotFound" - email was not found in the system
        /// "NotVerified" - the user hasn't been verified yet (they enter an initial password during verification).
        /// </returns>
        public static string RequestPasswordReset(string email)
        {
            email = Args.HasCharsLower(email, "email").Trim();

            var user = Repos.Users.GetByEmail(email);
            if (user == null)
                return UserNotFoundStatus;

            var resetKey = GenerateRandomAlphaNumeric(10);
            user.TrackChanges();
            user.PasswordResetKey = resetKey;
            user.Update();
            return null;
        }


        /// <summary>
        /// Creates a hashed (encrypted) password.
        /// </summary>
        /// <param name="pwd"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        private static string CreatePasswordHash(string pwd, string salt)
        {
            string saltAndPassword = String.Concat(pwd, salt);
            //note: the machineKey section in the web config provides the key for hashing.  the default is auto-generated (which is different per machine).  so we keep it the same for each environment (development, staging, production)
            string hashedPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(saltAndPassword, "sha1");
            return hashedPassword;
        }

        public static bool IsPasswordCorrect(this IUser user, string password)
        {
            if (!user.Password.HasChars())
                throw new InvalidOperationException("Password hasn't been set yet.");
            if (!password.HasChars())
                return false;
            password = password.ToLowerInvariant();//passwords are case insensitive right now
            string pwHash = CreatePasswordHash(password, user.PasswordSalt);
            return pwHash.EqualsCaseSensitive(user.Password);
        }



        /// <summary>
        /// Convenience method for setting the DisplayName using FirstName and LastName.  Does not call update.
        /// </summary>
        public static void UpdateDisplayName(this IUser user)
        {
            user.DisplayName = user.FirstName.EndWith(" ") + user.LastName.StartWithout(" ");
        }


        /// <summary>
        /// Creates password salt, which provides an extra layer of protection for our passwords.  
        /// </summary>
        /// <returns></returns>
        /// <remarks>This code, along with explanations, was taken from http://davidhayden.com/blog/dave/archive/2004/02/16/157.aspx and http://www.aspheute.com/english/20040105.asp</remarks> and http://community.bartdesmet.net/blogs/bart/archive/2004/12/25/515.aspx
        private static string CreateSalt()
        {
            const int size = 20;
            //Generate a cryptographic random number.
            var buff = new byte[size];
            new RNGCryptoServiceProvider().GetBytes(buff);
            return Convert.ToBase64String(buff);
        }


        /// <summary>
        /// Called from the password reset page, after the user requests a password reset and opens the link from the email they recieved.
        /// </summary>
        /// <param name="resetPasswordId"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>
        /// String indicating the results:
        /// null - success
        /// "NotFound" - email was not found in the system
        /// "InvalidKey" - the key passed in doesn't match the current one in the DB
        /// 
        /// Note that if the user's email is not verified, this will mark it as verified.  This makes sense because the only way for them to reset their password is to view their own email.  Now, it's very rare someone who isn't verified will reset their password before going through normal verification, but it can still happen.
        /// </returns>
        public static string ResetPassword(string email, string resetKey, string password)
        {
            email = Args.HasCharsLower(email, "email").Trim();
            password = Args.HasCharsLower(password, "password").Trim();
            resetKey = Args.HasChars(resetKey, "resetKey");

            var user = Repos.Users.GetByEmail(email);
            if (user == null)
                return UserNotFoundStatus;

            if (!resetKey.Equals(user.PasswordResetKey, StringComparison.Ordinal))
                return "InvalidKey";

            user.TrackChanges();
            user.PasswordResetKey = null;//so the old email can't be reused
            if (!user.IsVerified)
                user.IsVerified = true;//can happen if they haven't validated their email yet.  since pw link comes from an email, we can validate them now
            UpdatePassword(user, password);//update to the new password
            user.Update();
            IdentityService.SetIdentity(new UserIdentity
            {
                UserId = user.Id
            }, IdentityPeristance.TemporaryCookie, HttpContextFactory.Current);

            return null;
        }

        /// <summary>
        /// Indicates whether the passwordResetKey passed in matches the one in the DB.
        /// </summary>
        /// <param name="passwordResetKey"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsPasswordResetKeyCorrect(string email, string resetKey)
        {
            email = Args.HasCharsLower(email, "email").Trim();
            Args.HasChars(resetKey, "resetKey");

            var user = Repos.Users.GetByEmail(email);
            return resetKey.EqualsCaseSensitive(user.PasswordResetKey);
        }


        /// <summary>
        /// Gets the membership status of the user with the given email.  
        /// </summary>
        /// <param name="email"></param>
        /// <returns>
        /// "NotFound" - the email is not in the system, so it's a new user
        /// "Verified" - the user has been verified and has a password.  they can log in normally
        /// "NotVerified" - the user has been added (either initial signup or invitation) but has not been verified.  they need to verify their email and set a password
        /// </returns>
        public static string GetStatus(string email)
        {
            email = Args.Email(email, "email").Trim().ToLowerInvariant();
            var user = Repos.Users.GetByEmail(email);
            return GetStatus(user);
        }


        /// <summary>
        /// Gets the status of the user.  
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <returns>
        /// "NotFound" - the email is not in the system, so it's a new user
        /// "Verified" - the user has been verified and has a password.  they can log in normally
        /// "NotVerified" - the user has been added (either initial signup or invitation) but has not been verified.  they need to verify their email and set a password
        /// </returns>
        public static string GetStatus(IUser user)
        {
            if (user == null)
                return UserNotFoundStatus;
            if (user.IsVerified)
                return UserVerifiedStatus;
            else
                return UserNotVerifiedStatus;
        }

        /// <summary>
        /// Creates a new, unverified user with the given email address.  Does not create an account or send them an email.  That's up to the individual application.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static IUser Signup(string email, string nickName, string firstName, string lastName, int? timezoneOffset, dynamic signupSource, DateTime createdOn, string password = null)
        {
            if (email == null)//null emails will cause db error.  empty email will 
                email = string.Empty;
            email = email.ToLowerInvariant().Trim();

            if (email.HasChars() && Repos.Users.GetByEmail(email) != null)
                throw new Exception("A user with that email already exists.");

            if (string.IsNullOrEmpty(nickName))
                nickName = firstName.EndWith(" ") + lastName.StartWithout(" ");

            var user = Repos.Users.Instantiate().Once(u =>
            {
                u.CreatedOn = createdOn;
                u.CreatedByUserId = 0;
                u.Email = email;
                u.DisplayName = nickName;
                u.FirstName = firstName;
                u.LastName = lastName;
                u.IsVerified = false;
                u.TimezoneOffset = timezoneOffset;
                u.VerificationKey = GenerateVerificationKey();
                u.SignupSource = signupSource;
            });

            if (password.HasChars())
                UpdatePassword(user, password);

            Repos.Users.Insert(user);

            return user;
        }


        /// <summary>
        /// Sets the user's password and salt.  Doesn't call Update.  Currently this makes the password lowercase.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        public static void UpdatePassword(this IUser user, string password)
        {
            password = Args.HasCharsLower(password, "password").Trim();//always lowercase passwords so it's not case sensitive
            var salt = CreateSalt();
            user.Password = CreatePasswordHash(password, salt);
            user.PasswordSalt = salt;
        }


        /// <summary>
        /// Adds the user to the Users as a newly-invited user.  Doesn't send emails or anything.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="db"></param>
        public static IUser AddInvitedUser(int invitedBy, string email, string displayName, dynamic signupSource, bool checkForExisting)
        {
            email = Args.HasCharsLower(email);

            if (checkForExisting)
            {
                var currUser = Repos.Users.GetByEmail(email);
                if (currUser != null)
                    throw new ArgumentException("email was already in the system", email);
            }

            return UserExtensions.Insert(Repos.Users.Instantiate().Once(u =>
            {
                u.CreatedOn = DateTime.UtcNow;
                u.CreatedByUserId = invitedBy;
                u.Email = email;
                u.DisplayName = string.IsNullOrEmpty(displayName)
                                  ? email
                                  : displayName;
                u.IsVerified = false;
                u.VerificationKey = GenerateVerificationKey();
                u.SignupSource = signupSource;
            }));
        }


        public static string GenerateVerificationKey()
        {
            return GenerateRandomAlphaNumeric(10);
        }


        /// <summary>
        /// System.Web.Security.Membership.GeneratePassword didn't work, I had to write a quick one.  This one creates a string with a random combination of lower and uppercase letters and numbers.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateRandomAlphaNumeric(int length)
        {
            Args.GreaterThanZero(length, "length");
            var sb = new System.Text.StringBuilder();

            for (var i = 0; i < length; i++)
            {
                sb.Append(RandomAlphaNumericChars[Random.Next(0, RandomAlphaNumericChars.Length)]);//random upper bound is exclusive
            }
            return sb.ToString();
        }
        const string RandomAlphaNumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";


        #endregion

    }



}