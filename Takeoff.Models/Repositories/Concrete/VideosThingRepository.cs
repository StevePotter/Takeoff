using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.ThingRepositories
{
    public class VideosThingRepository : IVideosRepository
    {

        public IVideo Get(int id)
        {
            return Things.GetOrNull<VideoThing>(id);
        }
    }
}
