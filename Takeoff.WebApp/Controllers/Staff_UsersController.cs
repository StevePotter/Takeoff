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
using AutoMapper;

namespace Takeoff.Controllers
{
    [SpecialRestriction(SpecialRestriction.Staff)]
    [SubController("/staff/users", true)]
    public class Staff_UsersController : BasicController
    {


        public ActionResult Index(DataTableParams dataTableParams)
        {

            using (var db = DataModel.ReadOnly)
            {
                var users = (from ut in db.Things
                                join u in db.Users on ut.Id equals u.ThingId
                                where ut.DeletedOn == null && u.Email != null
                                select new
                                {
                                    ut.Id,
                                    u.Email,
                                    u.Name,
                                    u.IsVerified,
                                    ut.CreatedOn,
                                    HasAccount = (from at in db.Things where at.Type == Things.ThingType(typeof(AccountThing)) && at.OwnerUserId == ut.Id select at).Count() > 0,
                                });

                if (this.Request.IsAjaxRequest())
                {
                    var response = new DataTableResponse();
                    if (dataTableParams.SortBy.HasItems())
                    {
                        users = users.OrderBy(string.Join(",", dataTableParams.SortBy));
                    }

                    response.iTotalRecords = users.Count();

                    if (dataTableParams.Search.HasChars())
                    {
                        var search = dataTableParams.Search;
                        users = users.Where(u => u.Email.Contains(search) || u.Name.Contains(search));
                        response.iTotalDisplayRecords = users.Count();
                    }
                    else
                    {
                        response.iTotalDisplayRecords = response.iTotalRecords;
                    }
                    if (dataTableParams.DisplayLength > 0)
                    {
                        users = users.Skip(dataTableParams.DisplayStart).Take(dataTableParams.DisplayLength);
                    }
                    var data = users.ToArray();

                    response.sEcho = dataTableParams.Echo;
                    response.aaData = data.Select(record =>
                    {
                        return new
                        {
                            record.Id,
                            record.Email,
                            record.Name,
                            record.IsVerified,
                            CreatedOn = record.CreatedOn.ForJavascript(),
                            record.HasAccount,
                        };
                    }).ToArray();
                    return Json(response);
                }
            }
            return View();
        }


        /// <summary>
        /// Logs in as the given user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Impersonate(int id)
        {
            IoC.Get<IIdentityService>().SetIdentity(new UserIdentity
                                                        {
                                                            UserId = id
                                                        }, IdentityPeristance.TemporaryCookie, HttpContext);
            return this.RedirectToAction<DashboardController>(c => c.Index(null, null));
        }


        public ActionResult Details(int id)
        {
            var user = Things.Get<UserThing>(id);

            var model = new Staff_Users_Details
            {
                Id = user.Id,
                AccountId = user.Account.MapIfNotNull(a => new int?(a.Id)),
                Name = user.DisplayName,
                Email = user.Email,
                IsVerified = user.IsVerified,
                VerificationKey = user.VerificationKey,

            };
            return View(model);
        }


        [HttpGet]
        public ActionResult Edit(int id)
        {
            var user = Things.Get<UserThing>(id);
            var model = new Staff_Users_Edit();
            AutoMapper.Mapper.Map(user,model);
            model.Password = null;//automapper sets password so we gotta clear it
            return View(model);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult Edit(Staff_Users_Edit model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = Things.Get<UserThing>(model.Id);
            user.TrackChanges();

            if (model.Email.HasChars() )
            {
                model.Email = model.Email.ToLowerInvariant();//turn email addresses into lowercase
                user.Email = model.Email;
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UpdateDisplayName();
            if (model.Password.HasChars())
            {
                user.UpdatePassword(model.Password);
            }

            if (model.IsVerified != user.IsVerified)
                user.IsVerified = model.IsVerified;


            user.Update();
            return this.RedirectToAction(c => c.Details(model.Id));
        }



        [HttpGet]
        public ActionResult Delete(int id)
        {
            var user = Things.Get<UserThing>(id);
            ViewBag.Id = id;
            if (user.Account != null)
            {
                ViewBag.AccountId = user.Account.Id;
                return View("Delete-MustDeleteAccount");
            }
            return View();
        }


        [ActionName("Delete")]
        [HttpPost]
        public ActionResult DeletePost(int id)
        {
            var user = Things.Get<UserThing>(id);
            Accounts.DeleteAccountAndMaybeUser(user, this.RequestDate(), true);
            return this.RedirectToAction(c => c.Index(null));
        }
    }


}