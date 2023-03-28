
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Diagnostics;
//using System.Drawing;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading;
//using System.Web.Script.Serialization;
//using System.Xml.Linq;
//using Amazon.S3.Model;
//using Amazon.SQS.Model;
//using LitS3;
//using Mediascend;
//using Takeoff.Jobs;
//namespace Takeoff.Transcoder
//{

//    /// <summary>
//    /// Creates and completes the process of transcoding a single source video into 1-x target encoding profiles.
//    /// </summary>
//    public class DiyTranscodeTask: JobTask<string>
//    {
//        public DiyTranscodeTask(Message message)
//        {
//            Message = message;
//            EnableXmlReport = true;
//        }

//        #region Properties

//        static TranscodeTargetParameters FFmpegDefaultParams = new TranscodeTargetParameters
//        {
//            Format = "mp4",
//            VideoCodec = "libx264",
//            VideoBitRate = 500,
//            AudioCodec = "aac",
//            AudioBitRate = 56,
//            Preset = "libx264-normal",
//            FileExtension = "mp4",
//            FrameRate = 25//default to FFMpeg default, which is currently 25.  FFMpeg will not default to source frame rate - see "-r" in http://ffmpeg.org/ffmpeg-doc.html
//        };


//        /// <summary>
//        /// All the info regarding the source video and target outputs.  Taken from the Message.
//        /// </summary>
//        public TranscodeJobParameters Parameters { get; private set; }

//        /// <summary>
//        /// Contains the result information sent back to SQS that is picked up by the server.
//        /// </summary>
//        public TranscodeJobResult JobResult { get; private set; }

//        /// <summary>
//        /// The message taken from Amazon SQS.
//        /// </summary>
//        private Message Message { get; set; }


//        /// <summary>
//        /// The metadata for the source video.
//        /// </summary>
//        private VideoMetaData SourceMetaData { get; set; }

//        /// <summary>
//        /// The file path to the source video being transcoded.
//        /// </summary>
//        private string SourceVideoPath
//        {
//            get
//            {
//                if (Parameters == null)
//                    return null;

//                return Path.Combine(TempFolder, Parameters.SourceFileName);//the / nonsense is when a file is in a subdirectory
//            }
//        }

//        /// <summary>
//        /// The folder used to save sources and targets to while the transcoding is occuring.
//        /// </summary>
//        string TempFolder;

//        /// <summary>
//        /// Amazon S3 service interface.
//        /// </summary>
//        /// <returns></returns>
//        static S3Service S3Service
//        {
//            get
//            {
//                return new S3Service
//                {
//                    AccessKeyID = ConfigurationManager.AppSettings["AmazonAccessKey"],
//                    SecretAccessKey = ConfigurationManager.AppSettings["AmazonSecretKey"]
//                };
//            }
//        }

//        #endregion

//        #region Methods

//        /// <summary>
//        /// The primary function that kicks off the job.
//        /// </summary>
//        public void Execute()
//        {
//            Step("Job", () =>
//            {
//                Debug.Assert(Message != null);
//                var totalTime = Stopwatch.StartNew();
//                var jobElement = CurrentReportElement;
//                jobElement.SetElementValue("MessageBody", Message.Body);
//                jobElement.SetAttributeValue("MessageId", Message.MessageId);
//                jobElement.SetAttributeValue("MachineId", Program.MachineId);
//                jobElement.SetAttributeValue("StartTime", DateTime.Now.ToFormattedString(DateTimeValueFormat.ShortDateTime));

//                JobResult = new TranscodeJobResult();
//                Step("ParseMessage", () =>
//                {
//                    Parameters = new JavaScriptSerializer().Deserialize<TranscodeJobParameters>(Message.Body);
//                    jobElement.SetAttributeValue("JobId", Parameters.JobId);

//                    JobResult.JobId = Parameters.JobId;
//                    JobResult.CallbackActionUrl = Parameters.CallbackActionUrl;
//                    JobResult.Targets = Parameters.Targets.Select(o => new TranscodeTargetResult { AudioBitRate = o.AudioBitRate, VideoBitRate = o.VideoBitRate }).ToArray();
//                });

//                Step("CreateTempFolder", () =>
//                {
//                    TempFolder = Path.Combine(Path.Combine(Program.Directory, "TranscodeFiles"), Parameters.JobId);
//                    if (!Directory.Exists(TempFolder))
//                    {
//                        Directory.CreateDirectory(TempFolder);
//                    }
//                });

//                Step("DownloadSource", false, 2, TimeSpan.FromSeconds(5), DownloadSourceVideo);
//                Step("GetMetaData", GetMetaData);

