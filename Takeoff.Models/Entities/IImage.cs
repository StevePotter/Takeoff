namespace Takeoff.Data
{
    public interface IImage
    {
        /// <summary>
        /// The width, in pixels, of the thumbnail.
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// The height, in pixels, of the thumbnail.
        /// </summary>
        int Height { get; set; }
    }
}