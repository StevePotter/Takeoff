using System;
using System.Linq;
using CommandLine;
using Takeoff.Data;
using Takeoff.Models;
using System.Data.Objects;
using System.Text;
using CsvHelper;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Amazon;

namespace Takeoff.DataTools.Commands
{
    /*
     *TEMPORARY COMMAND USED DURING OUR CLOSING TO GET LIST OF EMAIL APOLOGIES
     *
     */

    public class ExportAccountData : BaseCommand
    {
        private string ExportRootPath = @"C:\Users\Steve\Dropbox (Personal)\Companies\Takeoff\Dev\Closing Down\Owner Exports With Videos";

        public ExportAccountData()
        {
            EnableXmlReport = false;
            NotifyOnErrors = true;
            LogJobInDatabase = false;
        }

        class CommentRecord
        {
            public bool IsReply { get; set; }
            public int ReplyToCommentId { get; set; }
            public int CommentId { get; set; }
            public string CommentText { get; set; }
            public DateTime CommentCreatedOn { get; set; }
            public string CommenterName { get; set; }
            public string CommenterEmail { get; set; }
            public double? CommentTimeInSeconds { get; set; }
            public string VideoName { get; set; }
            public int VideoId { get; set; }
            public string VideoUrl { get; set; }
            public string OriginalVideoFileName { get; set; }
            public string ProjectName { get; set; }
            public int ProjectId { get; set; }

        }