//                Step("CreateThumbnails", CreateThumbnails);
//                Step("TranscodeTargets", TranscodeTargets);
//                Step("UploadTranscodedVideos", false, 2, TimeSpan.FromSeconds(5), UploadTranscodedVideos);
//                Step("UploadThumbnails", false, 2, TimeSpan.FromSeconds(5), UploadThumbnails);

//                Step("DeleteTempFiles", true, 1, TimeSpan.Zero, DeleteTempFiles);

//                totalTime.Stop();
//                JobResult.EncodingCompleted = DateTime.UtcNow;
//                jobElement.SetAttributeValue("EndTime", DateTime.Now.ToFormattedString(DateTimeValueFormat.ShortDateTime));

//                Result = new JavaScriptSerializer().Serialize(JobResult);
//            });

//            Step("UploadReport", true, 3, TimeSpan.FromSeconds(4), false, () =>
//            {
//                if (JobResult != null)
//                    Report.Root.SetAttributeValue("ErrorCode", JobResult.ErrorCode);
//                S3Service.AddObjectString(Report.ToString(SaveOptions.None), ConfigurationManager.AppSettings["ReportBucket"], Parameters.JobId + ".xml", "application/xml", LitS3.CannedAcl.Private);
//            });
//        }

//        /// <summary>
//        /// Downloads the source video file.
//        /// </summary>
//        /// <returns></returns>
//        /// <remarks>If this fails, it is often because the video was deleted by the user just after uploading.</remarks>
//        private void DownloadSourceVideo()
//        {

//            var location = new S3FileLocation { Location = Parameters.SourceLocation, FileName = Parameters.SourceFileName };
//            AddReportAttribute("Location", Parameters.SourceLocation);
//            AddReportAttribute("FileName", Parameters.SourceFileName);
//            AddReportAttribute("Bucket", location.Bucket);
//            AddReportAttribute("Key", location.Key);

//            string url;
//            //at first I used the lits3 library but it wouldn't throw an exception when the connection broke.  amazon's s3client didn't report percentage.  so i went this way and jsut download from a url.  it works fine and uses the .net native classes so i get nice exceptiopsn and progres reporting
//            using (var client = new Amazon.S3.AmazonS3Client(ConfigurationManager.AppSettings["AmazonAccessKey"], ConfigurationManager.AppSettings["AmazonSecretKey"]))
//            {
//                url = client.GetPreSignedURL(new GetPreSignedUrlRequest { BucketName = location.Bucket, Key = location.Key, Protocol = Protocol.HTTP, Verb = HttpVerb.GET, Expires = DateTime.Now.AddHours(1).ToUniversalTime() });
//            }
//            var webClient = new WebClient();

//            var asyncDownload = false;
//            if (asyncDownload)
//            {
//                var downloadPercent = 0;
//                var done = false;
//                webClient.DownloadProgressChanged += (o, e) =>
//                {
//                    if (downloadPercent != e.ProgressPercentage && e.ProgressPercentage % 10 == 0)//so there aren't 100 lines for just downloading
//                    {
//                        WriteLine("Download is " + e.ProgressPercentage.ToString(NumberFormatInfo.InvariantInfo) + "% complete.");
//                        downloadPercent = e.ProgressPercentage;
//                    }
//                    if (downloadPercent == 100)
//                        done = true;
//                };
//                webClient.DownloadFileCompleted += (o, e) =>
//                {
//                    done = true;
//                };
//                webClient.DownloadFileAsync(new Uri(url), SourceVideoPath);
//                while (!done)
//                {
//                }
//            }
//            else
//            {
//                webClient.DownloadFile(new Uri(url), SourceVideoPath);
//            }

//            if (!File.Exists(SourceVideoPath))
//                throw new Exception("Source video couldn't be downloaded.");

//            AddReportAttribute("LocalPath", SourceVideoPath);
//            AddReportAttribute("FileSize", FileUtil.FormatFileSize((int)(new System.IO.FileInfo(SourceVideoPath).Length)));
//            AddReportAttribute("FileSizeBytes", new System.IO.FileInfo(SourceVideoPath).Length);
//        }

//        /// <summary>
//        /// Gets the source video metadata, setitng the MetaData property.
//        /// </summary>
//        private void GetMetaData()
//        {
//            this.SourceMetaData = VideoMetaData.Create(Program.Directory, SourceVideoPath);
//            if (SourceMetaData == null)
//                throw new Exception("Could not extract metadata from source video.");

//            CurrentReportElement.Add(new XElement(SourceMetaData.RawMetaData.Root));

