namespace Takeoff.Data
{
    public interface IVideoStream
    {
        /// <summary>
        /// The bit rate, in KPS, of the video.
        /// </summary>
        int? VideoBitRate { get; set; }

        /// <summary>
        /// The bit rate, in KPS, of the audio.
        /// </summary>
        int? AudioBitRate { get; set; }

        /// <summary>
        /// The encoding profile that is used to select the proper stream for a given device or browser.
        /// </summary>
        /// <remarks>
        /// Allowed values:
        /// "Web"
        /// "Mobile"
        /// </remarks>
        string Profile { get; set; }
    }
}