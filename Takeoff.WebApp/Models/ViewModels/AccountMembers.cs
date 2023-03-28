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
    /// For various actions within /account/members
    /// </summary>
    public class AccountMembers_Membership
    {
        /// <summary>
        /// The ID of the actual membership record.
        /// </summary>
        public int MembershipId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Role { get; set; }
        public int MemberId { get; set; }
        public string MemberEmail { get; set; }
        public string MemberName { get; set; }
    }

    /// <summary>
    /// /account/members
    /// </summary>
    public class AccountMembers_Index
    {
        public AccountMembers_Membership[] Memberships { get; set; }
    }

    /// <summary>
    /// /account/members/{id}
    /// </summary>
    public class AccountMembers_Details
    {
        public AccountMembers_Membership Membership { get; set; }

        public Production[] Productions { get; set; }

        public class Production
        {
            public int Id { get; set; }

            public string Title { get; set; }
        }
    }

    /// <summary>
    /// for /account/members/defailts 
    /// </summary>
    public class AccountMembers_Update
    {
        /// <summary>
        /// ID of the membership record
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The members's new role.
        /// </summary>
        public string Role { get; set;  }
    }

}
