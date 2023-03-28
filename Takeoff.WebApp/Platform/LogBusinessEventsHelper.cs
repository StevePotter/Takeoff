using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Newtonsoft.Json;
using Takeoff.Controllers;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff
{
    public static class LogBusinessEventsHelper
    {
        public static DateTime UtcToEst(this DateTime value)
        {
//            TimeZoneInfo. todo: this should use time zone
            return new DateTime(value.Ticks - TimeSpan.FromMinutes(240).Ticks, DateTimeKind.Local);
        }

        public static void LogBusinessEvent(this Controller controller, string eventType, object customProperties = null, int? userId = null, int? accountId = null, bool sendEmail = false)
        {
            IUser user;
            if (userId.HasValue)
            {
                user = controller.Repository<IUsersRepository>().Get(userId.Value);
            }
            else
            {
                user = controller.UserThing2();
                if (user != null)
                    userId = user.Id;
            }
            IAccount account;
            if (accountId.HasValue)
            {
                account = controller.Repository<IAccountsRepository>().Get(accountId.Value);                
            }
            else
            {
                account = user.MapIfNotNull(u => u.Account);
                if (account != null)
                    accountId = account.Id;
            }

            var bizEvent = new BusinessEventInsertParams
                               {
                                   Type = eventType,
                                   OccuredOn = controller.RequestDate(),
                                   UserId = userId,
                                   AccountId = accountId,
                                   RequestId = controller.RequestLogId(),
                               };

            if ( customProperties != null)
            {
                bizEvent.Parameters = new Dictionary<string, object>();
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(customProperties))
                {
                    bizEvent.Parameters[property.Name] = property.GetValue(customProperties);
                }
            }

            controller.Repository<IBusinessEventsRepository>().Insert(bizEvent);

            if ( sendEmail)
            {
                var to = ConfigUtil.AppSetting("SendBusinessEventEmailsTo");
                if (to.HasChars())
                {
                    SendEmail(account, user, to, bizEvent);
                }
            }
        }

        private static void SendEmail(IAccount account, IUser user, string to, BusinessEventInsertParams bizEventInsertParams)
        {
            var message = new OutgoingMessage
                              {
                                  To = to,
                                  Subject = "Biz Event " + bizEventInsertParams.Type,
                                  Template = "BusinessEvent",
                              };
            var body = new StringBuilder();
            Action<string, string> addNameValue =
                (name, value) => body.AppendLine(name.CharsOrEmpty() + ": " + value.CharsOrEmpty());
            addNameValue("Event Type", bizEventInsertParams.Type);
            addNameValue("Occured (est)", bizEventInsertParams.OccuredOn.UtcToEst().ToString(DateTimeFormat.ShortDateTime));
            addNameValue("Occured (utc)", bizEventInsertParams.OccuredOn.ToString(DateTimeFormat.ShortDateTime));
            bizEventInsertParams.RequestId.IfHasVal(requestId => addNameValue("Request Id", requestId.ToString()));
            bizEventInsertParams.TargetId.IfHasVal(targetId => addNameValue("Target Id", targetId.ToInvariant()));
            bizEventInsertParams.UserId.IfHasVal(id => addNameValue("User Id", id.ToInvariant()));
            if (user != null)
            {
                addNameValue("User Name", user.DisplayName);
                addNameValue("User Email", user.Email);
            }
            bizEventInsertParams.AccountId.IfHasVal(id => addNameValue("Account Id", id.ToInvariant()));
            if (account != null)
            {
                addNameValue("Account Status", account.Status.ToString());
                if (account.InTrialPeriod())
                {
                    addNameValue("Trial Ends", account.TrialPeriodEndsOn.Value.ToString(DateTimeFormat.ShortDateTime));
                }
                account.Plan.IfNotNull(plan => addNameValue("Plan", plan.Id));
            }

            if (bizEventInsertParams.Parameters.HasItems())
            {
                body.AppendLine("Parameters:");

                foreach (var pair in bizEventInsertParams.Parameters)
                {
                    body.Append("  "); //slight indent
                    addNameValue(pair.Key, FormatParameterValueForEmail(pair.Value));
                }
            }
            message.TextBody = body.ToString();
            OutgoingMail.Send(message);
        }

        private static string FormatParameterValueForEmail(object value)
        {
            if (value == null)
                return "[null]";

            if ( value is DateTime)
            {
                return value.CastTo<DateTime>().ToString(DateTimeFormat.ShortDateTime);
            }
            return Convert.ToString(value);
        }

    }

}