using System;
using System.Collections.Generic;
using Takeoff.Models;

namespace Takeoff.Data
{
    public interface IBusinessEventsRepository
    {
        void Insert(BusinessEventInsertParams data);

    }
}
