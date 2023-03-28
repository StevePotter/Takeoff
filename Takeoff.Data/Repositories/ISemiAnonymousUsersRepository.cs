using System;
using System.Collections.Generic;

namespace Takeoff.Data
{
    public interface ISemiAnonymousUsersRepository
    {
        ISemiAnonymousUser Get(Guid id);

        /// <summary>
        /// Creates a fresh instance.
        /// </summary>
        /// <returns></returns>
        ISemiAnonymousUser Instantiate();

        void Update(ISemiAnonymousUser entity);

        void Insert(ISemiAnonymousUser entity);
    }
}
