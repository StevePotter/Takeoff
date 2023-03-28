using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Mediascend;
using System.Configuration;
using Takeoff.Transcoder;

namespace Takeoff.Transcoder
{


    /// <summary>
    /// Contains information about a video that is useful for our transcoding stuff.  Also has the ability to extract this metadata from a program called MediaInfo.
    /// </summary>
    class VideoMetaData
    {
        /// <summary>
        /// Indicates whether the file is a video file.  Otherwise it might be audio only (mp3 or wav or whatever), or maybe a text file or something.
        /// </summary>
        public bool HasVideo { get; private set; }

        /// <summary>
        /// Whether the file has an audio track.  If not, avoid audio during transcoding.
        /// </summary>
        public bool HasAudio { get; private set; }

        /// <summary>
        /// Indicates whether Flash player 9+ can play this video. 
        /// </summary>
        public bool SupportedByFlash { get; private set; }

        /// <summary>
        /// Indicates whether Flash player 9+ can play the audio stream.   Will not be set if HasAudio is false.
        /// </summary>
        public bool AudioSupportedByFlash { get; private set; }

        /// <summary>
        /// Indicates whether Flash player 9+ can play the video stream.  Will not be set if HasVideo is false.
        /// </summary>
        public bool VideoSupportedByFlash { get; private set; }

        /// <summary>
        /// The duration of the video.  Note this is not perfectly exact.  For a video over a minute it rounds to lower second.  For hours, it rounds to lower minute.
        /// </summary>
        public TimeSpan? Duration { get; private set; }

        /// <summary>
        /// The frame rate of the video.
        /// </summary>
        public double FrameRate { get; private set; }

        /// <summary>
        /// The width of the video frame.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height of the video frame.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// An xml document containing each piece of metadata taken from MediaInfo tool.
        /// </summary>
        public XDocument RawMetaData { get; private set; }

        /// <summary>
        /// The bit rate of the video, in Kbps.
        /// </summary>
        public int VideoBitRate { get; private set; }

        /// <summary>
        /// The bit rate of the audio, in Kbps.
        /// </summary>
        public int AudioBitRate { get; private set; }

        /// <summary>
        /// 'General' formats that are natively supported by Flash.
        /// </summary>
        private static string[] FlashFileFormats = new[] { "Flash Video", "MPEG-4" };

        /// <summary>
        /// Video formats that are natively supported by Flash.
        /// </summary>
        private static string[] FlashVideoFormats = new[] { "VP6", "AVC" };

        /// <summary>
        /// Audio formats that are natively supported by Flash.
        /// </summary>
        private static string[] FlashAudioFormats = new[] { "AAC" };//mp3 doesn't work, and "MPEG Audio" was never tested

        /// <summary>
        /// Reads metadata from the file and extracts the important information.
        /// </summary>
        /// <param name="videoPath"></param>
        /// <returns></returns>
        public static VideoMetaData Create(string mediaInfoBaseDirectory, string filePath)
        {
            var metaData = new VideoMetaData
            {
                RawMetaData = ExtractMetadataFromMediaInfo(mediaInfoBaseDirectory, filePath)
            };

            metaData.HasVideo = metaData.HasAttributeInGroup("video");
            metaData.HasAudio = metaData.HasAttributeInGroup("audio");

            if (!metaData.HasVideo && !metaData.HasAudio)
                return metaData;//no need to go any further

            //extract the frame rate
            if (metaData.HasVideo)
            {
                var strFrameRate = metaData.GetVideoAttribute("Frame rate");
                if (strFrameRate != null)
                {
                    strFrameRate = strFrameRate.Before("fps", StringComparison.OrdinalIgnoreCase);
                    if (strFrameRate != null)
                    {
                        strFrameRate = strFrameRate.Trim();
                        double dblFrameRate;
                        if (double.TryParse(new string(strFrameRate.ToCharArray().Where(o => Char.IsDigit(o) || o == '.').ToArray()), out dblFrameRate))
                        {
                            metaData.FrameRate = dblFrameRate;
                        }
                    }
                }

                metaData.DetermineWidthAndHeight();
            }

            //duration is a bit special.  the normal mediainfo way will chop off certain "insignificant" digits.  if a vidoe is over a minute, it chops the milliseconds.  i want accurate duration.  if you put mediainfo in "--Full" mode, it will provide the complete duration (in a format that works great iwth TimeSpan.Parse()).  but it includes many "duration" values.
            var fullRawMetaData = ExtractMetadataFromMediaInfo(mediaInfoBaseDirectory, filePath, "--Full");
            var strDuration = fullRawMetaData.Root.Elements().Where(o => o.HasAttributeWithValue("group", "general", StringComparison.OrdinalIgnoreCase) && o.GetAttributeValue("key").Equals("duration", StringComparison.OrdinalIgnoreCase) && o.GetAttributeValue("value").Contains(":")).Select(e => e.GetAttributeValue("value").Trim()).OrderByDescending(v => v.Length).FirstOrDefault();
            if (strDuration.HasChars())
            {
                TimeSpan duration;
                if (TimeSpan.TryParse(strDuration, out duration))
                {
                    metaData.Duration = duration;
                }
            }
            metaData.DetermineFlashSupport();
            metaData.DetermineBitRates();

            return metaData;
        }

