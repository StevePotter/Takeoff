using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mediascend.Web;
using Takeoff.Data;

namespace Takeoff.ViewModels
{
    
    /// <summary>
    /// /account/memberships
    /// </summary>
    public class AccountMemberships_Index
    {
        public AccountMemberships_AccountsUserBelongsTo[] AccountsUserBelongsTo { get; set; }
    }

    /// <summary>
    /// For various actions within /account/members
    /// </summary>
    public class AccountMemberships_AccountsUserBelongsTo
    {
        /// <summary>
        /// The ID of the membership record.
        /// </summary>
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string AccountOwnerName { get; set; }
    }


    /// <summary>
    /// /account/memberships/{id}
    /// </summary>
    public class AccountMemberships_Details
    {
        /// <summary>
        /// The ID of the membership record.
        /// </summary>
        public int Id { get; set; }

        public DateTime JoinedAccountOn { get; set; }

        public UserSummary AccountOwner { get; set; }
        
        public Production[] Productions { get; set; }

        public class Production
        {
            public int Id { get; set; }

            public string Title { get; set; }
        }
    }

    ///// <summary>
    ///// for /account/members/defailts 
    ///// </summary>
    //public class AccountMembers_Update
    //{
    //    /// <summary>
    //    /// ID of the membership record
    //    /// </summary>
    //    public int Id { get; set; }

    //    /// <summary>
    //    /// The members's new role.
    //    /// </summary>
    //    public string Role { get; set;  }
    //}

}
