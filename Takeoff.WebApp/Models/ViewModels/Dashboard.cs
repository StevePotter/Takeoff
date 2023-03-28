using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mediascend.Web;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.ViewModels
{

    public class Dashboard_Index : StartupMessage
    {
        public IAccount Account { get; set; }

        public List<MembershipRequest> MembershipRequests { get; set; }

        public IEnumerable<Production> Productions { get; set; }

        public IEnumerable<ProductionActivityItem> Activity { get; set; } 

        public class MembershipRequest
        {
            public int Id { get; set; }
            public int ProductionId { get; set; }
            public string ProductionTitle { get; set; }
            public int UserId { get; set; }
            public string Email { get; set; }
            public string Name { get; set; }
            public bool IsInvitation { get; set; }
        }

        public class Production
        {
            public int Id { get; set; }
            public long CreatedOn { get; set; }
            public string Title { get; set; }
            public long LastChangeDate { get; set; }
            public string OwnerName { get; set; }
        }
    }
}
