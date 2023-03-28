using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.WebApp.Ripper
{
    class User: IUser
    {
        public int AccountId { get; set; }
        public int Id { get; set; }
        public int CreatedByUserId { get; set; }
        public int OwnerUserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? LastChangeId { get; set; }
        public DateTime? LastChangeDate { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsVerified { get; set; }
        public string VerificationKey { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordResetKey { get; set; }
        public int? TimezoneOffset { get; set; }
        public dynamic SignupSource { get; set; }
        public List<ISetting> Settings { get; set; }
        public List<IViewPrompt> PendingPrompts { get; set; }
        public Dictionary<int, IAccountMembership> AccountMemberships { get; set; }
        public Dictionary<int, IMembership> EntityMemberships { get; set; }
        public IAccount Account { get; set; }
        public DateTime? ConvertedFromBetaOn { get; set; }
        public void TrackChanges()
        {
            throw new NotImplementedException();
        }

        #region Dummy Data

        /// <summary>
        /// User subscribed to a freelance plan.
        /// </summary>
        public static IUser SubscribedFreelance
        {
            get
            {
                return new User
                {
                    Id = 56,
                    FirstName = "Bill",
                    LastName = "Sparks",
                    Email = "someonesemail@email.com",
                    DisplayName = "Bill Sparks",
                    Account = Ripper.Account.SubscribedFreelance,
                };
            }
        }

        public static IUser TrialFreelance
        {
            get
            {
                return new User
                {
                    Id = 56,
                    FirstName = "Bill",
                    LastName = "Sparks",
                    Email = "someonesemail@email.com",
                    DisplayName = "Bill Sparks",
                    Account = Ripper.Account.TrialFreelanceBeta,
                };
            }
        }


        /// <summary>
        /// User in demo mode.
        /// </summary>
        public static IUser Demo
        {
            get
            {
                return new User
                {
                    Id = 56,
                    FirstName = "",
                    LastName = "",
                    Email = "someonesemail@email.com",
                    DisplayName = "You",
                    Account = Ripper.Account.Demo,
                };
            }
        }

        public static IUser Trial2Anonymous
        {
            get
            {
                return new User
                {
                    Id = 56,
                    FirstName = "",
                    LastName = "",
                    Email = "",
                    DisplayName = "",
                    Account = Ripper.Account.Trial2,
                };
            }
        }

        public static IUser Trial2SignedUp
        {
            get
            {
                return new User
                {
                    Id = 56,
                    FirstName = "Bill",
                    LastName = "Franklin",
                    Email = "billf@gmail.com",
                    DisplayName = "Bill Franklin",
                    Account = Ripper.Account.Trial2,
                };
            }
        }


        public static IUser Guest
        {
            get
            {
                return new User
                {
                    Id = 2109,
                    FirstName = "Andy",
                    LastName = "Dodd",
                    Email = "takeoffguest@email.com",
                    DisplayName = "Andy Dodd",
                    Account = null,
                };
            }
        }

        public static UserView View1
        {
            get
            {
                return new UserView
                           {
                               Email = "email1@email.com",
                               Id = 5,
                               Name = "Andy Dodd"
                           };
            }
        }

        public static UserView View2
        {
            get
            {
                return new UserView
                           {
                               Email = "email3@email.com",
                               Id = 5,
                               Name = "Bernie Madoff"
                           };
            }
        }

        public static UserView View3
        {
            get
            {
                return new UserView
                           {
                               Email = "email3@email.com",
                               Id = 5,
                               Name = "John Madden"
                           };
            }
        }

        #endregion
    }
}