        /// <summary>
        /// Runs MediaInfo on the given file with the optional args supplied
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static XDocument ExtractMetadataFromMediaInfo(string mediaInfoBaseDirectory, string filePath, string args = null)
        {
            var proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = ConfigurationManager.AppSettings["mediaInfoPath"];
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.WorkingDirectory = mediaInfoBaseDirectory;
            proc.StartInfo.Arguments = (args.HasChars() ? args.Surround(" ") : string.Empty) + filePath.Surround("\"");
            proc.Start();

            string output = proc.StandardOutput.ReadToEnd();

            var root = new XElement("metadata");
            var currGroup = string.Empty;//MediaInfo has puts titles like "General", "Video" and "Audio" on a line.  The attributes follow with names and values separated with a semicolon.
            foreach (var line in output.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!line.Contains(":"))
                {
                    currGroup = line.Trim();
                }
                else
                {
                    var key = line.Substring(0, line.IndexOf(':')).Trim();
                    var value = line.Substring(line.IndexOf(':') + 1).Trim();

                    var element = new XElement("attribute");
                    element.SetAttributeValue("group", currGroup);
                    element.SetAttributeValue("key", key);
                    element.SetAttributeValue("value", value);
                    root.Add(element);
                }
            }
            return new XDocument(root);
        }

        private void DetermineWidthAndHeight()
        {
            var width = GetVideoAttribute("width");
            if (width != null)
            {
                //values in MediaInfo include spaces and "pixels" and other crap for dimensions so we isolate them
                string strWidth = width.FilterDigits();
                if (strWidth.HasChars())
                {
                    Width = strWidth.ToInt();
                }
            }
            var height = GetVideoAttribute("height");
            if (height != null)
            {
                string strHeight = height.FilterDigits();
                if (strHeight.HasChars())
                {
                    Height = strHeight.ToInt();
                }
            }
        }


        private void DetermineBitRates()
        {
            if (HasVideo)
            {
                //            40.1 Mbps', '1 536 Kbps'
                var strVideoRate = GetVideoAttribute("Bit rate");
                double dblVideoRate;
                if (strVideoRate != null && double.TryParse(new string(strVideoRate.ToCharArray().Where(o => Char.IsDigit(o) || o == '.').ToArray()), out dblVideoRate))
                {
                    if (strVideoRate.ToLowerInvariant().Contains("mbps"))
                    {
                        VideoBitRate = (int)(dblVideoRate * 1000);
                    }
                    else if (strVideoRate.ToLowerInvariant().Contains("kbps"))
                    {
                        VideoBitRate = (int)dblVideoRate;
                    }
                }
            }
            if (HasAudio)
            {
                var strAudioRate = GetAudioAttribute("Bit rate");
                double dblAudioRate;
                if (strAudioRate != null && double.TryParse(new string(strAudioRate.ToCharArray().Where(o => Char.IsDigit(o) || o == '.').ToArray()), out dblAudioRate))
                {
                    if (strAudioRate.ToLowerInvariant().Contains("mbps"))
                    {
                        AudioBitRate = (int)(dblAudioRate * 1000);
                    }
                    else if (strAudioRate.ToLowerInvariant().Contains("kbps"))
                    {
                        AudioBitRate = (int)dblAudioRate;
                    }
                }
            }

        }

        private void DetermineFlashSupport()
        {
            if (HasAudio)
                AudioSupportedByFlash = FlashAudioFormats.Where(o => o.Equals(GetAudioAttribute("format"), StringComparison.OrdinalIgnoreCase)).Count() > 0;
            if (HasVideo)
                VideoSupportedByFlash = FlashVideoFormats.Where(o => o.Equals(GetVideoAttribute("format"), StringComparison.OrdinalIgnoreCase)).Count() > 0;

            if (AudioSupportedByFlash && VideoSupportedByFlash && FlashFileFormats.Where(o => o.Equals(GetGeneralAttribute("format"), StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                SupportedByFlash = true;
            }
        }

        public string GetGeneralAttribute(string key)
        {
            return GetAttribute("general", key);
        }

        public string GetVideoAttribute(string key)
        {
            return GetAttribute("video", key);
        }

        public string GetAudioAttribute(string key)
        {
            return GetAttribute("audio", key);
        }

        private bool HasAttributeInGroup(string group)
        {
            return RawMetaData.Root.Elements().Where(o => o.HasAttributeWithValue("group", group, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() != null;
        }

        private string GetAttribute(string group, string key)
        {
            var att = RawMetaData.Root.Elements().Where(o => o.HasAttributeWithValue("group", group, StringComparison.OrdinalIgnoreCase) && o.GetAttributeValue("key").Equals(key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return att == null ? null : att.GetAttributeValue("value");
        }


        //public bool CanSkipTranscode(TranscodeTargetParameters targetParams)
        //{
        //    return SupportedByFlash && CanSkipAudio(targetParams) && CanSkipVideo(targetParams);
        //}

        ///// <summary>
        ///// Whether the audio transcoding can be skipped.  Note this does not take HasAudio into account.
        ///// </summary>
        ///// <param name="targetParams"></param>
        ///// <returns></returns>
        //public bool CanSkipAudio(TranscodeTargetParameters targetParams)
        //{
        //    return AudioSupportedByFlash && (targetParams.MaxSkipAudioBitRate.IsPositive() && AudioBitRate.IsPositive() && AudioBitRate <= targetParams.MaxSkipAudioBitRate);
        //}

        ///// <summary>
        ///// Whether the vidoe transcoding can be skipped.  Note this does not take HasVideo into account.
        ///// </summary>
        //public bool CanSkipVideo(TranscodeTargetParameters targetParams)
        //{
        //    return VideoSupportedByFlash && (targetParams.MaxSkipVideoBitRate.IsPositive() && VideoBitRate.IsPositive() && VideoBitRate <= targetParams.MaxSkipVideoBitRate);
        //}


    }


}
