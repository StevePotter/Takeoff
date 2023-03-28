using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Amazon.S3.Model;
using CommandLine;
using Takeoff.Models;
using Takeoff.Models.Data;
using Amazon.S3;

namespace Takeoff.DataTools.Commands
{
    public abstract class BaseCommandWithOptions<TOptions> : BaseCommand
    {
        public TOptions Options { get; private set; }

        protected sealed override void Perform(string[] commandLineArgs)
        {
            var options = Activator.CreateInstance<TOptions>();
            Options = options;
            var parser = CommandLine.Parser.Default;
            var helpText = new StringBuilder();
            if (parser.ParseArguments(commandLineArgs, options))
            {
                Perform(options);
            }
            else
            {
                Console.WriteLine("Error in command line arguments.  Exiting.");
                throw new Exception("Arguments couldn't be parsed.");
            }
        }

        protected abstract void Perform(TOptions arguments);

    }

    public class StepParams
    {
        public StepParams()
        {
            WriteStartAndEndToConsole = true;
        }
        public string Name { get; set; }

        public bool RunIfErrorOccured { get; set; }

        public int MaxTries { get; set; }

        public TimeSpan BetweenTries { get; set; }

        public bool AddToReport { get; set; }

        public System.Action Work { get; set; }

        public bool WriteStartAndEndToConsole { get; set; }
    }

    /// <summary>
    /// A command that performs some Takeoff-related job, such as sending digest emails.  
    /// </summary>
    /// <remarks>
    /// Commands have a few cool features.  First, by using Steps, you are given automatic rich logging to an XML file.  That XML can easily be uploaded to Amazon S3.  
    /// Second, the commands can be logged in the Takeoff db's Job table.  This allows a slick admin section that gives the jist of jobs, with links to each job's details.
    /// </remarks>
    public abstract class BaseCommand
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
        /// When true, the report will be uploaded to S3 for archiving.
        /// </summary>
        public bool UploadXmlReport
        {
            get { return _UploadXmlReport.GetValueOrDefault(EnableXmlReport); }
            set { _UploadXmlReport = value; }
        }
        private bool? _UploadXmlReport;

        public string UploadedXmlReportUrl { get; protected set; }

        /// <summary>
        /// The time (UTC) that the command began running.
        /// </summary>
        public DateTime? StartedOn { get; private set; }

        /// <summary>
        /// The time (UTC) that the command completed.
        /// </summary>
        public DateTime? EndedOn { get; private set; }

        private int CurrentOutputIndent = 0;

        public virtual bool ErrorOccured
        {
            get
            {
                return Errors != null && Errors.Length > 0;
            }
        }

        /// <summary>
        /// Indicates whether this job should have an entry in the database.
        /// </summary>
        public bool LogJobInDatabase { get; set; }

        public Guid JobId
        {
            get { return _jobId.Value; }
        }
        private Lazy<Guid> _jobId = new Lazy<Guid>(Guid.NewGuid);

        public virtual string JobType
        {
            get
            {
                return this.GetType().Name.EndWithout("Command");
            }
        }

        /// <summary>
        /// When true, an email notification will be sent if the job fails.
        /// </summary>
        public bool NotifyOnErrors { get; set; }


        #endregion

        public BaseCommand()
        {
            ReportRootElementName = "Job";
        }

        #region Methods

        protected abstract void Perform(string[] commandLineArgs);

        internal void Run(string[] commandLineArgs)
        {
            Stopwatch jobDuration = Stopwatch.StartNew();
            StartedOn = DateTime.UtcNow;
            if (EnableXmlReport)
            {
                Report = new XDocument();
                var root = new XElement(ReportRootElementName);
                Report.Add(root);
                root.SetAttributeValue("StartedOnUtc", StartedOn.Value.ToString(DateTimeFormat.ShortDateTime));
                root.SetAttributeValue("StartedOnLocal", StartedOn.Value.ToLocalTime().ToString(DateTimeFormat.ShortDateTime));
                root.SetAttributeValue("JobType", JobType);
                root.SetAttributeValue("JobId", JobId.ToString());
                root.SetAttributeValue("Arguments", commandLineArgs.HasItems() ? string.Join(", ", commandLineArgs.Where(arg => arg.HasChars())) : string.Empty);
                CurrentReportElement = root;
            }


            try
            {
                Perform(commandLineArgs);
            }
            catch (Exception ex)
            {
                OnErrorOccured(ex);
            }

            jobDuration.Stop();
            EndedOn = DateTime.UtcNow;
            if (EnableXmlReport)
            {
                Report.Root.SetAttributeValue("EndedOnUtc", EndedOn.Value.ToString(DateTimeFormat.ShortDateTime));
                Report.Root.SetAttributeValue("EndedOnLocal", EndedOn.Value.ToLocalTime().ToString(DateTimeFormat.ShortDateTime));
                Report.Root.SetAttributeValue("DurationSec", jobDuration.Elapsed.TotalSeconds.ToString(NumberFormat.Number, 3));//using stopwatch is more accurate
            }
            if (EnableXmlReport && UploadXmlReport)
            {
                Step("UploadReport", true, 3, TimeSpan.FromSeconds(2), false, UploadReport);
            }
            if (LogJobInDatabase)
            {
                Step("AddJobRecordToDb", true, 2, TimeSpan.FromSeconds(2), false, AddJobRecordToDb);
            }
            if (ErrorOccured && NotifyOnErrors)
            {
                Step("SendErrorReport", true, 2, TimeSpan.FromSeconds(2), false, SendErrorReport);
            }
        }

