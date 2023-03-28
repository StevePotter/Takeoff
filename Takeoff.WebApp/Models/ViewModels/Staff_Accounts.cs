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
    public class Staff_Accounts_Details
    {
        public int Id { get; set; }
        public IAccount Account { get; set; }
        public IUser Owner { get; set; }
        public string RecurlyUrl { get; set; }
    }

    public class Staff_Accounts_Edit
    {
        public int Id { get; set; }

        [DisplayName("Owner Id")]
        public int OwnerId { get; set; }

        [DisplayName("Account Plan")]
        public string PlanId { get; set; }

        [DisplayName("Account Status")]
        public AccountStatus Status { get; set; }

        [DisplayName("Trial Expiration")]
        public DateTime? TrialPeriodEndsOn { get; set; }

        [DisplayName("Current Billing Period Starts On")]
        public DateTime? CurrentBillingPeriodStartedOn { get; set; }

        

        public IEnumerable<SelectListItem> Plans
        {
            get
            {
                return Repos.Plans.Get().Select(p =>
                    {
                        return new SelectListItem
                        {
                            Value = p.Id,
                            Text = p.Title,
                        };
                    });
            }
        }

    }

}


