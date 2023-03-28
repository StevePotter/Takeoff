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
    /// for /account/plan
    /// </summary>
    public class AccountPlan_Index
    {
        public IPlan Plan { get; set; }

        public PlanForSale[] AvailablePlans { get; set; }
    }

    /// <summary>
    /// for /account/plan/edit, where you upgrade or downgrade
    /// </summary>
    public class AccountPlan_Edit
    {
        public IPlan CurrentPlan { get; set; }
        public IPlan NewPlan { get; set; }
    }


    /// <summary>
    /// for /account/plan/edit, where you downgrade but the current account exceeds the new plan's limit(s), and thus an error message is given.
    /// </summary>
    public class AccountPlan_Edit_DowngradeInvalid
    {

        public string[] LimitsExceeded { get; set; }
    }
}