//            CurrentReportElement.SetAttributeValue("IsVideo", SourceMetaData.HasVideo);
//            CurrentReportElement.SetAttributeValue("Duration", SourceMetaData.Duration.GetValueOrDefault().TotalSeconds);
//            CurrentReportElement.SetAttributeValue("VideoBitRate", SourceMetaData.VideoBitRate);
//            CurrentReportElement.SetAttributeValue("AudioBitRate", SourceMetaData.AudioBitRate);
//            CurrentReportElement.SetAttributeValue("FrameRate", SourceMetaData.FrameRate);
//            CurrentReportElement.SetAttributeValue("Height", SourceMetaData.Height);
//            CurrentReportElement.SetAttributeValue("Width", SourceMetaData.Width);

//            if (!SourceMetaData.HasVideo)//will happen for mp3 uploads, txt, or whatever
//            {
//                JobResult.ErrorCode = TranscodeJobErrorCodes.NotAVideo;
//                return;
//            }

//            JobResult.Source = new TranscodeSourceVideoMetadata
//            {
//                Duration = !SourceMetaData.Duration.HasValue ? 0.0 : SourceMetaData.Duration.Value.TotalSeconds,
//                AudioBitRate = SourceMetaData.AudioBitRate,
//                VideoBitRate = SourceMetaData.VideoBitRate,
//                FrameRate = SourceMetaData.FrameRate,
//                Height = SourceMetaData.Height,
//                Width = SourceMetaData.Width,
//            };
//        }

//        /// <summary>
//        /// Creates the transcoded video(s).  Returns a value indicating whether all the targets were transcoded.
//        /// </summary>
//        /// <param name="tempFolder"></param>
//        /// <param name="sourceVideoPath"></param>
//        /// <returns></returns>
//        private void TranscodeTargets()
//        {
//            for (var i = 0; i < Parameters.Targets.Length; i++)
//            {
//                Step("TranscodeTarget" + (i + 1).ToInvariant(), () =>
//                {
//                    var targetParams = Parameters.Targets[i];
//                    var targetResult = JobResult.Targets[i];
//                    Debug.Assert(targetParams.AudioBitRate == targetResult.AudioBitRate && targetParams.VideoBitRate == targetResult.VideoBitRate);

//                    AddReportAttribute("TargetVideoBitRate", targetParams.VideoBitRate);
//                    AddReportAttribute("TargetAudioBitRate", targetParams.AudioBitRate);
//                    AddReportAttribute("HasAudio", SourceMetaData.HasAudio);
//                    if (SourceMetaData.HasAudio)
//                        AddReportAttribute("SkipAudio", SourceMetaData.CanSkipAudio(targetParams));
//                    AddReportAttribute("SkipVideo", SourceMetaData.CanSkipVideo(targetParams));

//                    if (SourceMetaData.CanSkipTranscode(targetParams))
//                    {
//                        targetResult.TranscodingSkipped = true;
//                        AddReportAttribute("TranscodingSkipped", true);
//                    }
//                    else
//                    {
//                        targetResult.FileName = Guid.NewGuid().StripDashes() + (targetParams.FileExtension ?? "mp4").StartingWith(".");
//                        TranscodeVideo(targetParams, targetResult);
//                        var sourceBytes = (int)(new System.IO.FileInfo(SourceVideoPath).Length);
//                        var targetBytes = (int)(new System.IO.FileInfo(GetTargetPath(targetResult)).Length);

//                        AddReportAttribute("TranscodedFileSize", FileUtil.FormatFileSize(targetBytes));
//                        if (SourceMetaData.SupportedByFlash && targetParams.SkipIfResultIsBiggerThanSource && sourceBytes <= targetBytes)
//                        {
//                            targetResult.TranscodingSkipped = true;
//                            AddReportAttribute("TranscodingSkipped", true);
//                            AddReportMessage("Transcoding completed but target was bigger than source so we skip the rest.");
//                        }
//                        else
//                        {
//                            targetResult.FileSize = (int)targetBytes;
//                        }
//                    }
//                });
//            }
//        }

//        #region Actual Transcoding

//        /// <summary>
//        /// Transcodes a single video file using the specs provided.
//        /// </summary>
//        /// <param name="job"></param>
//        private void TranscodeVideo(TranscodeTargetParameters targetParams, TranscodeTargetResult targetResult)
//        {
//            var outputPath = GetTargetPath(targetResult);
//            var defaults = FFmpegDefaultParams;
//            var ffmpegPath = ConfigurationManager.AppSettings["ffmpegPath"];
//            Debug.Assert(outputPath != null);
//            var audioCodec = StringUtil.FirstWithChars(targetParams.AudioCodec, defaults.AudioCodec);

