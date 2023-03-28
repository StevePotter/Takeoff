using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Takeoff.Data
{
    public interface IChange
    {
        int Id { get; set; }

        System.DateTime Date { get; set; }

        int UserId { get; set; }

        int ThingId { get; set; }

        string ThingType { get; set; }

        System.Nullable<int> ThingParentId { get; set; }

        string Action { get; set; }

        string Data { get; set; }

        string Description { get; set; }
    }

}
