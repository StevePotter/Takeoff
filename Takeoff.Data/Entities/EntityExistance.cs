using System;
using System.IO;

namespace Takeoff.Data
{

    public enum EntityExistance
    {
        /// <summary>
        /// The entity is currently in the system.
        /// </summary>
        Active,
        /// <summary>
        /// The entity existed at one point but was deleted.
        /// </summary>
        Deleted,
        /// <summary>
        /// The entity has never existed.
        /// </summary>
        Never
    }

}