//            Step("TranscodeVideo", () =>
//            {
//                StringBuilder opts = new StringBuilder();

//                AddCommandArg(opts, "i", SourceVideoPath.InQuotes());//input file
//                if (StringUtil.HasChars(targetParams.Preset))//you MUST put ffmpeg preset arg first.  or else the args it declares will overwrite any previous ones.  it's ridiculous (i know, and it took me hours to figure it out), but it's the way it is
//                {
//                    var presetDirectory = ConfigurationManager.AppSettings["ffmpegPresetDirectory"];
//                    AddCommandArgIf(opts, "fpre", Path.Combine(presetDirectory, targetParams.Preset.EndingWith(".ffpreset")).InQuotes(), StringUtil.HasChars(targetParams.Preset));
//                }
//                AddCommandArg(opts, "y", null);//overwrite output files.  shouldn't happen but if it does, don't halt the transcoding waiting for user input
//                AddCommandArg(opts, "threads", "0");//0 means use the optimal number of threads
//                AddCommandArg(opts, "f", StringUtil.FirstWithChars(targetParams.Format, defaults.Format));//ensure we format as an mp4

//                //-- Video
//                if (SourceMetaData.CanSkipVideo(targetParams))
//                {
//                    AddCommandArg(opts, "vcodec", "copy");//bitrate will be copied as well so it doesn't need to be included
//                }
//                else
//                {
//                    AddCommandArg(opts, "vcodec", StringUtil.FirstWithChars(targetParams.VideoCodec, defaults.VideoCodec));
//                    AddCommandArg(opts, "b", NumberUtil.FirstPositive(targetParams.VideoBitRate, defaults.VideoBitRate).ToInvariant() + "k");//video bit rate

//                    var frameRate = GetFrameRate(targetParams);
//                    AddCommandArg(opts, "r", NumberUtil.FirstPositive(frameRate, defaults.FrameRate).ToInvariant());

//                    if (targetParams.KeyFramesPerSecond.IsPositive()) //key frames
//                    {
//                        var gopSize = (int)Math.Round(frameRate / (double)targetParams.KeyFramesPerSecond);
//                        if (gopSize <= 1)//when -g is 1, it doesn't work, and only 1 keyframe in the way beginnign of the video is inserted.  couldn't find why in docs, but i prevent that here.  
//                            gopSize = 2;
//                        AddCommandArg(opts, "g", gopSize.ToInvariant());
//                    }
//                }
//                AddCommandArgIf(opts, "s", targetParams.Width.ToInvariant() + "x" + targetParams.Height.ToInvariant(), targetParams.Width.IsPositive() && targetParams.Height.IsPositive());//frame size

//                //-- Audio
//                if (!SourceMetaData.HasAudio)
//                {
//                    AddCommandArg(opts, "an", null);
//                }
//                else if (SourceMetaData.CanSkipAudio(targetParams))
//                {
//                    AddCommandArg(opts, "acodec", "copy");//bitrate will be copied as well so it doesn't need to be included
//                }
//                else if (audioCodec == "aac")
//                {
//                    AddCommandArg(opts, "an", null);//ignore the audio because we add it down below                
//                }
//                else
//                {
//                    AddCommandArg(opts, "acodec", audioCodec);
//                    AddCommandArg(opts, "ab", NumberUtil.FirstPositive(targetParams.AudioBitRate, defaults.AudioBitRate).ToInvariant() + "k");//audio bit rate
//                }

//                if (StringUtil.HasChars(targetParams.TranscoderArgs))//any additional args go at the end to overwrite any previous ones
//                    opts.Append(targetParams.TranscoderArgs.Surround(" "));

//                opts.Append(outputPath.InQuotes().Surround(" "));//target file
//                AddReportAttribute("Args", opts.ToString());

//                //run the process
//                using (Process process = System.Diagnostics.Process.Start(new ProcessStartInfo(ConfigurationManager.AppSettings["ffmpegPath"], opts.ToString())
//                {
//                    UseShellExecute = false,
//                    CreateNoWindow = true,
//                    RedirectStandardError = true,
//                    RedirectStandardOutput = true,
//                    WorkingDirectory = Program.Directory,

//                }))
//                {
//                    string output = process.StandardError.ReadToEnd();
//                    process.WaitForExit();
//                    process.Close();
//                    //fyi, i removed the ffmpeg output because it was so massive
//                }
//                var fileInfo = new FileInfo(outputPath);
//                if (!fileInfo.Exists || fileInfo.Length <= 0 )
//                {
//                    JobResult.ErrorCode = TranscodeJobErrorCodes.NotCompatible;
//                    throw new Exception("FFMpeg transcoding failed.");
//                }
//            });


