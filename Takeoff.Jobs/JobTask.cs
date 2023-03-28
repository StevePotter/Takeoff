using System;
using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using System.Collections.Generic;

namespace Takeoff.Jobs
{

    /// <summary>
    /// A single task performed by a job.  
    /// </summary>
    /// <remarks>
    /// This is optional for a job to use but is helpful by providing a few benefits:
    /// - Broken down into steps.  Each step can be timed, logged, and repeated upon error.
    /// - Logging.  An XML document is returned that contains details of each step.  It can then be saved to S3, the server, whatever.
    /// </remarks>
    public class JobTask
    {
        #region Properties

        /// <summary>
        /// Indicates the name of the root element for this task.  Element will include a few attributes like date/time started.
        /// </summary>
        public string ReportRootElementName { get; set; }

        /// <summary>
        /// Contains details about the transcode job, which is useful for analyzing performance, knowing more about what kinds of video (type, size, etc) are being uploaded, and error tracing.
        /// The contents of this are saved into an S3 bucket defined in app settings "ReportBucket" key.
        /// </summary>
        public XDocument Report { get; private set; }

        /// <summary>
        /// The current xml element being written to for the report.
        /// </summary>
        protected XElement CurrentReportElement { get; private set; }

        /// <summary>
        /// Indicates critical errors that occured during one or more steps.
        /// </summary>
        public Exception[] Errors { get; set; }

        /// <summary>
        /// When true (default is false), an xml report that includes steps and other information, will be available.  Otherwise it will be ignored. 
        /// </summary>
        public bool EnableXmlReport { get; set; }

        /// <summary>
        /// The time (UTC) that the first step in this task was run.
        /// </summary>
        public DateTime? StartedOn { get; private set; }

        /// <summary>
        /// The time (UTC) that the task was completed.
        /// </summary>
        public DateTime? EndedOn { get; private set; }


        public virtual bool ErrorOccured
        {
            get
            {
                return Errors != null && Errors.Length > 0;
            }
        }

        #endregion

        #region Methods

        public JobTask()
        {
            ReportRootElementName = "Task";
        }

        /// <summary>
        /// Occurs when a step has failed the maximum number of times.  
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnErrorOccured(Exception ex)
        {
            if (Errors == null)
                Errors = new Exception[] { ex };
            else
            {
                var e = new List<Exception>();
                e.AddRange(Errors);
                e.Add(ex);
                Errors = e.ToArray();
            }
        }

        /// <summary>
        /// Called by Job, this marks the job as complete.
        /// </summary>
        protected internal virtual void OnEnded()
        {
            this.EndedOn = DateTime.UtcNow;
        }

        /// <summary>
        /// Writes the given line of text to the output.
        /// </summary>
        /// <param name="text"></param>
        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }

        /// <summary>
        /// Represents around a step called "Step" in the task. 
        /// </summary>
        public void Step(Action work)
        {
            Step("Step", false, work);
        }

        /// <summary>
        /// Represents around a step in the task.  No retries are allowed.  
        /// </summary>
        public void Step(string name, Action work)
        {
            Step(name, false, work);
        }

        /// <summary>
        /// Represents around a step in the task.  No retries are allowed.  
        /// </summary>
        public void Step(string name, bool runIfErrorOccured, Action work)
        {
            Step(name, runIfErrorOccured, 1, TimeSpan.Zero, work);
        }

        /// <summary>
        /// Represents around a step in the task.  If an exception occurs during this, retries are allowed.  This method shouldn't throw an exception.  
        /// </summary>
        public void Step(string name, bool runIfErrorOccured, int maxTries, TimeSpan betweenTries, Action work)
        {
            Step(name, runIfErrorOccured, maxTries, betweenTries, true, work);
        }

        /// <summary>
        /// Represents around a step in the task.  If an exception occurs during this, retries are allowed.  This method shouldn't throw an exception.  
        /// </summary>
        /// <param name="name"></param>
        /// <param name="work"></param>
        public void Step(string name, bool runIfErrorOccured, int maxTries, TimeSpan betweenTries, bool addToReport, Action work)
        {
            if (!StartedOn.HasValue)
                StartedOn = DateTime.UtcNow;

            if (!EnableXmlReport && addToReport)
                addToReport = false;
            if (maxTries < 1)
                throw new ArgumentOutOfRangeException("maxTries");

            if (!runIfErrorOccured && ErrorOccured)
            {
                return;//bypass if an error occurs
            }

            WriteLine("Start " + name);
            var watch = Stopwatch.StartNew();
            //append the element and create the document if necessary
            var previousReportElement = CurrentReportElement;
            if (addToReport)
            {
                var element = new XElement("Step");
                element.SetAttributeValue("Name", name);
                if (Report == null)
                {
                    Report = new XDocument();
                    var root = new XElement(ReportRootElementName);
                    Report.Add(root);
                    root.SetAttributeValue("StartedOn", DateTime.Now.ToString(DateTimeFormat.ShortDateTime));//put the start date/time for the initial element
                    root.Add(element);
                    previousReportElement = root;
                }
                else
                {
                    previousReportElement.Add(element);//append this to the report
                }
                CurrentReportElement = element;
            }

            for (var i = 0; i < maxTries; i++)
            {
                try
                {
                    work();//if you want to be able to get the current state of the job ("DownloadVideo, running 15secs."), you'd have to make the asynchronous
                    break;
                }
                catch (Exception ex)
                {
                    var exReport = ex.Report();
                    WriteLine("Exception Occured: " + exReport);

                    if (addToReport)
                    {
                        var msgElement = new XElement("Error");
                        msgElement.SetAttributeValue("Attempt", i.ToInvariant());
                        msgElement.SetValue(exReport);
                        CurrentReportElement.Add(msgElement);
                    }
                    if (i == maxTries - 1)
                    {
                        OnErrorOccured(ex);

                        if (addToReport)
                        {
                            CurrentReportElement.SetAttributeValue("Failed", true);
                        }
                        break;
                    }
                    else
                    {
                        if (betweenTries > TimeSpan.Zero)
                            Thread.Sleep(betweenTries);
                    }
                }
            }

            watch.Stop();
            if (addToReport)
            {
                CurrentReportElement.SetAttributeValue("Duration", Math.Round(watch.Elapsed.TotalSeconds, 3));
                CurrentReportElement = previousReportElement;
                Report.Root.SetAttributeValue("EndedOn", DateTime.Now.ToString(DateTimeFormat.ShortDateTime));
            }
            WriteLine("End " + name + ".  Took " + watch.Elapsed.ToString());
        }

        /// <summary>
        /// Add an xml attribute to the current report element.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddReportAttribute(XName name, object value)
        {
            WriteLine(name.ToString() + " = " + value.ToString());
            if (EnableXmlReport)
            {
                CurrentReportElement.SetAttributeValue(name, value);
            }
        }

        /// <summary>
        /// Add an element to the current report element that contains the message passed.
        /// </summary>
        public void AddReportMessage(string message)
        {
            WriteLine(message);

            if (EnableXmlReport)
            {
                var msgElement = new XElement("Message");
                msgElement.SetValue(message);
                CurrentReportElement.Add(msgElement);
            }
        }

        #endregion
    }


    /// <summary>
    /// A JobTask that returns some kind of object that can be used for reporting purposes or whatever.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JobTask<T> : JobTask
    {
        public T Result { get; set; }
    }

}
