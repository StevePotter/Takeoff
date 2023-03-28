using System;
using System.IO;
using Takeoff.Models;

namespace Takeoff.Data
{
    public interface ISemiAnonymousUser
    {
        Guid Id { get; set; }

        DateTime CreatedOn { get; set; }

        /// <summary>
        /// The thing, currently production or video, that the user has access to.
        /// </summary>
        int? TargetId { get; set; }

        string UserName { get; set; }


    }

}