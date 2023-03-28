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
using System.ComponentModel;
using Takeoff.Data;

namespace Takeoff.Controllers
{
    [SpecialRestriction(SpecialRestriction.Staff)]
    [SubController("/staff/accounts", true)]
    public class Staff_AccountsController : BasicController
    {


        public ActionResult Index(DataTableParams dataTableParams)
        {
            ViewData["SectionTitle"] = "Accounts";

            using (var db = DataModel.ReadOnly)
            {
                var accounts = (from at in db.Things
                                join a in db.Accounts on at.Id equals a.ThingId
                                join ut in db.Things on at.CreatedByUserId equals ut.Id//todo: when you start tracking ownership, use that column instead.  also note this could create an orphaned record if the owner doesn't exist
                                join u in db.Users on ut.Id equals u.ThingId
                                where at.DeletedOn == null && at.Type == Things.ThingType(typeof(AccountThing)) && (a.Status != AccountStatus.Demo.ToString())
                                select new
                                {
                                    Id = at.Id,
                                    OwnerId = u.ThingId,
                                    Email = u.Email,
                                    u.Name,
                                    IsVerified = u.IsVerified,
                                    CreatedOn = at.CreatedOn,
                                    a.Status,
                                    Plan = a.PlanId,
                                    FileCount = (from t in db.Things
                                                      join f in db.Files on t.Id equals f.ThingId
                                                      where (f.IsSample == null || !f.IsSample.Value) && t.DeletedOn == null && f.DeletePhysicalFile && t.Type == Things.ThingType(typeof(FileThing)) && t.AccountId == at.Id
                                                      select f).Count(),
                                    FileSize = (from t in db.Things
                                                     join f in db.Files on t.Id equals f.ThingId
                                                     where (f.IsSample == null || !f.IsSample.Value) && t.DeletedOn == null && f.DeletePhysicalFile && f.Bytes.HasValue && t.Type == Things.ThingType(typeof(FileThing)) && t.AccountId == at.Id
                                                     select (long?)f.Bytes).Sum().GetValueOrDefault(),
                                    VideoCount = (from t in db.Things
                                                  join v in db.Videos on t.Id equals v.ThingId
                                                  where (v.IsSample == null || !v.IsSample.Value) && t.DeletedOn == null && t.AccountId == at.Id && t.Type == Things.ThingType(typeof(VideoThing))
                                                  select t).Count(),
                                    VideoDuration = (from t in db.Things
                                                     join v in db.Videos on t.Id equals v.ThingId
                                                     where (v.IsSample == null || !v.IsSample.Value) && t.DeletedOn == null && t.AccountId == at.Id && v.Duration.HasValue && t.Type == Things.ThingType(typeof(VideoThing))
                                                     select (long?)v.Duration).Sum().GetValueOrDefault(),
                                    VideoStreamCount = (from t in db.Things
                                                        join f in db.Files on t.Id equals f.ThingId
                                                        where (f.IsSample == null || !f.IsSample.Value) && t.DeletedOn == null && t.AccountId == at.Id && t.Type == Things.ThingType(typeof(VideoStreamThing))
                                                        select t).Count(),
                                    VideoStreamSize = (from t in db.Things
                                                       join f in db.Files on t.Id equals f.ThingId
                                                       where (f.IsSample == null || !f.IsSample.Value) && t.DeletedOn == null && t.AccountId == at.Id && f.DeletePhysicalFile && f.Bytes.HasValue && t.Type == Things.ThingType(typeof(VideoStreamThing))
                                                       select (long?)f.Bytes).Sum().GetValueOrDefault(),
                                    DownloadSize = (from ft in db.FileDownloadLogs
                                                    where ft.AccountId == at.Id
                                                    select (long?)ft.Bytes).Sum().GetValueOrDefault(),
                                    DownloadCount = (from ft in db.FileDownloadLogs
                                                     where ft.AccountId == at.Id
                                                     select ft).Count(),
                                    UploadSize = (from ft in db.FileUploadLogs
                                                  where ft.AccountId == at.Id
                                                  select (long?)ft.Bytes).Sum().GetValueOrDefault(),
                                    UploadCount = (from ft in db.FileUploadLogs
                                                   where ft.AccountId == at.Id
                                                   select ft).Count(),
                                    ProjectCount = (from t in db.Things where t.DeletedOn == null && t.Type == Things.ThingType(typeof(ProjectThing)) && t.AccountId == at.Id select t).Count(),
                                    MemberCount = (from t in db.Things where t.DeletedOn == null && t.Type == Things.ThingType(typeof(AccountMembershipThing)) && t.AccountId == at.Id select t).Count(),
                                });

                if (this.Request.IsAjaxRequest())
                {
                    var response = new DataTableResponse();
                    if (dataTableParams.SortBy.HasItems())
                    {
                        accounts = accounts.OrderBy(string.Join(",", dataTableParams.SortBy));
                    }

                    response.iTotalRecords = accounts.Count();
                    if (dataTableParams.Search.HasChars())
                    {
                        var search = dataTableParams.Search;
                        accounts = accounts.Where(u => u.Email.Contains(search) || u.Name.Contains(search) || u.Plan.Contains(dataTableParams.Search) || u.Status.Contains(dataTableParams.Search));
                        response.iTotalDisplayRecords = accounts.Count();
                    }
                    else
                    {
                        response.iTotalDisplayRecords = response.iTotalRecords;
                    }
                    if (dataTableParams.DisplayLength > 0)
                    {
                        accounts = accounts.Skip(dataTableParams.DisplayStart).Take(dataTableParams.DisplayLength);
                    }
                    var data = accounts.ToArray();
                    //var accountThings = accounts.Select(a => Things.GetOrNull<AccountThing>(a.Id));

                    response.sEcho = dataTableParams.Echo;
                    response.aaData = data.Select(record =>
                    {
                        return new
                        {
                            record.Id,
                            record.OwnerId,
                            record.Plan,
                            CreatedOn = record.CreatedOn.ForJavascript(),
                            record.Name,
                            record.Status,
                            Email = record.Email,
                            IsVerified = record.IsVerified,
                            record.ProjectCount,
                            record.VideoCount,
                            record.VideoDuration,
                            record.VideoStreamCount,
                            record.VideoStreamSize,
                            record.FileCount,
                            record.FileSize,
                            record.UploadCount,
                            record.UploadSize,
                            record.DownloadCount,
                            record.DownloadSize,
                            record.MemberCount,
                        };
                    }).ToArray();
                    return Json(response);
                }
            }
            return View();
        }

