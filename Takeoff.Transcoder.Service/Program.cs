using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.ComponentModel;
using Amazon.SQS.Model;
using System.ServiceProcess;
using System.IO;
using Mediascend;
using Mediascend.Transcoder;
using Takeoff.Transcoder;
using System.Net;
using Takeoff.Jobs;
using System.Configuration;


namespace Takeoff.Transcoder
{

    class Program
    {

        #region Properties

        /// <summary>
        /// The working directory that contains the exe and other crap.
        /// </summary>
        public static string Directory { get; set; }

        /// <summary>
        /// The amazon instance ID of this machine.
        /// </summary>
        public static string MachineId
        {
            get
            {
                if (_MachineId == null)
                {
                    _MachineId = GetMachineId();
                }
                return _MachineId;
            }
        }
        private static string _MachineId;

        #endregion

        #region Methods

        static void Main(string[] args)
        {
            var argsTable = args.ArgsToTable();
            //startDelay is useful when attaching the debugger to a windows service or something
            if (argsTable.ContainsKey("startDelay"))
            {
                var seconds = argsTable["startDelay"].ToInt();
                Console.WriteLine("Starting job manager in " + seconds.ToInvariant() + " seconds.");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
            }


            var supervisor = new Supervisor();
            supervisor.Jobs.Add(new EncodingTriageJob());
            supervisor.Jobs.Add(new CommandProcessorJob(supervisor, MachineId, "Transcoder"));

            if (argsTable.ContainsKey("console"))
            {
                Directory = Environment.CurrentDirectory;
                supervisor.Start();
                Console.WriteLine("Started");
                Console.ReadLine();
            }
            else
            {
                Console.SetOut(new DebuggerWriter());
                ServiceBase.Run(new WindowsService(supervisor));
            }
        }

        /// <summary>
        /// If this is an EC2 machine, this gets the machine's instanceId.  If not, it returns the current computer's name.
        /// </summary>
        /// <returns></returns>
        private static string GetMachineId()
        {
#if DEBUG
            return Environment.MachineName;
#endif
            try
            {
                var request = WebRequest.Create("http://169.254.169.254/latest/meta-data/instance-id");
                request.Timeout = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
                var response = request.GetResponse();
                var body = new StringBuilder();
                string line;
                var webstream = new StreamReader(response.GetResponseStream());
                while ((line = webstream.ReadLine()) != null)
                {
                    body.AppendLine(line);
                }
                return body.ToString().Trim();
            }
            catch
            {
                return Environment.MachineName;
            }
        }

        #endregion


    }


}



//in csae you ever want to 
            //var sqs = new Amazon.SQS.AmazonSQSClient(ConfigurationManager.AppSettings["AmazonAccessKey"], ConfigurationManager.AppSettings["AmazonSecretKey"]);
            //sqs.DeleteQueue(new DeleteQueueRequest
            //{
            //    QueueUrl = sqs.CreateQueue(new Amazon.SQS.Model.CreateQueueRequest
            //    {
            //        QueueName = "TranscodeRequest"
            //    }).CreateQueueResult.QueueUrl
            //});
            //sqs.DeleteQueue(new DeleteQueueRequest
            //{
            //    QueueUrl = sqs.CreateQueue(new Amazon.SQS.Model.CreateQueueRequest
            //    {
            //        QueueName = "TranscodeResult"
            //    }).CreateQueueResult.QueueUrl
            //});
            //sqs.DeleteQueue(new DeleteQueueRequest
            //{
            //    QueueUrl = sqs.CreateQueue(new Amazon.SQS.Model.CreateQueueRequest
            //    {
            //        QueueName = "ZencoderRequest"
            //    }).CreateQueueResult.QueueUrl
            //});
            //sqs.DeleteQueue(new DeleteQueueRequest
            //{
            //    QueueUrl = sqs.CreateQueue(new Amazon.SQS.Model.CreateQueueRequest
            //    {
            //        QueueName = "ZencoderResult"
            //    }).CreateQueueResult.QueueUrl
            //});