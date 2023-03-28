using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Mediascend.Web;
using System.Web.Script.Serialization;
using System.Collections;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Models.Data;
using System.Dynamic;
using MvcContrib;
using Takeoff.Transcoder;
using System.Configuration;
using Newtonsoft.Json.Linq;
using System.Web.UI.DataVisualization.Charting;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Text;
using Takeoff.Resources;
using Recurly;
using System.Xml.Linq;
using Takeoff.ViewModels;
using System.Linq.Dynamic;

namespace Takeoff.Controllers
{
    /// <summary>
    /// Contains maintenance tasks for the backend of Takeoff.
    /// </summary>
    [SpecialRestriction(SpecialRestriction.Staff | SpecialRestriction.DeferredRequest)]
    public class StaffJobsController : BasicController
    {


        #region Subscriptions

        ///// <summary>
        ///// Synchronizes pastdue, valid, and expired accounts in Recurly with our system.
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult BillingSync()
        //{

        //    //first we update any trials that have expired
        //    var today = this.RequestDate().Date;
        //    var newExpiredAccounts = DataModel.ReadOnlyQuery(db => db.Accounts.Where(a => a.Status == AccountStatus.Trial.ToString() && a.TrialPeriodEndsOn != null && a.TrialPeriodEndsOn.Value.Date <= today).FilterDeletedThings(db, t => t.ThingId).Select(a => a.ThingId).ToArray());
        //    if (newExpiredAccounts.Length > 0)
        //    {
        //        foreach (var account in newExpiredAccounts.Select(id => Things.GetOrNull<AccountThing>(id)).Where(a => a != null))
        //        {
        //            account.TrackChanges();
        //            account.Status = AccountStatus.TrialExpired;
        //            account.Update();
        //            new EmailController().TrialExpirationWarning(account.Owner.Id);
        //        }
        //    }

        //    //handle past due discrepencies
        //    {
        //        //past due...suspend account
        //        var recurlyPastDueIds = new HashSet<int>(GetRecurlyAccounts("pastdue_subscribers"));

        //        //get any current accounts that are marked past due but aren't in recurly's results
        //        var takeoffOutstandingAccounts = DataModel.ReadOnlyQuery(db => db.Accounts.Where(a => a.Status == AccountStatus.Pastdue.ToString() && !recurlyPastDueIds.Contains(a.ThingId)).FilterDeletedThings(db, t => t.ThingId).Select(a => a.ThingId).ToArray());
        //        if (takeoffOutstandingAccounts.Length > 0)
        //        {
        //            foreach (var account in takeoffOutstandingAccounts.Select(id => Things.GetOrNull<AccountThing>(id)).Where(a => a != null))
        //            {
        //                account.TrackChanges();
        //                account.Status = AccountStatus.Pastdue;
        //                account.Update();
        //            }
        //        }
        //    }

        //    //handle expired (or no subscription at all) discrepencies
        //    {
        //        //expired or no subscription at all
        //        var recurlyInactiveIds = new HashSet<int>(GetRecurlyAccounts("non_subscribers"));

        //        //if an account is marked as expired or no subscription, we only care if the account was previously subscribed or past due
        //        var statuses = new AccountStatus[] { AccountStatus.Subscribed, AccountStatus.Pastdue }.Select(s => s.ToString()).ToArray();

        //        //gets accounts that are subscribed or past due and are marked as inactive in recurly.  in this case, they have expired
        //        var takeoffOutstandingAccounts = DataModel.ReadOnlyQuery(db => db.Accounts.Where(a => statuses.Contains(a.Status) && recurlyInactiveIds.Contains(a.ThingId)).FilterDeletedThings(db, t => t.ThingId).Select(a => a.ThingId).ToArray());
        //        if (takeoffOutstandingAccounts.Length > 0)
        //        {
        //            foreach (var account in takeoffOutstandingAccounts.Select(id => Things.GetOrNull<AccountThing>(id)).Where(a => a != null))
        //            {
        //                account.TrackChanges();
        //                account.Status = AccountStatus.ExpiredNonpayment;
        //                account.Update();
        //            }
        //        }
        //    }

