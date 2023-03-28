using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Takeoff.Resources;

namespace Takeoff.Models
{
    /// <summary>
    /// Provides access to various data repositories.
    /// </summary>
    public static class Repos
    {
        public static IUserRepository Users { get; set; }
    }
}