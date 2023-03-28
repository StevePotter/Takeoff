using System;
using System.IO;
using System.Linq;
using Takeoff.Models;

namespace Takeoff.Data
{

    /// <summary>
    /// Helper extensions for the IProduction interface.
    /// </summary>
    public static class ProductionExtensions
    {

        public static bool IsGuestAccessEnabled(this IProduction production)
        {
            return production.GuestPassword.HasChars();
        }

    }

}