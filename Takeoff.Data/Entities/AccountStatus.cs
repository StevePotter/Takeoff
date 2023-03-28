using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Takeoff.Data
{

    public enum AccountStatus
    {
        /// <summary>
        /// They are in a free plan and don't have to pay for Takeoff.  Their plan may have limits though.
        /// </summary>
        FreePlan = 0,
        /// <summary>
        /// The account is in demo mode.  Restrictions are enforced via the plan, which is "demo".  Old demo accounts can be deleted.
        /// DEPRECATED.  No longer needed since we have new "non-expiring" trial plan
        /// </summary>
        Demo = 1,
        /// <summary>
        /// They are in the trial period.  Please note that if they subscribe during the trial, the status will become Subscribed even though they won't be billed until the trial is over.  This might affect messages during upgrades and downgrades, emails, things like that.
        /// </summary>
        Trial = 2,
        /// <summary>
        /// They were in a free trial but it expired.  At this point they must sign up.
        /// </summary>
        TrialExpired = 3,
        /// <summary>
        /// Combines demo + trial, and will be used for trials after it is developed.  
        /// </summary>
        Trial2 = 8,
        /// <summary>
        /// What used to be 'demo', now is considered a trial with an anonymous user.
        /// </summary>
        TrialAnonymous = 9,
        /// <summary>
        /// The subscription is in good paid standing.
        /// </summary>
        Subscribed = 4,
        /// <summary>
        /// The account is delinquent.  It should be in the dunning process.
        /// </summary>
        Pastdue = 5,
        /// <summary>
        /// The subscription expired due to nonpayment.
        /// </summary>
        ExpiredNonpayment = 6,
        /// <summary>
        /// The account has been manually suspended by our staff.  This is for someone who violates the user agreement or tries to do something sneaky.
        /// </summary>
        Suspended = 7,
    }

}
