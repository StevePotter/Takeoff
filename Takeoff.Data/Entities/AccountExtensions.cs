using System;
using System.IO;
using System.Linq;
using Takeoff.Models;

namespace Takeoff.Data
{
    /// <summary>
    /// Helper extensions for the IAccount interface.
    /// </summary>
    public static class AccountExtensions
    {
        /// <summary>
        /// Indicates whether the user is subscribed to a plan, meaning they pay money each month or whatever.  The subscription might be expired or delinquent but they still are subscribed.
        /// If they are not subscribed, they are in a free plan, a demo, or a trial.
        /// </summary>
        public static bool IsSubscribed(this IAccount account)
        {
            switch (account.Status)
            {
                case AccountStatus.Subscribed:
                case AccountStatus.ExpiredNonpayment:
                case AccountStatus.Pastdue:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// When true, there is a problem such as billing problem or abuse, and the account has been suspended.
        /// </summary>
        public static bool IsSuspended(this IAccount account)
        {
            switch (account.Status)
            {
                case AccountStatus.ExpiredNonpayment:
                case AccountStatus.Pastdue:
                case AccountStatus.TrialExpired:
                case AccountStatus.Suspended:
                    return true;
                default:
                    return false;
            }
        }


        /// <summary>
        /// Indicates whether this account can be signed up (purchased).  
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static bool CanPurchase(this IAccount account)
        {
            switch (account.Status)
            {
                case AccountStatus.Demo:
                case AccountStatus.Trial:
                case AccountStatus.Trial2:
                case AccountStatus.TrialExpired:
                    return true;
                default:
                    return false;
            }
        }


        public static DateTime? CurrentBillingPeriodEndsOn(this IAccount account)
        {
            var start = account.CurrentBillingPeriodStartedOn;
            var plan = account.Plan;
            if (!start.HasValue || plan == null)
            {
                return null;
            }
            return plan.BillingCycleEnd(start.Value);
        }


        /// <summary>
        /// Indicates whether the trial period hasn't ended yet.  Keep in mind their status could be Subscribed if this is true.
        /// </summary>
        public static bool InTrialPeriod(this IAccount account)
        {
            return account.DaysLeftInTrial.GetValueOrDefault(0) > 0;
        }

        public static bool IsTrial(this IAccount account)
        {
            switch (account.Status)
            {
                case AccountStatus.Trial2:
                case AccountStatus.TrialExpired:
                case AccountStatus.Trial:
                case AccountStatus.TrialAnonymous:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Indicates whether the account was created during the Takeoff beta period.
        /// </summary>
        public static bool CreatedDuringBeta(this IAccount account)
        {
            return account.ConvertedFromBetaOn.HasValue;
        }

        /// <summary>
        /// Max number of seconds a particular video can be.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static int VideoDurationLimit(this IAccount account)
        {
            var plan = account.Plan;
            if (plan != null && plan.VideoDurationLimit.HasValue)
            {
                return plan.VideoDurationLimit.Value;
            }

            return System.Configuration.ConfigUtil.AppSettingOrDefault<int>("VideoDurationLimit", 3600*4);
        }


    }
}