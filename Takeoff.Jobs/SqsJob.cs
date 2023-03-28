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
    /// Helpful job class that recieves a single message from SQS, does a task based on that message's contents, and possibly sends out a message with results.
    /// </summary>
    /// <remarks>
    /// The job works like this:
    /// 1.  Ping sqs queue for some message(s)
    /// 2.  If no message found, sleep for SleepTimeAfterNoMessageRecieved.  Go to 1
    /// 3.  Perform some task with each message.
    /// 4.  Delete the messages.  Go to 1
    /// </remarks>
    public abstract class SqsJob : Job
    {
        public SqsJob()
        {
            SleepTimeAfterNoMessageRecieved = TimeSpan.FromSeconds(5);
            SqsVisibilityTimeout = TimeSpan.FromMinutes(10);
            SleepTimeAfterFetchException = TimeSpan.FromSeconds(30);
        }

        #region Properties

        /// <summary>
        /// VisibilityTimeout for Sqs RecieveMessage.  This should be larger than the max job time ever will be.
        /// </summary>
        public TimeSpan SqsVisibilityTimeout { get; set; }

        /// <summary>
        /// The Thread.Sleep time if no message is found from SQS.
        /// </summary>
        public TimeSpan SleepTimeAfterNoMessageRecieved { get; set; }

        /// <summary>
        /// The Thread.Sleep time if an exception occurs while retrieving the incoming message.  This is often due to connectivity issues.
        /// </summary>
        public TimeSpan SleepTimeAfterFetchException { get; set; }

        /// <summary>
        /// The maximum messages that can be recieved at one time.  Default is one.
        /// </summary>
        public int MaxNumberOfMessages { get; set; }

        /// <summary>
        /// Hooks up to Amazon SQS.
        /// </summary>
        protected AmazonSQSClient SqsClient
        {
            get
            {
                if (_SqsClient == null)
                    _SqsClient = new Amazon.SQS.AmazonSQSClient(ConfigurationManager.AppSettings["AmazonAccessKey"], ConfigurationManager.AppSettings["AmazonSecretKey"]);
                return _SqsClient;
            }
        }
        AmazonSQSClient _SqsClient;

        /// <summary>
        /// The url to the queue that contains commands that are sent for this specific server.
        /// </summary>
        string IncomingQueueUrl
        {
            get
            {
                if (_IncomingQueueUrl == null)
                {
                    _IncomingQueueUrl = SqsClient.CreateQueue(new Amazon.SQS.Model.CreateQueueRequest
                    {
                        QueueName = IncomingQueueName
                    }).CreateQueueResult.QueueUrl;
                }
                return _IncomingQueueUrl;
            }
        }
        string _IncomingQueueUrl;

        /// <summary>
        /// The name of the queue that recieves incoming messages that kick off the job.
        /// </summary>
        protected abstract string IncomingQueueName
        {
            get;
        }


        /// <summary>
        /// The url to the optional queue that contains commands that are sent for this specific server.
        /// </summary>
        string OutgoingQueueUrl
        {
            get
            {
                if (_OutgoingQueueUrl == null && !string.IsNullOrEmpty(OutgoingQueueName))
                {
                    _OutgoingQueueUrl = SqsClient.CreateQueue(new Amazon.SQS.Model.CreateQueueRequest
                    {
                        QueueName = OutgoingQueueName
                    }).CreateQueueResult.QueueUrl;
                }
                return _OutgoingQueueUrl;
            }
        }
        string _OutgoingQueueUrl;

        /// <summary>
        /// The name of the queue that recieves incoming messages that kick off the job.
        /// </summary>
        protected virtual string OutgoingQueueName
        {
            get
            {
                return null;
            }
        }


        #endregion

        #region Methods

        public override object FetchTask()
        {
            var result = SqsClient.ReceiveMessage(new Amazon.SQS.Model.ReceiveMessageRequest
            {
                QueueUrl = IncomingQueueUrl,
                MaxNumberOfMessages = 1,
                VisibilityTimeout = (int)SqsVisibilityTimeout.TotalSeconds
            });//an exception here can happen, which indicates a failure with SQS system or our credentials.  It will get caught upstream.  At this point no messages are recieved so no job id so no error reporting is available
            if (result.ReceiveMessageResult.Messages.Count == 0)
            {
                return null;
            }
            else
            {
                return result.ReceiveMessageResult.Messages[0];
            }
        }

        protected override void OnFetchUnsuccessful()
        {
            Thread.Sleep(SleepTimeAfterNoMessageRecieved);
        }

        protected override void OnFetchException(Exception ex)
        {
            Thread.Sleep(SleepTimeAfterFetchException);
        }

        /// <summary>
        /// Performs the task for a given message that was recieved.  Returns a message body that will be sent to the outgoing queue.  If this returns null or the outgoing queue is null, no message will be sent.
        /// </summary>
        /// <param name="fetchResult"></param>
        /// <returns></returns>
        public abstract JobTask<String> PerformTask(Message fetchResult);

        public sealed override JobTask PerformTask(object fetchResult)
        {
            var message = (Message)fetchResult;
            var result = PerformTask(message);
            result.Step("DeleteOriginalMessage", true, () =>
                {
                    SqsClient.DeleteMessage(new DeleteMessageRequest
                    {
                        QueueUrl = IncomingQueueUrl,
                        ReceiptHandle = message.ReceiptHandle
                    });
                });

            var outgoingUrl = OutgoingQueueUrl;
            if (outgoingUrl != null && result.Result != null)
            {
                result.Step("SendResultMessage", true, () =>
                {
                    SqsClient.SendMessage(new SendMessageRequest
                    {
                        MessageBody = result.Result,
                        QueueUrl = outgoingUrl
                    });
                });
            }

            return result;
        }

        protected override void OnDisabledDuringFetch(object fetchResult)
        {
            var message = (Message)fetchResult;
            try
            {
                //this will put the message back on the queue (although it'll unfortuantely go to the end of it).
                SqsClient.SendMessage(new SendMessageRequest
                {
                    MessageBody = message.Body,
                    QueueUrl = IncomingQueueUrl
                });
                SqsClient.DeleteMessage(new DeleteMessageRequest
                {
                    ReceiptHandle = message.ReceiptHandle,
                    QueueUrl = IncomingQueueUrl
                });
            }
            catch
            {
                //too bad but it'll get put back in a while by SQS automatically
            }
            base.OnDisabledDuringFetch(fetchResult);
        }

        #endregion


    }
}