//            //if the audio was in a separate file (due to ffmpeg shitty aac encoder), encode that bad boy!
//            if (SourceMetaData.HasAudio && !SourceMetaData.CanSkipAudio(targetParams) && audioCodec == "aac")
//            {
//                EncodeAudioSeparately(targetParams, targetResult);
//            }

//            // Video that comes out of ffmpeg requires the entire video to be downloaded before it starts playing.  This is because the metadata is at the end of the file.
//            // I couldn't find an ffmpeg option to fix that, so I use another utility called MP4Box to do it.  It does not reencode, it simply rearranges the file a bit.  In fact the file size shuldn't change.
//            // http://www.longtailvideo.com/support/forum/Setup-Problems/10143/How-to-setup-to-play-H264-files-thanks-#msg65172
//            Step("MuxMp4", () =>
//            {
//                using (var proc = new System.Diagnostics.Process())
//                {
//                    proc.StartInfo.FileName = ConfigurationManager.AppSettings["mp4BoxPath"];
//                    proc.StartInfo.Arguments = string.Format("-isma {0}", outputPath.InQuotes());
//                    proc.StartInfo.UseShellExecute = false;
//                    proc.StartInfo.WorkingDirectory = Program.Directory;
//                    proc.Start();
//                    proc.WaitForExit();
//                    proc.Close();
//                }
//            });

//        }

//        /// <summary>
//        /// ffmpeg's aac encoder sucks so we have to do a huge workaround to provide good aac audio (which is required for Flash).  This method handles that.
//        /// </summary>
//        /// <remarks>
//        /// It works by:
//        /// 1. Extracting audio into a wav file using ffmpeg
//        /// 2. Transcoding the audio into aac via nero encoder
//        /// 3. Adding the aac audio to the video file (which should already be transcoded without audio) using mp4box
//        /// </remarks>
//        private void EncodeAudioSeparately(TranscodeTargetParameters targetParams, TranscodeTargetResult targetResult)
//        {
//            var outputPath = GetTargetPath(targetResult);
//            var defaults = FFmpegDefaultParams;
//            var ffmpegPath = ConfigurationManager.AppSettings["ffmpegPath"];

//            string audioFile = outputPath + ".aac";//the path to the separate audio file to mux in.  
//            var wavAudioFile = outputPath + ".wav";
//            Step("ExtractAudio", () =>
//            {
//                var startInfo = new ProcessStartInfo(ffmpegPath,
//                    string.Format("-i {0} -threads 0 -acodec pcm_s16le -f wav {1}", SourceVideoPath.InQuotes(), wavAudioFile.InQuotes()))
//                {
//                    UseShellExecute = false,
//                    CreateNoWindow = true,
//                    RedirectStandardError = true,
//                    RedirectStandardOutput = true,
//                    WorkingDirectory = Program.Directory,
//                };
//                AddReportAttribute("Args", startInfo.Arguments);
//                //extract the audio into a wav file (highly uncompressed)
//                using (var process = Process.Start(startInfo))
//                {
//                    string output = process.StandardError.ReadToEnd();
//                    process.WaitForExit();
//                    process.Close();
//                }
//                if (!File.Exists(wavAudioFile))
//                    throw new Exception("Couldn't extract the audio.");
//            });
//            //now take the big ass wav file and encode it to aac using nero
//            Step("TranscodeAudio", () =>
//            {
//                var bitRate = (NumberUtil.FirstPositive(targetParams.VideoBitRate, defaults.VideoBitRate) * 1000).ToInvariant();
//                var startInfo = new ProcessStartInfo(ConfigurationManager.AppSettings["neroAacEncPath"],
//                    string.Format("-br {0} -if {1} -of {2}", bitRate, wavAudioFile.InQuotes(), audioFile.InQuotes()))
//                {
//                    UseShellExecute = true,
//                    WorkingDirectory = Program.Directory,
//                };
//                AddReportAttribute("Args", startInfo.Arguments);
//                //extract the audio into a wav file (highly uncompressed)
//                using (var process = Process.Start(startInfo))
//                {
//                    process.WaitForExit();
//                    process.Close();
//                }
//                if (!File.Exists(audioFile))
//                    throw new Exception("Wav file was not encoded.");
//                File.Delete(wavAudioFile);//no longer needed
//            });

