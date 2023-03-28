using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.SQS.Model;
using System.Threading;

namespace Takeoff.ServiceManager
{

    /// <summary>
    /// Sends commands to a specific server.  This is currently used for telling a server to stop or resume transcoding.  Also used for health monitoring.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var argsTable = ArgsToTable(args);

            var accessKey = argsTable["accessKey"];
            var secretKey = argsTable["secretKey"];
            var machineId = argsTable["machine"];
            var serviceName = argsTable["service"];
            var jobs = argsTable["jobs"];
            var command = argsTable["command"];
            var checkResponse = argsTable["checkResponse"].Equals("true", StringComparison.OrdinalIgnoreCase);

            var incomingQueueName = string.Format("{0}_{1}_CommandRequest", machineId, serviceName).ToLowerInvariant();
            var outgoingQueueName = string.Format("{0}_{1}_CommandResponse", machineId, serviceName).ToLowerInvariant();

            var sqsClient = new Amazon.SQS.AmazonSQSClient(accessKey, secretKey);
            var queueUrl = sqsClient.CreateQueue(new Amazon.SQS.Model.CreateQueueRequest
            {
                QueueName = incomingQueueName
            }).CreateQueueResult.QueueUrl;

            Console.WriteLine("Sending command message to server queue '" + queueUrl);
            var commandMessageId = sqsClient.SendMessage(new Amazon.SQS.Model.SendMessageRequest
            {
                MessageBody = jobs + ":" + command,
                QueueUrl = queueUrl
            }).SendMessageResult.MessageId;

            if (checkResponse)
            {
                Amazon.SQS.Model.Message message = null;

                var resultQueueUrl = sqsClient.CreateQueue(new CreateQueueRequest
                {
                    QueueName = outgoingQueueName,
                }).CreateQueueResult.QueueUrl;

                Console.WriteLine("Checking for response from " + resultQueueUrl);
                var foundIt = false;
                while (!foundIt)
                {
                    Console.WriteLine("Waiting for 2 seconds.");
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    var result = sqsClient.ReceiveMessage(new Amazon.SQS.Model.ReceiveMessageRequest
                    {
                        QueueUrl = resultQueueUrl,
                        MaxNumberOfMessages = 1,
                    });//an exception here can happen, which indicates a failure with SQS system or our credentials.  It will get caught upstream.  At this point no messages are recieved so no job id so no error reporting is available
                    
                    if (result.Messages?.Count > 0)
                    {
                        message = result.Messages[0];

                        //first line of the result message must match the command message sent.  otherwise we assume it's old and just delete/ignore
                        var lines = message.Body.SplitLines();
                        if (lines.IsIndexInBounds(0) && lines[0].Trim().Equals(commandMessageId))
                        {
                            Console.WriteLine("Recieved a response:");
                            if (lines.Length > 1)
                            {
                                lines.Skip(1).Each(l => Console.WriteLine(l));
                            }
                            foundIt = true;
                        }
                        else
                        {
                            Console.WriteLine("Ignoring a response.");
                        }

                        sqsClient.DeleteMessage(new DeleteMessageRequest
                        {
                            QueueUrl = resultQueueUrl,
                            ReceiptHandle = message.ReceiptHandle
                        });
                    }
                }
            }
        }


        #region Command Args

        /// <summary>
        /// Made for apps with a "main" method - console apps, windows services, winforms, etc.  Takes the lame string[] of parameters and turns them into a dictionary of key/value pairs.  
        /// </summary>
        /// <returns></returns>
        /// <example>
        /// 
        /// Command line: app.exe "input.txt" -a 2 -b -c "3 v"
        /// 
        /// args.ArgsToTable(new string[]{"in"}) will have these entries:
        /// "in": "input.txt"
        /// "a": "2"
        /// "b": null
        /// "c": "3 v"
        /// 
        /// </example>
        public static Dictionary<string, string> ArgsToTable(string[] args, params string[] defaultArgs)
        {
            return ArgsToTable(args, true, defaultArgs);
        }

        /// <summary>
        /// Made for apps with a "main" method - console apps, windows services, winforms, etc.  Takes the lame string[] of parameters and turns them into a dictionary of key/value pairs.  
        /// </summary>
        public static Dictionary<string, string> ArgsToTable(string[] args, bool makeLookupCaseSensitive, params string[] defaultArgs)
        {
            bool foundNamedArg = false;
            Dictionary<string, string> argTable = new Dictionary<string, string>(makeLookupCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < args.Length; i++)
            {
                var currArg = args[i];
                string value = null;
                if (currArg.StartsWith("-", StringComparison.Ordinal))
                {
                    if (!foundNamedArg)
                        foundNamedArg = true;
                    currArg = currArg.StartWithout("-", StringComparison.Ordinal);//cut off the -

                    //check for a cooresponding value
                    if (args.IsIndexInBounds(i + 1) && !args[i + 1].StartsWith("-"))
                    {
                        value = args[i + 1];
                        i++;
                    }
                }
                else
                {
                    if (foundNamedArg)
                        throw new ArgumentException("Argument '" + currArg + "' wasn't a named parameter.  Unnamed params must occur before any named ones.  Did you forget the dash before the argument name?");

                    if (!defaultArgs.IsIndexInBounds(i))
                        throw new ArgumentException("Arg #" + i.ToInvariant() + " didn't have an entry in defaultArgs");

                    value = currArg;
                    currArg = defaultArgs[i];
                }

                argTable.Add(currArg, value);
            }
            return argTable;
        }

        #endregion
    }
}