        /// <summary>
        /// Show owner details, plan, expiration shit, clear thing cache, impersonate
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Details(int id)
        {
            var accountThing = Things.Get<AccountThing>(id);
            return View(new Staff_Accounts_Details
                            {
                                Id = id,
                                Account = accountThing,
                                RecurlyUrl = "https://{0}.recurly.com/accounts/{1}".FormatString(Recurly.Configuration.RecurlySection.Current.Subdomain, id),
                                Owner = accountThing.Owner,
                            });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var account = Things.Get<AccountThing>(id);
            var model = new Staff_Accounts_Edit
            {
                Id = id,
                OwnerId = account.OwnerUserId,
                PlanId = account.PlanId,
                Status = account.Status,
                TrialPeriodEndsOn = account.TrialPeriodEndsOn,
                CurrentBillingPeriodStartedOn = account.CurrentBillingPeriodStartedOn,
            };
            return View(model);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult Edit(Staff_Accounts_Edit model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var account = Things.Get<AccountThing>(model.Id);
            account.TrackChanges();

            if (model.Status != account.Status)
            {
                account.Status = model.Status;
            }

            //they are changing the ownership
            UserThing newOwner = null;
            if (model.OwnerId != account.OwnerUserId)
            {
                newOwner = Things.GetOrNull<UserThing>(model.OwnerId);
                if (newOwner == null)
                {
                    ModelState.AddModelError("OwnerId", "No user could be found with that ID.");
                    return View(model);
                }
                else if (newOwner.Account != null)
                {
                    ModelState.AddModelError("OwnerId", "The new owner already has an account.  A user cannot have more than one account.");
                    return View(model);
                }
            }

            if (model.PlanId != account.PlanId)
            {
                if (Repos.Plans.Get(model.PlanId) == null)
                {
                    ModelState.AddModelError("PlanId", "Invalid Plan.");
                    return View(model);
                }
                else
                {
                    account.ChangePlan(model.PlanId, false);
                }
            }

            if (model.TrialPeriodEndsOn.HasValue)//Kind was unspecified, which resulted in inequality.
                model.TrialPeriodEndsOn = model.TrialPeriodEndsOn.Value.ToUniversalTime();
            if (model.TrialPeriodEndsOn != account.TrialPeriodEndsOn)
            {
                account.TrialPeriodEndsOn = model.TrialPeriodEndsOn;
            }

            if (model.CurrentBillingPeriodStartedOn.HasValue)//Kind was unspecified, which resulted in inequality.
                model.CurrentBillingPeriodStartedOn = model.CurrentBillingPeriodStartedOn.Value.ToUniversalTime();
            if (model.CurrentBillingPeriodStartedOn != account.CurrentBillingPeriodStartedOn)
            {
                account.CurrentBillingPeriodStartedOn = model.CurrentBillingPeriodStartedOn;
            }

            //delete the current ownership records and add a new one with the new owner
            if (newOwner != null)
            {
                foreach (var membershipThing in DataModel.ReadOnlyQuery(db => from at in db.Things where at.AccountId == account.Id && at.Type == Takeoff.Models.Things.ThingType(typeof(AccountMembershipThing)) select at.Id).Select(id => Things.Get<AccountMembershipThing>(id)))
                {
                    membershipThing.Verify(Permissions.Delete, this.UserThing()).Delete();
                }
                account.OwnerUserId = model.OwnerId;
                account.AddChild(new AccountMembershipThing
                {
                    CreatedByUserId = newOwner.Id,
                    CreatedOn = this.RequestDate(),
                    UserId = newOwner.Id,
                    Role = TakeoffRoles.Owner,
                    TargetId = account.Id,
                    AccountId = account.Id
                }).Insert();
            }

            account.Update();

            return this.RedirectToAction(c => c.Details(model.Id));
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            var account = Things.Get<AccountThing>(id);
            ViewBag.Id = id;
            return View(account);
        }


        [HttpPost]
        public ActionResult Delete(int id, bool deleteUser)
        {
            var account = Things.Get<AccountThing>(id);
            ViewBag.Id = id;
            Accounts.DeleteAccountAndMaybeUser(account.Owner, this.RequestDate(), deleteUser);
            return this.RedirectToAction(c => c.Index(null));
        }
    }


}