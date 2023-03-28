using System;
using System.Collections.Generic;
using System.IO;

namespace Takeoff.Data
{

    public class BusinessEventInsertParams
    {
        public Guid? Id { get; set; }

        public string Type { get; set; }

        public DateTime OccuredOn { get; set; }

        public int? UserId { get; set; }

        public int? AccountId { get; set; }

        public int? TargetId { get; set; }

        public Guid? RequestId { get; set; }

        public Dictionary<string, object> Parameters { get; set; }
    }


}