        /// <summary>
        /// Adds a record of this job to the database.
        /// </summary>
        private void AddJobRecordToDb()
        {
            using (var db = new DataModel())
            {
                var record = new JobLog
                {
                    Id = JobId,
                    StartedOn = StartedOn,
                    JobType = JobType,
                    EndedOn = EndedOn,
                    Status = ErrorOccured ? "Failed" : "Complete",
                };
                db.JobLogs.InsertOnSubmit(record);

                db.SubmitChanges();
            }
        }

        /// <summary>
        /// Uploads the xml report to S3.
        /// </summary>
        protected virtual void UploadReport()
        {
            var xml = Report.ToString(SaveOptions.None);

            using (var s3 = new Amazon.S3.AmazonS3Client(ConfigurationManager.AppSettings["AmazonAccessKey"], ConfigurationManager.AppSettings["AmazonSecretKey"]))
            {
                s3.PutObject(new PutObjectRequest
                {
                    BucketName = ReportUploadBucket,
                    CannedACL = S3CannedACL.AuthenticatedRead,
                    Key = S3FilePath,
                    ContentType = "application/xml",
                    ContentBody = xml,

                });
                UploadedXmlReportUrl = s3.GetPreSignedURL(new GetPreSignedUrlRequest
                {
                    BucketName = ReportUploadBucket,
                    Expires = DateTime.Today.AddYears(10),
                    Key = S3FilePath,
                    Protocol = Protocol.HTTPS,
                });
            }
        }

        protected virtual string ReportUploadBucket
        {
            get
            {
                return ConfigurationManager.AppSettings["JobLogBucket"];
            }
        }