//            Step("CombineAudioVideo", () =>
//            {
//                using (var proc = new System.Diagnostics.Process())
//                {
//                    proc.StartInfo.FileName = ConfigurationManager.AppSettings["mp4BoxPath"];
//                    proc.StartInfo.Arguments = string.Format("-fps {0} -add {1} {2}", GetFrameRate(targetParams).ToInvariant(), audioFile.InQuotes(), outputPath.InQuotes());//fps is debatable, but i am including it anyway http://forum.doom9.org/archive/index.php/t-106317.html
//                    proc.StartInfo.UseShellExecute = false;
//                    proc.Start();
//                    proc.WaitForExit();
//                    proc.Close();
//                }
//                File.Delete(audioFile);//clean up the temp audio file
//            });
//        }

//        private double GetFrameRate(TranscodeTargetParameters targetParams)
//        {
//            var frameRate = targetParams.FrameRate;
//            if (!frameRate.IsPositive() || (SourceMetaData.FrameRate.IsPositive() && targetParams.UseSourceFrameRateIfLower && SourceMetaData.FrameRate < targetParams.FrameRate))
//                frameRate = SourceMetaData.FrameRate;
//            return NumberUtil.FirstPositive(frameRate, FFmpegDefaultParams.FrameRate);
//        }


//        #endregion

//        #region Thumbnails

//        private void CreateThumbnails()
//        {
//            if (Parameters.Thumbnails <= 0)
//                return;
//            Size thumbnailBox = new Size(Parameters.ThumbnailMaxWidth, Parameters.ThumbnailMaxHeight);
//            var thumbPathPrefix = Guid.NewGuid().StripDashes() + "-t";
//            var ffmpegPath = ConfigurationManager.AppSettings["ffmpegPath"];

//            //space the thumbnails out in the middle of "intervals" of the video.  this way we don't have thumbnaisl on first or last frames.  for example, a 30 second video with 3 thumbnails would have them at 5, 15, 25 seconds.
//            var thumbnailInterval = Math.Round((this.SourceMetaData.Duration.Value.TotalSeconds / (double)(Parameters.Thumbnails)), 1);
//            var thumbSize = new Size(SourceMetaData.Width, SourceMetaData.Height).Resize(thumbnailBox);
//            var thumbnails = new List<TranscodedVideoThumbnail>();
//            var thumbnailTime = thumbnailInterval / 2;
//            var thumbnailIndex = 0;

//            if (thumbnailInterval == 0)
//            {
//                CreateThumbnail(thumbPathPrefix, 0, thumbnailIndex, thumbSize, thumbnails);
//            }
//            else
//            {
//                //you could create thumbnails in one shot but it doesn't allow a starting offset.  So instead of 5, 15, and 25 seconds, you would get 0,10,20,30 sec.  This way is probably slower, not sure how much though.
//                while (thumbnailTime < this.SourceMetaData.Duration.Value.TotalSeconds)
//                {
//                    CreateThumbnail(thumbPathPrefix, thumbnailTime, thumbnailIndex, thumbSize, thumbnails);
//                    thumbnailTime += thumbnailInterval;
//                    thumbnailIndex++;
//                }
//            }
//            JobResult.Thumbnails = thumbnails.ToArray();
//        }


//        private void CreateThumbnail(string thumbPathPrefix, double thumbnailTime, int thumbnailIndex, Size thumbSize, List<TranscodedVideoThumbnail> thumbnails)
//        {
//            var fileName = thumbPathPrefix + thumbnailIndex.ToInvariant() + ".jpg";
//            var outputPath = Path.Combine(TempFolder, fileName);
//            StringBuilder opts = new StringBuilder();
//            AddCommandArg(opts, "i", SourceVideoPath.InQuotes());//input file
//            AddCommandArg(opts, "y", null);//overwrite output files.  shouldn't happen but if it does, don't halt the transcoding waiting for user input
//            AddCommandArg(opts, "ss", thumbnailTime.ToInvariant());
//            AddCommandArg(opts, "f", "image2");
//            AddCommandArg(opts, "vcodec", "mjpeg");
//            AddCommandArg(opts, "vframes", "1");
//            AddCommandArg(opts, "an", null);
//            AddCommandArg(opts, "s", thumbSize.Width.ToInvariant() + "x" + thumbSize.Height.ToInvariant());
//            opts.Append((outputPath).InQuotes().Surround(" "));//target file
//            using (Process process = System.Diagnostics.Process.Start(new ProcessStartInfo(ConfigurationManager.AppSettings["ffmpegPath"], opts.ToString())
//            {
//                UseShellExecute = false,
//                CreateNoWindow = true,
//                RedirectStandardError = true,
//                RedirectStandardOutput = true,
//                WorkingDirectory = Program.Directory,
//            }))
//            {
//                string output = process.StandardError.ReadToEnd();
//                process.WaitForExit();
//                process.Close();