        //    //subscriptions that are active
        //    {
        //        //expired or no subscription at all
        //        var recurlyActiveIds = new HashSet<int>(GetRecurlyAccounts("active_subscribers"));

        //        //gets accounts that are not marked as subscribed in takeoff but in recurly
        //        var takeoffOutstandingAccounts = DataModel.ReadOnlyQuery(db => db.Accounts.Where(a => a.Status != AccountStatus.Subscribed.ToString() && recurlyActiveIds.Contains(a.ThingId)).FilterDeletedThings(db, t => t.ThingId).Select(a => a.ThingId).ToArray());
        //        if (takeoffOutstandingAccounts.Length > 0)
        //        {
        //            foreach (var account in takeoffOutstandingAccounts.Select(id => Things.GetOrNull<AccountThing>(id)).Where(a => a != null))
        //            {
        //                account.TrackChanges();
        //                account.Status = AccountStatus.Subscribed;
        //                account.Update();
        //            }
        //        }
        //    }


        //    return this.Empty();
        //}

        //private List<int> GetRecurlyAccounts(string show)
        //{
        //    PagingHelper paging = null;
        //    var recurlyPastDueAccountIds = new List<int>();
        //    while (paging == null || !paging.IsLastPage)
        //    {
        //        var pageParam = "?page=" + (paging == null ? "1" : paging.PageNumber.ToInvariant());
        //        XDocument xDoc = null;//todo: append page
        //        RecurlyClient.PerformRequest(RecurlyClient.HttpRequestMethod.Get, "/accounts" + pageParam + "&show=" + show, (reader =>
        //        {
        //            xDoc = XDocument.Load(reader);
        //        }));
        //        if (paging == null)
        //        {
        //            paging = new PagingHelper();
        //            paging.PageSize = xDoc.Descendants("per_page").First().Value.ToInt();
        //            paging.TotalItemCount = xDoc.Descendants("total_entries").First().Value.ToInt();
        //        }

        //        paging.PageIndex = xDoc.Descendants("current_page").First().Value.ToInt() - 1;//their paging is 1-based
        //        recurlyPastDueAccountIds.AddRange(xDoc.Descendants("account_code").Select(e => e.Value.ToInt()).ToArray());
        //    }
        //    return recurlyPastDueAccountIds;
        //}

        ///// <summary>
        ///// Sends emails for when a trial expires.
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult DailyEmails()
        //{
        //    const string TaskType = "TrialExpirationEmails";
        //    using (var db = new DataModel())
        //    {
        //        //it already ran today
        //        if ((from t in db.TaskLogs where t.Date.Date == DateTime.Today && t.Type == TaskType select t).Count() > 0)
        //            return new EmptyResult();

        //        var accountIds = (from a in db.Accounts where a.Status == AccountStatus.Trial.ToString() select a.ThingId).ToArray();
        //        foreach (var account in Things.Get(accountIds).Cast<AccountThing>())
        //        {
        //            var daysLeft = account.DaysLeftInTrial;
        //            if (daysLeft.HasValue)
        //            {
        //                var owner = account.Owner;
        //                if (owner.Email.HasChars() && owner.IsVerified)//don't send emails to non-verified users.  honestly what's the point if they haven't come back?
        //                {
        //                    //don't send at day 0, that's sent when the account status is changed
        //                    switch (daysLeft.Value)
        //                    {
        //                        case 7:
        //                        case 2:
        //                            new EmailController().TrialExpirationWarning(owner.Id);
        //                            break;
        //                    }
        //                }
        //            }
        //        }
        //        db.TaskLogs.InsertOnSubmit(new TaskLog
        //        {
        //            Date = DateTime.UtcNow,
        //            Type = TaskType
        //        });
        //        db.SubmitChanges();
        //    }
        //    return this.Empty();
        //}

        #endregion


    }
}
