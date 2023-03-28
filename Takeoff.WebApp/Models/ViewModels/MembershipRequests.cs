using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Takeoff.ViewModels
{
    public class MembershipRequests_Details
    {
        public int RequestId { get; set; }
        public string RequestedByName { get; set; }
        public string RequestedByEmail   { get; set; }

        public int ProductionId { get; set; }
        public string ProductionTitle { get; set; }

        public string Note { get; set; }

        public DateTime CreatedOn { get; set; }
    }


    public class MembershipRequests_Reject
    {
        /// <summary>
        /// User ID who requested membership, which we've declined.
        /// </summary>
        public int RequestedById { get; set; }
        public string RequestedByName { get; set; }
    }

    public class MembershipRequests_InvitationAccepted
    {
        public int InvitedById { get; set; }
        public string InvitedByDisplayName { get; set; }
        public int ProductionId { get; set; }
    }


    public class MembershipRequests_InvitationRejected
    {
        public int InvitedById { get; set; }
        public string InvitedByDisplayName { get; set; }
        public int ProductionId { get; set; }
    }
}
