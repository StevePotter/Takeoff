using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Models;

namespace Takeoff.Data
{
    public interface IAccountsRepository
    {
        /// <summary>
        /// Returns the given account, or null if it doesn't exist.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IAccount Get(int id);

        /// <summary>
        /// Gets the account that is owned by the given user, but only if the account has been deleted.  
        /// </summary>        
        IAccount GetDeletedAccount(int ownerUserId);

        void BeginUpdate(IAccount entity);

        void Update(IAccount entity);

    }
}
