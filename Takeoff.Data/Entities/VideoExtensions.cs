using System;
using System.IO;
using System.Linq;
using Takeoff.Models;

namespace Takeoff.Data
{

    /// <summary>
    /// Helper extensions for the IPlan interface.
    /// </summary>
    public static class VideoExtensions
    {

        public static bool IsGuestAccessEnabled(this IVideo video)
        {
            return video.GuestPassword.HasChars();
        }


    }

}