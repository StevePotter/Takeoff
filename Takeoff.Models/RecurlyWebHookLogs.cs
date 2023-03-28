using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Models.Data;

namespace Takeoff.Models
{
    public static class RecurlyWebHookLogs
    {
        public static void Log(RecurlyWebHookLogItem item)
        {
            using (var db = new DataModel())
            {
                db.RecurlyWebHookLogs.InsertOnSubmit(new RecurlyWebHookLog
                                                         {
                                                              AccountId = item.AccountId,
                                                              ReceivedOn = item.ReceivedOn,
                                                              ServerRequestId = item.ServerRequestId,
                                                              Type = item.Type,
                                                              Processed = item.Processed,
                                                         });
                db.SubmitChanges();
            }   
        }
    }


    public class RecurlyWebHookLogItem
    {
        public int? AccountId { get; set; }

        public string Type { get; set; }

        public System.DateTime? ReceivedOn { get; set; }

        public System.Guid? ServerRequestId { get; set; }

        public bool Processed { get; set; }
    }
}
