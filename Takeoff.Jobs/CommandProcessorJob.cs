using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.SQS;
using System.Configuration;
using System.Threading;
using Amazon.SQS.Model;

namespace Takeoff.Jobs
{
    /// <summary>
    /// Watches a queue for commands.  These commands can be directions (like disabled) or reporting or whatever.
    /// </summary>
    /// <remarks>
    /// Each SQS message is a series of commands.  Each line is a command with the following arguments:
    /// 1: jobs.  A space delimited list of job names the command applies to.  "All" means all jobs, including CommandProcessorJob.  "AllBut" is all jobs but CommandProcessorJob.  
    /// 2: command The command that each job will recieve
    /// 
    /// Ex:  Transcode Thumbnail: disable
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// Line 1:  A comma-separated list of jobs this applies to.  "All" means all jobs, including CommandProcessorJob.  "AllBut" is all jobs but CommandProcessorJob.
    /// Line 2:  The command string.  
    /// {
    ///     Jobs: "Transcode,Blah"  //comma separated list of job names the command applies to
    ///     
    /// 
    /// }
    /// 
    /// </remarks>
    public class CommandProcessorJob : SqsJob
    {
        public CommandProcessorJob(Supervisor supervisor, string machineName, string serviceName)
        {
            Supervisor = supervisor;
            this.SqsVisibilityTimeout = TimeSpan.FromMinutes(2);
            this.SleepTimeAfterNoMessageRecieved = TimeSpan.FromSeconds(10);

            _IncomingQueueName = string.Format("{0}_{1}_CommandRequest", machineName, serviceName).ToLowerInvariant();
            _OutgoingQueueName = string.Format("{0}_{1}_CommandResponse", machineName, serviceName).ToLowerInvariant();
        }

        #region Properties

        protected override string IncomingQueueName
        {
            get
            {
                return _IncomingQueueName;
            }
        }
        private string _IncomingQueueName;

        protected override string OutgoingQueueName
        {
            get
            {
                return _OutgoingQueueName;
            }
        }
        private string _OutgoingQueueName;

        private Supervisor Supervisor;

        #endregion

        #region Methods


        /// <summary>
        /// Runs the command on the necessary jobs.  Returns a readout of the results.
        /// </summary>
        /// <param name="fetchResult"></param>
        /// <returns></returns>
        public override JobTask<string> PerformTask(Message fetchResult)
        {
            var task = new JobTask<string>();
            task.Step(() =>
                {
                    StringBuilder result = new StringBuilder();
                    result.AppendLine(fetchResult.MessageId);//first line of result is id of incoming message
                    foreach (var jobCommand in fetchResult.Body.SplitLines().Where(line => line.Contains(':')))
                    {
                        var split = jobCommand.Split(':');
                        if (split.Length == 2)
                        {
                            var command = split[1].Trim().ToLowerInvariant();
                            result.AppendLine("Command: " + command);

                            var jobNames = split[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(s =>s.Trim()).ToArray();
                            List<Job> jobs = new List<Job>();
                            if (jobNames.Contains("All", StringComparer.OrdinalIgnoreCase))
                            {
                                jobs.AddRange(this.Supervisor.Jobs);
                            }
                            else if (jobNames.Contains("AllBut", StringComparer.OrdinalIgnoreCase))
                            {
                                jobs.AddRange(this.Supervisor.Jobs.Where(job => job != this));
                            }
                            else
                            {
                                jobs.AddRange(this.Supervisor.Jobs.Where(job => jobNames.Contains(job.Name, StringComparer.OrdinalIgnoreCase)));
                            }
                            if (jobs.Count == 0)
                            {
                                result.AppendLine("NO JOBS FOUND!");
                            }
                            else
                            {
                                foreach (var job in jobs)
                                {
                                    result.AppendLine("----------------------");
                                    result.AppendLine("Job: " + job.Name);
                                    result.AppendLine(job.ProcessCommand(command));
                                }
                            }
                            result.AppendLine("------------------------------------------------------------------------");
                        }
                    }
                    task.Result = result.ToString();
                });
            return task;            
        }

        #endregion

    }
}