//                if (File.Exists(outputPath))
//                {
//                    thumbnails.Add(new TranscodedVideoThumbnail
//                    {
//                        FileName = fileName,
//                        Height = thumbSize.Height,
//                        Width = thumbSize.Width,
//                        Time = thumbnailTime,
//                        FileLocation = this.Parameters.ThumbnailLocation
//                    });
//                }
//            }
//        }


//        /// <summary>
//        /// Uploads the video thumbails.
//        /// </summary>
//        private void UploadThumbnails()
//        {
//            if (JobResult.Thumbnails == null)
//                return;
//            for (var i = 0; i < JobResult.Thumbnails.Length; i++)
//            {
//                Step("UploadThumbnail" + (i + 1).ToInvariant(), false, 2, TimeSpan.FromSeconds(5), () =>
//                {
//                    var thumbnail = JobResult.Thumbnails[i];
//                    var path = Path.Combine(TempFolder, thumbnail.FileName);
//                    Debug.Assert(File.Exists(path));
//                    var location = new S3FileLocation { Location = thumbnail.FileLocation, FileName = thumbnail.FileName };

//                    LitS3.CannedAcl acl = (LitS3.CannedAcl)Enum.Parse(typeof(LitS3.CannedAcl), Parameters.ThumbnailAcl.ToString());
//                    Debug.Assert(acl == LitS3.CannedAcl.Private || acl == LitS3.CannedAcl.PublicRead, "Not an acl I approve of.");
//                    AddReportAttribute("Location", location.Location);
//                    AddReportAttribute("FileName", location.FileName);
//                    AddReportAttribute("Bucket", location.Bucket);
//                    AddReportAttribute("Key", location.Key);

//                    if (acl == CannedAcl.PublicRead && Parameters.AddNonExpireCacheIfThumbnailAclPublic)
//                    {
//                        UploadFileToS3AsPublicCached(path, location.Bucket, location.Key, FileUtil.GetMimeType(Path.GetExtension(thumbnail.FileName)), false);
//                    }
//                    else
//                    {
//                        S3Service.AddObject(path, location.Bucket, location.Key, FileUtil.GetMimeType(Path.GetExtension(thumbnail.FileName)), acl);
//                    }
//                });
//            }

//        }

//        #endregion


//        #region S3 Util

//        public void UploadFileToS3AsPublicCached(string filePath, string bucketName, string filePathInBucket, string mimeType, bool isGZipped)
//        {
//            UploadFileToS3(new MemoryStream(File.ReadAllBytes(filePath)), bucketName, filePathInBucket, "public, max-age=31536000", CannedAcl.PublicRead, mimeType, isGZipped);//use max-age instead of expires because its a sliding expiration
//        }

//        public void UploadFileToS3(MemoryStream inputStream, string bucketName, string filePathInBucket, string cacheControl, CannedAcl acl, string mimeType, bool isGZipped)
//        {
//            var request = new AddObjectRequest(S3Service, bucketName, filePathInBucket);
//            request.CannedAcl = acl;
//            request.ContentLength = inputStream.Length;
//            if (!string.IsNullOrEmpty(cacheControl))
//                request.CacheControl = cacheControl;
//            if (isGZipped)
//                request.ContentEncoding = "gzip";
//            request.ContentType = mimeType;
//            inputStream.Position = 0;
//            request.PerformWithRequestStream(delegate(Stream stream)
//            {
//                CopyStream(inputStream, stream, inputStream.Length, null);
//                stream.Flush();
//            });
//        }


//        private static void CopyStream(Stream source, Stream dest, long length, Action<long> progressCallback)
//        {
//            byte[] buffer = new byte[0x2000];
//            if (progressCallback != null)
//            {
//                progressCallback(0L);

//            }
//            int num = 0;
//            while (num < length)
//            {
//                int count = source.Read(buffer, 0, buffer.Length);
//                if (count <= 0)
//                {
//                    throw new Exception("Unexpected end of stream while copying.");
//                }
//                dest.Write(buffer, 0, count);
//                num += count;
//                if (progressCallback != null)
//                {
//                    progressCallback((long)num);
//                }
//            }
//        }


//        #endregion

//        public override bool ErrorOccured
//        {
//            get
//            {
//                return base.ErrorOccured || (JobResult != null && JobResult.ErrorCode != TranscodeJobErrorCodes.NotSet);
//            }
//        }

//        protected override void OnErrorOccured(Exception ex)
//        {
//            base.OnErrorOccured(ex);
//            if (JobResult != null && JobResult.ErrorCode == TranscodeJobErrorCodes.NotSet)
//                JobResult.ErrorCode = TranscodeJobErrorCodes.Other;
//        }


