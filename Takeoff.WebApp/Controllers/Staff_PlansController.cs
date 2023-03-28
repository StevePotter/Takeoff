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

namespace Takeoff.Controllers
{
    [SpecialRestriction(SpecialRestriction.Staff)]
    [SubController("/staff/plans", true)]
    public class Staff_PlansController : BasicController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewData["SectionTitle"] = "Plans";
        }

        public ActionResult Index()
        {
            ViewData.Model = Repos.Plans.Get();
            return View();
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new Staff_Plans_Plan
            {                
            });
        }
 
        [HttpPost]
        [ValidateJsonAntiForgeryToken()]
        public ActionResult Create(Staff_Plans_Plan model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.CreatedOn = this.RequestDate();

            var data = Repos.Plans.Insert(Repos.Plans.Instantiate().Once(model.UpdateData));
            if (!data.IsFree)
            {
                try
                {
                    CreateRecurlyPlan(data);
                }
                catch
                {
                    return View("SyncError");
                }
            }

            return this.RedirectToAction(c => c.Index());
        }

        public ActionResult Details(string id)
        {
            var data = Repos.Plans.Get(id);
            if (data == null)
                throw new ArgumentException("Plan with id '" + id + "' could not be found.");

            return View(new Staff_Plans_Plan(data));
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            var data = Repos.Plans.Get(id);
            if (data == null)
                throw new ArgumentException("Plan with id '" + id + "' could not be found.");
            
            return View(new Staff_Plans_Plan(data));
        }

        /// <summary>
        /// todo: don't allow changing of id
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken()]
        public ActionResult Edit(Staff_Plans_Plan model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var data = Repos.Plans.Get(model.Id);
            model.UpdateData(data);
            Repos.Plans.Update(data);

            try
            {
                UpdateRecurlyPlan(data);
            }
            catch
            {
                return View("SyncError");
            }

            return this.RedirectToAction(c => c.Index());
        }


        [HttpPost]
        [ValidateJsonAntiForgeryToken()]
        public ActionResult Delete(string id)
        {
            var accountsWithPlan = DataModel.ReadOnlyQuery(db => db.Accounts.Where(a => a.PlanId == id).Count());
            if (accountsWithPlan > 0)
            {
                throw new InvalidOperationException(accountsWithPlan.ToInvariant() + " accounts currently use this plan.  You cannot delete a plan until no accounts use it.");
            }
            Repos.Plans.Delete(id);
            try
            {
                DeleteRecurlyPlan(id);
            }
            catch
            {
                return View("SyncError");
            }
  
            return this.RedirectToAction(c => c.Index());
        }


        [HttpGet]
        public ActionResult Sync()
        {
            return View();
        }

        /// <summary>
        /// Updates Recurly plans to match the plans in our database.
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Sync(string planId)
        {
            IPlan[] recurlyPlans = null;
            RecurlyClient.PerformRequest(RecurlyClient.HttpRequestMethod.Get, "/company/plans", (reader) =>
            {
                recurlyPlans = XDocument.Load(reader).Descendants("plan").Select(e => FromRecurlyPlan(e)).ToArray();
            });


            recurlyPlans.SyncTo(Repos.Plans.Get(), p => p.Id, p => p.Id,
                (takeoffPlan, recurlyPlan) => ArePlansEqual(takeoffPlan, recurlyPlan), plansToAdd =>
                        {
                            //these plans were in db but not recurly.  the only trick here is that free plans don't go in recurly
                            plansToAdd.Where(p => !p.IsFree).Each(data =>
                                {
                                  CreateRecurlyPlan(data);
                                });
                        },
                        plansToDelete =>
                        {
                            //these plans are in recurly but not our db, so kill em!
                            plansToDelete.Each(data =>
                                {
                                    DeleteRecurlyPlan(data.Id);
                                });
                        }, plansToUpdate =>
                        {
                            plansToUpdate.Each(toUpdate =>
                            {
                                UpdateRecurlyPlan(toUpdate.Source);
                            });
                        }
                        );
            return this.Empty();
        }
        
        /// <summary>
        /// Calls the Recurly API to createa a new plan.
        /// </summary>
        /// <param name="data"></param>
        private void CreateRecurlyPlan(IPlan data)
        {

            XDocument xDoc = null;
            RecurlyClient.PerformRequest(RecurlyClient.HttpRequestMethod.Post, "/company/plans", (writer) =>
            {
                WriteRecurlyPlanXml(writer, data);
            }, (reader) =>
            {
                xDoc = XDocument.Load(reader);
            });
        }


        /// <summary>
        /// Deletes a plan from Recurly.
        /// </summary>
        /// <param name="id"></param>
        private static void DeleteRecurlyPlan(string id)
        {
            RecurlyClient.PerformRequest(RecurlyClient.HttpRequestMethod.Delete, "/company/plans/" + id);
        }

        /// <summary>
        /// Indicates whether the plans from Takeoff's DB and Recurly's API are equivalent.
        /// </summary>
        /// <param name="fromTakeoffDb"></param>
        /// <param name="fromRecurly"></param>
        /// <returns></returns>
        private static bool ArePlansEqual(IPlan fromTakeoffDb, IPlan fromRecurly)
        {
            return fromRecurly.Title.EqualsCaseSensitive(fromTakeoffDb.Title)
                    && fromRecurly.Notes.EqualsCaseSensitive(fromTakeoffDb.Notes)
                    && fromRecurly.PriceInCents == fromTakeoffDb.PriceInCents
                    && fromRecurly.BillingIntervalLength == fromTakeoffDb.BillingIntervalLength
                    && fromRecurly.BillingIntervalType == fromTakeoffDb.BillingIntervalType
                    && fromRecurly.TrialIntervalLength == fromTakeoffDb.TrialIntervalLength
                    && fromRecurly.TrialIntervalType == fromTakeoffDb.TrialIntervalType;
        }

        private void UpdateRecurlyPlan(IPlan data)
        {
            RecurlyClient.PerformRequest(RecurlyClient.HttpRequestMethod.Put, "/company/plans/" + data.Id, (writer) =>
            {
                WriteRecurlyPlanXml(writer, data);
            });
        }

        /// <summary>
        /// Creates a takeoff plan with the necessary properties from the Recurly plan taken from their API.
        /// </summary>
        /// <param name="recurlyPlan"></param>
        /// <returns></returns>
        static IPlan FromRecurlyPlan(XElement recurlyPlan)
        {
            return Repos.Plans.Instantiate().Once(p =>
                                                  {
                                                      p.Id = recurlyPlan.Element("plan_code").ValueOrDefault();
                                                      p.Title = recurlyPlan.Element("name").ValueOrDefault();
                                                      p.Notes = recurlyPlan.Element("description").ValueOrDefault();
                                                      p.PriceInCents =
                                                          recurlyPlan.Element("unit_amount_in_cents").ValueOrDefault
                                                              <int>();
                                                      p.BillingIntervalLength =
                                                          recurlyPlan.Element("plan_interval_length").ValueOrDefault
                                                              <int>();
                                                      p.BillingIntervalType =
                                                          Enum<PlanInterval>.Parse(
                                                              recurlyPlan.Element("plan_interval_unit").ValueOrDefault(),
                                                              true);
                                                      p.TrialIntervalLength =
                                                          recurlyPlan.Element("trial_interval_length").ValueOrDefault
                                                              <int>();
                                                      p.TrialIntervalType =
                                                          Enum<PlanInterval>.Parse(
                                                              recurlyPlan.Element("trial_interval_unit").
                                                                  ValueOrDefault(),true);
                                                  });
        }

        protected void WriteRecurlyPlanXml(XmlTextWriter xmlWriter, IPlan plan)
        {
            xmlWriter.WriteStartElement("plan");
            xmlWriter.WriteElementString("plan_code", plan.Id);
            xmlWriter.WriteElementString("name", plan.Title);
            xmlWriter.WriteElementString("description", plan.Notes);
            xmlWriter.WriteElementString("unit_amount_in_cents", plan.PriceInCents.ToInvariant());
            xmlWriter.WriteElementString("plan_interval_length", plan.BillingIntervalLength.ToInvariant());
            xmlWriter.WriteElementString("plan_interval_unit", plan.BillingIntervalType.ToString().ToLowerInvariant());
            xmlWriter.WriteElementString("trial_interval_length", plan.TrialIntervalLength.ToInvariant());
            xmlWriter.WriteElementString("trial_interval_unit", plan.TrialIntervalType.ToString().ToLowerInvariant());
            xmlWriter.WriteEndElement();
        }

    }
}