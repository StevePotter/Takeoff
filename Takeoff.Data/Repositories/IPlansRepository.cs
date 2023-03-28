using System;
using System.Collections.Generic;

namespace Takeoff.Data
{
    public interface IPlansRepository
    {
        IPlan Get(string id);

        IEnumerable<IPlan> Get();

        /// <summary>
        /// Gets the plans that can currently be purchased, sorted by price ascending.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IPlan> GetPlansForSale();

        void Delete(string id);

        void Update(IPlan entity);

        IPlan Instantiate();

        IPlan Insert(IPlan entity);
    }
}
