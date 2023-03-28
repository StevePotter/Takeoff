using System;
using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Takeoff.Jobs
{
    /// <summary>
    /// Defines a job that runs at a defined interval, like "every 2 hours".  Note that if the service is stopped and started, the job will run right away, so the time interval is not guaranteed to be perfect.
    /// </summary>
    public class IntervalJob : Job
    {
        public IntervalJob()
        {
        }

        /// <summary>
        /// The frequency at which this job ideally will be run.
        /// </summary>
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// Occurs when the task needs to be performed.  You can handle this event to avoid subclassing this.
        /// </summary>
        public event EventHandler<JobTaskEventArgs> PerformingTask;

        public override object FetchTask()
        {
            var lastTask = Tasks.FirstOrDefault();
            if (lastTask == null || lastTask.ErrorOccured || (lastTask.StartedOn + Interval > DateTime.UtcNow))
            {
                return new object();
            }
            return null;
        }

        protected override void OnFetchUnsuccessful()
        {
            System.Threading.Thread.Sleep((int)Interval.TotalMilliseconds / 10);//polling time is 1/10 of the interval.  this is a very safe number
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

    public class JobTaskEventArgs : EventArgs
    {
        public JobTaskEventArgs(JobTask task)
        {
            Task = task;
        }

        public JobTask Task { get; private set; }
    }
}