        protected override void Perform(string[] commandLineArgs)
        {
            try
            {
                var s3 = new Amazon.S3.AmazonS3Client("AKIAJYKCKJUIQSZWAF7A", "draJjhtxNs2nXaq+n7khfiGndH+8VWRZx30zsz9G", RegionEndpoint.USEast1);
                var db = DataModel.ReadOnly;

                var accountsQuery = (from at in db.Things
                                     join a in db.Accounts on at.Id equals a.ThingId
                                     join ut in db.Things on at.CreatedByUserId equals ut.Id//todo: when you start tracking ownership, use that column instead.  also note this could create an orphaned record if the owner doesn't exist
                                     join u in db.Users on ut.Id equals u.ThingId
                                     where at.DeletedOn == null && at.Type == Things.ThingType(typeof(AccountThing)) && (a.Status == AccountStatus.Subscribed.ToString() || a.Status == AccountStatus.FreePlan.ToString() || a.Status == AccountStatus.Pastdue.ToString() || a.Status == AccountStatus.Trial.ToString())
                                     select new
                                     {
                                         AccountId = at.Id,
                                         AccountOwnerId = u.ThingId,
                                         AccountOwnerEmail = u.Email,
                                         AccountOwnerName = u.Name,
                                         AccountOwnerFirstName = u.FirstName,
                                         AccountOwnerLastName = u.LastName,
                                         AccountCreatedOn = at.CreatedOn,
                                     }).OrderByDescending(a => a.AccountCreatedOn);
                var accountsSql = accountsQuery.ToTraceString();//copy/paste into sql manager
                var accounts = accountsQuery.ToArray();
                var accountNumber = 0;
                Console.WriteLine($"Got {accounts.Length} active non trial accounts.");
                var accountsByID = accounts.ToDictionary(a => a.AccountId);
                var commentRecordsPerAccount = new Dictionary<int, CommentRecord[]>();
                foreach (var account in accounts)
                {
                    accountNumber++;
                    Console.WriteLine($"Processing account {accountNumber} of {accounts.Length}");
                    var accountProcessedFile = Path.Combine(ExportRootPath, $".{account.AccountId}");
                    if (File.Exists(accountProcessedFile))
                    {
                        Console.WriteLine($"Account {account.AccountId} was already processed.");
                        continue;
                    }
                    List<CommentRecord> recordsToExport = new List<CommentRecord>();
                    var projectIdsQuery = (from t in db.Things
                                           where t.DeletedOn == null && t.Type == Things.ThingType(typeof(ProjectThing)) && t.AccountId == account.AccountId
                                           select t.Id);
                    var projectIdsSql = projectIdsQuery.ToTraceString();
                    var projectIds = projectIdsQuery.ToArray();
                    Console.WriteLine($"Got {projectIds.Length} projects for account {account.AccountId}, owner {account.AccountOwnerEmail}");
               //     ExportRootPath = @"C:\Users\Steve\Dropbox (Personal)\Companies\Takeoff\Dev\Closing Down\Owner Exports With Videos";
                //    ExportRootPath = @"F:\Owner Exports With Videos";
                    var saveDirectory = Path.Combine(ExportRootPath, account.AccountId.ToInvariant());
                    if (!Directory.Exists(saveDirectory))
                    {
                        Console.Write($"Creating directory {saveDirectory}");
                        Directory.CreateDirectory(saveDirectory);
                    }

                    foreach (var projectId in projectIds)
                    {
                        var foundOne = false;
                        var project = Things.Get<ProjectThing>(projectId);
                        var projectView = (ProjectThingView)project.CreateViewData(null);

                        int i = 0;
                        foreach (var video in projectView.Videos)
                        {
                            i++;
                            Console.WriteLine($"Processing video {video.Id} for project {projectId} account {account.AccountId}.  {i} of {projectView.Videos.Length} videos");
                            var videoThing = (VideoThing)project.FindDescendantById(video.Id);
                            var videoView = (VideoThingDetailView)videoThing.CreateViewData(null);
                            var videoUrl = string.Empty;
                            var originalVideoFileName = string.Empty;
                            var stream = videoThing.ChildrenOfType<VideoStreamThing>().FirstOrDefault(s => s.Profile == "Web");
                            if (stream != null)
                            {
                                var location = new S3FileLocation { Location = stream.Location, FileName = stream.FileName };
                                var downloadFileName = SafeFileName($"Video - {project.Title.CharsOrEmpty()} - {videoThing.Title.CharsOrEmpty()} - {videoThing.Id}{Path.GetExtension(stream.FileName)}");
                                videoUrl = s3.GetPreSignedURL(new Amazon.S3.Model.GetPreSignedUrlRequest
                                {
                                    BucketName = location.Bucket,
                                    Key = location.Key,
                                    Protocol = Amazon.S3.Protocol.HTTPS,
                                    Verb = Amazon.S3.HttpVerb.GET,
                                    Expires = DateTime.Now.AddMonths(6),
                                    ResponseHeaderOverrides = new Amazon.S3.Model.ResponseHeaderOverrides { ContentDisposition = "attachment; filename=" + downloadFileName }
                                });

//                                videoUrl = SafeFileName($"Video - {project.Title.CharsOrEmpty()} - {videoThing.Title.CharsOrEmpty()} - {videoThing.Id}{Path.GetExtension(stream.FileName)}");
                                //var videoPath = Path.Combine(saveDirectory, videoUrl);
                                //if (File.Exists(videoPath))
                                //{
                                //    Console.WriteLine($"Video {videoUrl} already existed.  Not downloading.");
                                //}
                                //else
                                //{
                                //    using ()
                                //    {
                                //        Console.WriteLine($"Downloading video stream for account {account.AccountOwnerEmail} from {location.Key} to {videoPath}");
                                //        var response = s3.GetObject("to-projects", location.Key);
                                //        response.WriteResponseStreamToFile(videoPath);
                                //        response.Dispose();
                                //    }
                                //}
                            }

                            var source = videoThing.ChildrenOfType<FileThing>().FirstOrDefault();
                            if (source != null)
                            {
                                var location = new S3FileLocation { Location = source.Location, FileName = source.FileName };
                                originalVideoFileName = location.FileName;
                                //using (var s3 = new Amazon.S3.AmazonS3Client("AKIAJYKCKJUIQSZWAF7A", "draJjhtxNs2nXaq+n7khfiGndH+8VWRZx30zsz9G", RegionEndpoint.USEast1))
                                //{
                                //    var response = s3.GetObject("to-projects", location.Key);
                                //    var fileSize = new FileSize(response.ContentLength);
                                //    Console.WriteLine($"Original video is {Math.Ceiling(fileSize.MegaBytes)} MB");
                                //    var keys = response.Metadata.Keys.ToArray();
                                //    //                                    response.WriteResponseStreamToFile(Path.Combine(saveDirectory, filename));
                                //    //                                    response.Dispose();
                                //}

                            }
                            Console.WriteLine($"Adding {videoView.Comments.Length} comments and their replies.");
                            foreach (var comment in videoView.Comments)
                            {
                                foundOne = true;
                                recordsToExport.Add(CreateRecord(projectView, videoView, comment, null, videoUrl, originalVideoFileName));
                                foreach (var reply in comment.Replies)
                                {
                                    recordsToExport.Add(CreateRecord(projectView, videoView, reply, comment.Id, videoUrl, originalVideoFileName));
                                }
                            }
                        }
                        if (foundOne)
                        {
                            var fileName = SafeFileName($"Project {project.Id} - {project.Title}.csv");
                            Save(Path.Combine(saveDirectory, fileName), recordsToExport.Where(r => r.ProjectId == projectId).ToArray(), true);
                        }
                    }
                    Save(Path.Combine(saveDirectory, $"All Comments.csv"), recordsToExport.ToArray(), true);
                    commentRecordsPerAccount[account.AccountId] = recordsToExport.ToArray();
                    File.WriteAllText(accountProcessedFile,DateTime.Now.ToLongDateString());
                    Console.WriteLine($"Added marker file {accountProcessedFile} for account to indicate it's done.");
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        static string SafeFileName(string file)
        {
            Array.ForEach(Path.GetInvalidFileNameChars(),
                  c => file = file.Replace(c.ToString(), String.Empty));
            return file;
        }

        void Save(string path, CommentRecord[] records, bool includeProjectName)
        {
            StringBuilder text = new StringBuilder();
            //var csv = new CsvWriter(new StringWriter(text));
            using (var fileWriter = new StringWriter(text))
            {
                using (var csv = new CsvWriter(fileWriter))
                {
                    if (includeProjectName)
                    {
                        csv.WriteField("Project Id");
                        csv.WriteField("Project Name");

                    }
                    csv.WriteField("Video Id");
                    csv.WriteField("Video Name");
                    csv.WriteField("Video Link");
                    csv.WriteField("Comment Id");
                    csv.WriteField("Commenter Email");
                    csv.WriteField("Commenter Name");
                    csv.WriteField("Comment Time In Video (seconds)");
                    csv.WriteField("Is Reply");
                    csv.WriteField("Reply To Comment Id");
                    csv.WriteField("Comment");
                    csv.NextRecord();
                    foreach (var comment in records)
                    {
                        if (includeProjectName)
                        {
                            csv.WriteField(comment.ProjectId);
                            csv.WriteField(comment.ProjectName);
                        }
                        csv.WriteField(comment.VideoId);
                        csv.WriteField(comment.VideoName);
                        csv.WriteField(comment.VideoUrl);
                        csv.WriteField(comment.CommentId);
                        csv.WriteField(comment.CommenterEmail ?? string.Empty);
                        csv.WriteField(comment.CommenterName ?? "(Anonymous)");
                        if (comment.CommentTimeInSeconds.HasValue)
                            csv.WriteField(comment.CommentTimeInSeconds);
                        else
                            csv.WriteField(string.Empty);
                        if (comment.IsReply)
                        {
                            csv.WriteField(comment.IsReply);
                            csv.WriteField(comment.ReplyToCommentId);
                        }
                        else
                        {
                            csv.WriteField(string.Empty);
                            csv.WriteField(string.Empty);
                        }
                        csv.WriteField(comment.CommentText);
                        csv.NextRecord();
                    }
                }
            }
            File.WriteAllText(path, text.ToString(), Encoding.UTF8);
        }

        CommentRecord CreateRecord(ProjectThingView project, VideoThingDetailView video, CommentThingView comment, int? replyTo, string videoUrl, string originalFileName)
        {
            var commentRecord = new CommentRecord
            {
                ProjectName = project.Title,
                ProjectId = project.Id,
                VideoName = video.Title,
                VideoId = video.Id,
                CommenterEmail = comment.Creator?.Email,
                CommenterName = comment.Creator?.Name,
                CommentCreatedOn = comment.CreatedOn.FromForJavascript(),
                CommentId = comment.Id,
                CommentText = comment.Body,
                VideoUrl = videoUrl.CharsOrEmpty(),
                OriginalVideoFileName =  originalFileName,
            };
            comment.IfType<VideoCommentThingView>((c) =>
            {
                commentRecord.CommentTimeInSeconds = c.StartTime;
            });
            if (replyTo.HasValue)
            {
                commentRecord.ReplyToCommentId = replyTo.Value;
                commentRecord.IsReply = true;
            }

            return commentRecord;
        }

    }
}