        protected virtual string S3FilePath
        {
            get
            {
                return JobType + "/" + JobId + ".xml";
            }
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

        protected void SendErrorReport()
        {
            var mail = new Models.OutgoingMessage
            {
                From = ConfigurationManager.AppSettings["SendMailFromAddress"],
                To = ConfigurationManager.AppSettings["SendErrorEmailsTo"],
                Subject = string.Format("Error During {0} Job", JobType),
            };
            var text = new StringBuilder();
            text.AppendLine(string.Format("ID: {0}", JobId));
            text.AppendLine(string.Format("Started: {0}", StartedOn.Value.ToString(DateTimeFormat.ShortDateTime)));
            if (UploadedXmlReportUrl.HasChars())
                text.AppendLine(string.Format("Log Url: {0}", UploadedXmlReportUrl));
            text.AppendLine();
            text.AppendLine("---------- Errors ----------");
            Errors.Each((error, i) =>
            {
                text.AppendLine("Error #" + i.ToInvariant());
                text.AppendLine(error.Report().Indent());
                text.AppendLine();
            });

            if (Report != null)
            {
                text.AppendLine();
                text.AppendLine("---------- Report ----------");
                text.AppendLine(Report.ToString(SaveOptions.None));
            }
            mail.TextBody = text.ToString();
            Console.WriteLine(mail.TextBody);
            OutgoingMail.SendNow(mail);
        }

        /// <summary>
        /// Represents around a step called "Step" in the task. 
        /// </summary>
        public void Step(System.Action work)
        {
            Step("Step", false, work);
        }

        /// <summary>
        /// Represents around a step in the task.  No retries are allowed.  
        /// </summary>
        public void Step(string name, System.Action work)
        {
            Step(name, false, work);
        }

        /// <summary>
        /// Represents around a step in the task.  No retries are allowed.  
        /// </summary>
        public void Step(string name, bool runIfErrorOccured, System.Action work)
        {
            Step(name, runIfErrorOccured, 1, TimeSpan.Zero, work);
        }

        /// <summary>
        /// Represents around a step in the task.  If an exception occurs during this, retries are allowed.  This method shouldn't throw an exception.  
        /// </summary>
        public void Step(string name, bool runIfErrorOccured, int maxTries, TimeSpan betweenTries, System.Action work)
        {
            Step(new StepParams
            {
                Name = name,
                RunIfErrorOccured = runIfErrorOccured,
                MaxTries = maxTries,
                BetweenTries = betweenTries,
                Work = work
            });
        }
                /// <summary>
        /// Represents around a step in the task.  If an exception occurs during this, retries are allowed.  This method shouldn't throw an exception.  
        /// </summary>
        /// <param name="name"></param>
        /// <param name="work"></param>
        public bool Step(string name, bool runIfErrorOccured, int maxTries, TimeSpan betweenTries, bool addToReport, System.Action work)
        {
            return Step(new StepParams
            {
                Name = name,
                RunIfErrorOccured = runIfErrorOccured,
                MaxTries = maxTries,
                BetweenTries = betweenTries,
                AddToReport = addToReport,
                Work = work
            });
        }

        /// <summary>
        /// Represents around a step in the task.  If an exception occurs during this, retries are allowed.  This method shouldn't throw an exception.  
        /// </summary>
        /// <param name="name"></param>
        /// <param name="work"></param>
        public bool Step(StepParams stepParams)
        {
            CurrentOutputIndent++;
            if (!EnableXmlReport && stepParams.AddToReport)
                stepParams.AddToReport = false;
            if (stepParams.MaxTries < 1)
                throw new ArgumentOutOfRangeException("maxTries");

            if (!stepParams.RunIfErrorOccured && ErrorOccured)
            {
                return false;//bypass if an error occurs
            }
            bool result = true;

            if (stepParams.WriteStartAndEndToConsole)
                Console.WriteLine("Start " + stepParams.Name);
            var watch = Stopwatch.StartNew();
            //append the element and create the document if necessary
            var previousReportElement = CurrentReportElement;
            if (stepParams.AddToReport && Report != null)
            {
                var element = new XElement("Step");
                element.SetAttributeValue("Name", stepParams.Name);                
                previousReportElement.Add(element);//append this to the report
                CurrentReportElement = element;
            }

            for (var i = 0; i < stepParams.MaxTries; i++)
            {
                try
                {
                    stepParams.Work();//if you want to be able to get the current state of the job ("DownloadVideo, running 15secs."), you'd have to make the asynchronous
                    break;
                }
                catch (Exception ex)
                {
                    var exReport = ex.Report();
                    Console.WriteLine("Exception Occured: " + exReport);

                    if (stepParams.AddToReport)
                    {
                        var msgElement = new XElement("Error");
                        msgElement.SetAttributeValue("Attempt", i.ToInvariant());
                        msgElement.SetValue(exReport);
                        CurrentReportElement.Add(msgElement);
                    }
                    if (i == stepParams.MaxTries - 1)
                    {
                        result = false;
                        OnErrorOccured(ex);

                        if (stepParams.AddToReport)
                        {
                            CurrentReportElement.SetAttributeValue("Failed", true);
                        }
                        break;
                    }
                    else
                    {
                        if (stepParams.BetweenTries > TimeSpan.Zero)
                            Thread.Sleep(stepParams.BetweenTries);
                    }
                }
            }

            watch.Stop();
            if (stepParams.AddToReport)
            {
                CurrentReportElement.SetAttributeValue("DurationSec", watch.Elapsed.TotalSeconds.ToString(NumberFormat.Number, 3));
                CurrentReportElement = previousReportElement;
            }
            CurrentOutputIndent--;
            if (stepParams.WriteStartAndEndToConsole)
                Console.WriteLine("End   {0}.  Took {1}", stepParams.Name, FormatElapsed(watch.Elapsed));
            return result;
        }

        public static string FormatElapsed(TimeSpan time)
        {
            return time.TotalSeconds.ToString(NumberFormat.Number, 3) + " sec";
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
        /// Adds a custom element inside the current 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="attributes"></param>
        public void AddDynamicObjectToReport(string name, object obj)
        {
            //this is basic stuff and doesn't include complex children or arrrays
            var element = new XElement(name);
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                object propValue = descriptor.GetValue(obj);
                if (propValue != null)
                {
                    element.SetAttributeValue(XmlConvert.EncodeName(descriptor.Name), propValue);
                }
            }
            WriteLine(element.ToString());
            if (EnableXmlReport)
            {
                CurrentReportElement.Add(element);
            }
        }



        /// <summary>
        /// Add an element to the current report element that contains the message passed.
        /// </summary>
        public void AddReportMessage(string message)
        {
            if (EnableXmlReport)
            {
                var msgElement = new XElement("Message");
                msgElement.SetValue(message);
                CurrentReportElement.Add(msgElement);
            }
        }

        /// <summary>
        /// Writes to the console output only.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public void WriteLine(string value)
        {
            WriteLine(value, false);
        }

        public void WriteLine(string value, bool addToReport)
        {
            Console.WriteLine(value.Indent(CurrentOutputIndent * 2));
            if (addToReport)
            {
                AddReportMessage(value);
            }
        }

        public void WriteLine(string value, bool addToReport, params object[] arg)
        {
            Console.WriteLine(string.Format(value, arg).Indent(CurrentOutputIndent * 2));
            if (addToReport)
            {
                AddReportMessage(string.Format(value, arg));
            }
        }


        #endregion


    }

}
