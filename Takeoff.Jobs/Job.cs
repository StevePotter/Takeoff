using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Takeoff.Jobs
{
    /// <summary>
    /// Performs a certain type of task.  First it finds a task (like getting message from a queue) and then performs the task.  Then it repeats.  Think of this in terms a real-world job:  secretary, truck driver, etc.
    /// </summary>
    /// <remarks>
    /// Under the covers it's all done asynchronously using .NET 4.0's new parallel task library
    /// </remarks>
    public abstract class Job
    {
        public Job()
        {
            CancellationTokenSource = new CancellationTokenSource();
            Status = TaskStatus.Idle;
            TasksToStore = 1000;
        }

        /// <summary>
        /// Gets or sets a name for this job.  Useful for logging.  If not set, it will default to the type name (without "Job" at the end)
        /// </summary>
        public string Name
        {
            get
            {
                return _Name ?? GetType().Name.EndWithout("Job");
            }
            set
            {
                _Name = value;
            }
        }
        string _Name;

        public bool IsEnabled
        {
            get
            {
                return _IsEnabled;
            }
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;
                    if (value)
                    {
                        Debug.Assert(Status == TaskStatus.Idle);
                        Start();
                    }
                }
            }
        }
        private bool _IsEnabled = true;

        public TaskStatus Status { get; private set; }

        /// <summary>
        /// The maximum number of tasks that can be held in Tasks. Default is 1000.
        /// </summary>
        public int TasksToStore { get; set; }

        /// <summary>
        /// Gets a list of tasks that have run, including one that may be running.  Tasks are ordered by age, so the current task will be at 0, last at TasksToStore-1.
        /// </summary>
        public JobTask[] Tasks
        {
            get
            {
                return _tasks.ToArray();
            }
        }
        private List<JobTask> _tasks = new List<JobTask>();

        private CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Waits for a queue message or something then returns an action that represents the job to be executed.  Should return null if it is not available.  Otherwise it should return some object (like a message) that will be passed to Runjob) Should return immediately after finding or not finding the job, or if it was cancelled.
        /// </summary>
        /// <returns></returns>
        public abstract object FetchTask();

        /// <summary>
        /// Called after FetchTask returns a result indicating no job was picked up.  After this is called, FetchTask will be run again right away.  For something like SQS, this gives an opportunity to wait a bit before hitting SQS, which cuts down on the number of requests.
        /// </summary>
        /// <remarks>This should not throw an exception.</remarks>
        protected virtual void OnFetchUnsuccessful()
        {

        }

        protected virtual void OnFetchException(Exception ex)
        {

        }

        protected virtual void OnTaskException(Exception ex)
        {

        }


        /// <summary>
        /// Called after FetchTask if the task was disabled during FetchTask.  This should "undo" anything done in FetchTask, such as returning a message to the queue.
        /// </summary>
        /// <param name="fetchResult"></param>
        protected virtual void OnDisabledDuringFetch(object fetchResult)
        {

        }

        /// <summary>
        /// Creates a task and runs steps within it.  This shouldn't throw an exception.
        /// </summary>
        /// <param name="fetchResult"></param>
        /// <returns></returns>
        public abstract JobTask PerformTask(object fetchResult);

        public virtual void Start()
        {
            if (Status != TaskStatus.Idle)
                throw new InvalidOperationException("Can't call Start unless it's Idle");

            if (!IsEnabled)
                throw new InvalidOperationException("Can't call Start when disabled.");

            WriteLine("Starting");

            Status = TaskStatus.FetchingJob;
            Task task = null;
            task = new Task(() =>
            {
                object fetch;
                try
                {
                    WriteLine("Fetching task");
                    fetch = FetchTask();
                }
                catch (Exception ex)
                {
                    WriteLine("Exception while fetching: " + ex.Report());
                    OnFetchException(ex);
                    Status = TaskStatus.Idle;
                    if (IsEnabled)
                    {
                        task.ContinueWith((t2) => Start(), TaskContinuationOptions.LongRunning);
                    }
                    return;
                }

                if (fetch != null)
                {
                    task.ContinueWith((t) =>
                    {
                        Status = TaskStatus.RunningJob;
                        if (IsEnabled)
                        {
                            JobTask jobTask = null;
                            try
                            {
                                WriteLine("Performing task");
                                jobTask = PerformTask(fetch);
                            }
                            catch( Exception ex )
                            {
                                WriteLine("CRUCIAL Exception while performing task: " + ex.Report());
                                OnTaskException(ex);
                            }
                            //add to tasks list
                            if (jobTask != null)
                            {
                                jobTask.OnEnded();
                                _tasks.Insert(0, jobTask);
                                while (_tasks.Count > TasksToStore)
                                    _tasks.RemoveAt(_tasks.Count - 1);
                            }
                        }
                        else
                        {
                            WriteLine("Disabled while fetching task");
                            try
                            {
                                OnDisabledDuringFetch(fetch);
                            }
                            catch( Exception ex )
                            {
                                WriteLine("CRUCIAL Exception after OnDisabledDuringFetch: " + ex.Report());
                            }
                        }
                        Status = TaskStatus.Idle;
                        if (IsEnabled)
                        {
                            task.ContinueWith((t2) => Start(), TaskContinuationOptions.LongRunning);
                        }
                    }, TaskContinuationOptions.LongRunning);
                }
                else
                {
                    task.ContinueWith((t) =>
                    {
                        WriteLine("No task found");
                        try
                        {
                            OnFetchUnsuccessful();
                        }
                        catch (Exception ex)
                        {
                            WriteLine("CRUCIAL Exception after OnFetchUnsuccessful: " + ex.Report());
                        }
                        Status = TaskStatus.Idle;
                        if (IsEnabled)
                        {
                            task.ContinueWith((t2) => Start(), TaskContinuationOptions.LongRunning);
                        }
                    }, TaskContinuationOptions.LongRunning);
                }
            }, CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            task.Start();//didn't use Factory.StartNew because sometimes within the action the "task" variable was null.  No idea why but this fixed it.
        }



        /// <summary>
        /// Writes the given line of text to the output stream.  Includes date and name.
        /// </summary>
        /// <param name="text"></param>
        protected void WriteLine(string text)
        {
            Console.WriteLine(DateTime.Now.ToString(DateTimeFormat.ShortDateTime) + " - " + Name + " - " + text);
        }


        /// <summary>
        /// Processes the given command, giving a friendly readout.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected internal virtual string ProcessCommand(string command)
        {
            if (command.Equals("disable", StringComparison.OrdinalIgnoreCase))
            {
                if (!this.IsEnabled)
                    return "Already disabled";
                this.IsEnabled = false;
                return "Disabled";
            }
            if (command.Equals("enable", StringComparison.OrdinalIgnoreCase))
            {
                if (this.IsEnabled)
                    return "Already enabled";
                this.IsEnabled = true;
                return "Enabled";
            }
            if (command.Equals("status", StringComparison.OrdinalIgnoreCase))
            {
                return ReportStatus().ToString();
            }
            return null;
        }

        /// <summary>
        /// Gives a status report.  Response to "status" command.
        /// </summary>
        /// <returns></returns>
        protected virtual StringBuilder ReportStatus()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Enabled: " + IsEnabled.ToString());
            sb.AppendLine("Status: " + Status.ToString());
            sb.AppendLine(Math.Min(Tasks.Length,3).ToInvariant() + " most recent tasks");
            for( var i = 0; i < Math.Min(Tasks.Length,3); i++ )
            {
                var task = Tasks[i];
                sb.Append("     Started: " + (task.StartedOn.HasValue ? task.StartedOn.Value.ToLocalTime().ToString(DateTimeFormat.ShortDateTime) : "Never"));
                sb.Append("  Ended: " + (task.EndedOn.HasValue ? task.EndedOn.Value.ToLocalTime().ToString(DateTimeFormat.ShortDateTime) : "Never"));
                sb.Append("  Error Occured: " + task.ErrorOccured.ToString());                
            }
            return sb;
        }

    }

    public enum TaskStatus
    {
        Idle,
        FetchingJob,
        RunningJob
    }


}
