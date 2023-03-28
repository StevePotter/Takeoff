using System;
using System.Linq;
using CommandLine;
using Takeoff.Data;
using Takeoff.Models;
using System.Data.Objects;
using System.Text;
using CsvHelper;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Takeoff.DataTools.Commands
{
    /*
     *TEMPORARY COMMAND USED DURING OUR CLOSING TO GET LIST OF EMAIL APOLOGIES
     *
     */

    public class GetAccounts : BaseCommand
    {
        public GetAccounts()
        {
            EnableXmlReport = false;
            NotifyOnErrors = true;
            LogJobInDatabase = false;
        }

        protected override void Perform(string[] commandLineArgs)
        {
            var db = DataModel.ReadOnly;

            var accounts = (from at in db.Things
                            join a in db.Accounts on at.Id equals a.ThingId
                            join ut in db.Things on at.CreatedByUserId equals ut.Id//todo: when you start tracking ownership, use that column instead.  also note this could create an orphaned record if the owner doesn't exist
                            join u in db.Users on ut.Id equals u.ThingId
                            where at.DeletedOn == null && at.Type == Things.ThingType(typeof(AccountThing)) && (a.Status == AccountStatus.Subscribed.ToString() || a.Status == AccountStatus.FreePlan.ToString() || a.Status == AccountStatus.Pastdue.ToString() || a.Status == AccountStatus.Trial.ToString())
                            select new
                            {
                                Id = at.Id,
                                OwnerId = u.ThingId,
                                Email = u.Email,
                                u.Name,
                                u.FirstName,
                                u.LastName,
                                IsVerified = u.IsVerified,
                                CreatedOn = at.CreatedOn,
                                a.Status,
                                Plan = a.PlanId
                            });
            var accountsSql = accounts.ToTraceString();//copy/paste into sql manager

            //StringBuilder text = new StringBuilder();
            //var csv = new CsvWriter(new StringWriter(text));
            //csv.WriteField("Email");
            //csv.WriteField("Name");
            //csv.WriteField("Plan");
            //csv.WriteField("Status");
            //csv.WriteField("CreatedOn");
            //csv.NextRecord();
            //var accts = accounts.ToArray();
            //foreach (var test in accts)
            //{
            //    csv.WriteField(test.Email);
            //    csv.WriteField(test.Name);
            //    csv.WriteField(test.Plan);
            //    csv.WriteField(test.Status.ToString());
            //    csv.WriteField(test.CreatedOn);
            //    csv.NextRecord();
            //}
            //File.WriteAllText("c:\temp\accounts.csv", text.ToString());



            var users = (from ut in db.Things
                         join u in db.Users on ut.Id equals u.ThingId
                         where ut.DeletedOn == null && u.IsVerified && u.Email != "" && u.Email != null
                         select new
                         {
                             Id = ut.Id,
                             Email = u.Email,
                             u.Name,
                             u.FirstName,
                             u.LastName,
                             IsVerified = u.IsVerified,
                             CreatedOn = ut.CreatedOn,
                             HasAccount = (from at in db.Things where at.Type == Things.ThingType(typeof(AccountThing)) && at.OwnerUserId == ut.Id select at).Count() > 0,
                         });
            var usersSql = users.ToTraceString();//copy/paste into sql manager

            var usersWithRecentActivity = (from ut in db.Things
                         join u in db.Users on ut.Id equals u.ThingId
                         where ut.DeletedOn == null && 
                         u.IsVerified && u.Email != "" && u.Email != null &&
                         (from actionSource in db.ActionSources where actionSource.UserId == ut.Id && actionSource.Date > DateTime.Today.Subtract(TimeSpan.FromDays(200)) select actionSource).Count() > 0 &&
                         (from at in db.Things where at.Type == Things.ThingType(typeof(AccountThing)) && at.OwnerUserId == ut.Id select at).Count() == 0

                          select new
                         {
                             Id = ut.Id,
                             Email = u.Email,
                             u.Name,
                             u.FirstName,
                             u.LastName,
                             IsVerified = u.IsVerified,
                             CreatedOn = ut.CreatedOn,
                         });
            var usersWithRecentActivitySql = usersWithRecentActivity.ToTraceString();//copy/paste into sql manager


            //StringBuilder text = new StringBuilder();
            //var csv = new CsvWriter(new StringWriter(text));
            //csv.WriteField("Email");
            //csv.WriteField("Name");
            //csv.WriteField("Plan");
            //csv.WriteField("Status");
            //csv.WriteField("CreatedOn");
            //csv.NextRecord();
            //var accts = accounts.ToArray();
            //foreach (var test in accts)
            //{
            //    csv.WriteField(test.Email);
            //    csv.WriteField(test.Name);
            //    csv.WriteField(test.Plan);
            //    csv.WriteField(test.Status.ToString());
            //    csv.WriteField(test.CreatedOn);
            //    csv.NextRecord();
            //}
            //File.WriteAllText("c:\temp\allusers.csv", text.ToString());

        }

    }

    static class ViewSql
    {
        public static string ToTraceString<T>(this IQueryable<T> target)
        {
            IQueryable query = target as IQueryable;
            if (query == null)
            {
                return null;
            }

            //get provider
            Type tQueryImpl = query.GetType();
            FieldInfo fiContext = tQueryImpl.GetField("context", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fiContext == null)
            {
                return null;
            }

            Object objProvider = fiContext.GetValue(query);
            if (objProvider == null)
            {
                return null;
            }

            var dataContext = objProvider as System.Data.Linq.DataContext;
            if (dataContext == null)
            {
                return null;
            }
            var command = dataContext.GetCommand(query);
            var sql = command.CommandText;
            foreach (var p in command.Parameters.Cast<System.Data.Common.DbParameter>())
            {
                if (p.Value is string)
                {
                    sql = sql.Replace(p.ParameterName, "'" + (string)p.Value + "'");
                }
                else if (p.Value is double || p.Value is int)
                {
                    sql = sql.Replace(p.ParameterName, p.Value.ToString());
                }
                else if (p.Value is DateTime)
                {
                    sql = sql.Replace(p.ParameterName, $"Convert(datetime, '{((DateTime)p.Value).ToString("yyyy-M-d")}')");


                }
                else
                {
                    sql += Environment.NewLine + p.ParameterName + " " + p.Value;
                }
//                    throw new NotImplementedException("Dunno how to replace this sql parameter.");
            }
            return sql;
        }
    }
    
}

