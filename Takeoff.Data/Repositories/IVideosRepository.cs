using System;
using System.Collections.Generic;

namespace Takeoff.Data
{
    public interface IVideosRepository
    {
        /// <summary>
        /// Returns a video with the given id, or null if the video doesn't exist.  
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IVideo Get(int id);
    }
}
