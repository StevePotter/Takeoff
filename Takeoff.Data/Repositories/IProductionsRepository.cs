using System;
using System.Collections.Generic;

namespace Takeoff.Data
{
    public interface IProductionsRepository
    {
        /// <summary>
        /// Returns a production with the given id, or null if it doesn't exist.  
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IProduction Get(int id);

        int GetSampleProductionId(int accountId);
    }
}