//        /// <summary>
//        /// Uploads the transcoded videos.
//        /// </summary>
//        private void UploadTranscodedVideos()
//        {
//            for (var i = 0; i < Parameters.Targets.Length; i++)
//            {
//                Step("UploadTranscodedVideo" + (i + 1).ToInvariant(), false, 2, TimeSpan.FromSeconds(5), () =>
//                {
//                    var targetParams = Parameters.Targets[i];
//                    var targetResult = JobResult.Targets[i];

//                    if (targetResult.TranscodingSkipped)
//                    {
//                        AddReportMessage("Transcoding was skipped so there's no need to upload.");
//                    }
//                    else
//                    {
//                        string transcodedVideoPath = GetTargetPath(targetResult);


//                        //                        var client = new Amazon.S3.AmazonS3Client(ConfigurationManager.AppSettings["AmazonAccessKey"], ConfigurationManager.AppSettings["AmazonSecretKey"], new AmazonS3Config().WithCommunicationProtocol(Protocol.HTTP));
//                        //                        var response = client.PutObject(new Amazon.S3.Model.PutObjectRequest
//                        //                        {
//                        //                            CannedACL = S3CannedACL.Private,
//                        //                            BucketName = location.Bucket,
//                        //                            Key = location.Key,
//                        //                            FilePath = transcodedVideoPath,
//                        //                            ContentType = FileUtil.GetMimeType(Path.GetExtension(targetResult.FileName))
//                        //                        });

//                        //            response.

//                        //            s3Service.GetObject(location.Bucket, location.Key, SourceVideoPath);//synchronous...will not return until file is downloaded


//                        var s3Service = S3Service;

//                        var uploadProgress = 0;
//                        s3Service.AddObjectProgress += (o, e) =>
//                        {
//                            if (e.ProgressPercentage != uploadProgress && e.ProgressPercentage % 10 == 0)
//                            {
//                                uploadProgress = e.ProgressPercentage;
//                                WriteLine("Upload is " + uploadProgress.ToString(NumberFormatInfo.InvariantInfo) + "% complete.");
//                            }
//                        };
//                        var location = new S3FileLocation { Location = targetParams.TargetLocation, FileName = targetResult.FileName };
//                        var acl = (LitS3.CannedAcl)Enum.Parse(typeof(LitS3.CannedAcl), targetParams.TargetAcl.ToString());
//                        Debug.Assert(acl == LitS3.CannedAcl.Private || acl == LitS3.CannedAcl.PublicRead, "Not an acl I approve of.");

//                        AddReportAttribute("Location", location.Location);
//                        AddReportAttribute("FileName", location.FileName);
//                        AddReportAttribute("Bucket", location.Bucket);
//                        AddReportAttribute("Key", location.Key);

//                        s3Service.AddObject(transcodedVideoPath, location.Bucket, location.Key, FileUtil.GetMimeType(Path.GetExtension(targetResult.FileName)), acl);

//                        targetResult.FileLocation = location.Location;//this is here because the AddObject blocks and we want to only set this if it really worked
//                    }
//                });
//            }

//        }


//        /// <summary>
//        /// Deletes the temp files directory.
//        /// </summary>
//        private void DeleteTempFiles()
//        {
//            //exceptions could be raised if a file is in use somehow.  shouldn't happen but we don't want that to kill the job...so wrap the delete calls in empty try/catch
//            foreach (var file in Directory.GetFiles(this.TempFolder))
//            {
//                try
//                {
//                    File.Delete(file);
//                    WriteLine("Deleted " + file);
//                }
//                catch
//                {
//                }
//            }

//            try
//            {
//                Directory.Delete(TempFolder);
//                WriteLine("Deleted " + TempFolder);
//            }
//            catch
//            {
//            }
//        }


//        /// <summary>
//        /// Util function for adding a command line arg.
//        /// </summary>
//        private static void AddCommandArg(StringBuilder sb, string name, string value)
//        {
//            sb.Append("-" + name + " ");
//            if (value != null)
//                sb.Append(value + " ");
//        }

//        /// <summary>
//        /// Util function for adding a command line arg.
//        /// </summary>
//        private static void AddCommandArgIf(StringBuilder sb, string name, string value, bool condition)
//        {
//            if (condition)
//                AddCommandArg(sb, name, value);
//        }

//        /// <summary>
//        /// Gets the local file path of a transcoded target.  
//        /// </summary>
//        /// <param name="targetResult"></param>
//        /// <returns></returns>
//        private string GetTargetPath(TranscodeTargetResult targetResult)
//        {
//            if (targetResult.FileName == null)
//                return null;
//            return Path.Combine(TempFolder, targetResult.FileName);
//        }

        

//        #endregion
//    }

//}
