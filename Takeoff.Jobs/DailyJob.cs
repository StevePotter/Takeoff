using System;
using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Takeoff.Jobs
{
    /// <summary>
    /// Defines a job that runs once a day.
    /// </summary>
    public class DailyJob : Job
    {
        public DailyJob()
        {
        }

        /// <summary>
        /// Occurs when the task needs to be performed.  You can handle this event to avoid subclassing this.
        /// </summary>
        public event EventHandler<JobTaskEventArgs> PerformingTask;

        public override object FetchTask()
        {
            var lastTask = Tasks.FirstOrDefault();
            if (lastTask == null || lastTask.ErrorOccured || (lastTask.StartedOn.GetValueOrDefault().DayOfYear != DateTime.Now.DayOfYear))
            {
                return new object();
            }
            return null;
        }

        protected override void OnFetchUnsuccessful()
        {
            System.Threading.Thread.Sleep(TimeSpan.FromMinutes(30));
        }

        public virtual JobTask PerformTask()
        {
            var task = new JobTask();
            if (PerformingTask != null)
            {
                PerformingTask(this, new JobTaskEventArgs(task));
            }

            return task;
        }

        public sealed override JobTask PerformTask(object fetchResult)
        {
            return PerformTask();
        }
    }


}
