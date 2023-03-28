using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Takeoff.Jobs
{
    /// <summary>
    /// Manages a bunch of jobs.  Very simple class....maybe not necessary.
    /// </summary>
    public class Supervisor
    {
        public List<Job> Jobs
        {
            get
            {
                if (_Jobs == null)
                {
                    _Jobs = new List<Job>();
                }
                return _Jobs;
            }
        }
        private List<Job> _Jobs;

        public virtual void Start()
        {
            foreach (var job in Jobs)
            {
                job.Start();
            }
        }

        /// <summary>
        /// "Stops" the jobs by disabling them.  
        /// </summary>
        public virtual void Disable()
        {
            foreach (var job in Jobs)
                job.IsEnabled = false;
        }
    }


}
