using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;


namespace Takeoff.Platform
{
    public static class WebImageUtil
    {
        static Lazy<ImageCodecInfo> LogoPngEncoder = new Lazy<ImageCodecInfo>(() =>
        {
            return ImageCodecInfo.GetImageEncoders().Where(c => c.FormatDescription == "PNG").First();
        });
        static Lazy<ImageCodecInfo> LogoJpegEncoder = new Lazy<ImageCodecInfo>(() =>
        {
            return ImageCodecInfo.GetImageEncoders().Where(c => c.FormatDescription == "JPEG").First();
        });

        /// <summary>
        /// Loads the image from the input stream, resizes it to fit within the bounds, and saves it in a web-ready format (png or jpg).
        /// Then it uploads it to s3 as public read, non-expiring cache.  Gives it a random file name.
        /// Returns the file name and size of the image.  Returns null if the input was empty or not an image.
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="uploadLocation"></param>
        /// <returns></returns>
        public static Tuple<string, Size> ResizeAndUpload(Stream inputStream, string uploadLocation, Size maxSize, long encoderQuality)
        {
            MemoryStream outputStream = new MemoryStream();
            Bitmap image;
            try
            {
                image = new Bitmap(inputStream);
            }
            catch
            {
                return null;
            }
            ImageCodecInfo encoder = null;
            EncoderParameters encoderParams = null;
            string extension = null;
            var originalFormat = image.RawFormat;
            //.net's png encoder supports transparency but files sizes are big.  so we use png if there is transparency...otherwise use jpg

            //called on either the original (if not in web-ready format) or the resized image
            Action<Bitmap> save = (img) =>
            {
                //preserve png format and also use it if there is transparency (HasTransparency is a slow slow method by the way so we take care to run it on the resized image)
                if (originalFormat.Equals(System.Drawing.Imaging.ImageFormat.Png) || img.HasTransparency())
                {
                    extension = "png";
                    encoder = LogoPngEncoder.Value;
                    encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, encoderQuality);
                }
                else
                {
                    extension = "jpg";
                    encoder = LogoJpegEncoder.Value;
                    encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, encoderQuality);
                }
                img.Save(outputStream, encoder, encoderParams);
            };

            var resizedDimensions = image.Size.Resize(maxSize);
            if (image.Width != resizedDimensions.Width || image.Height != resizedDimensions.Height)
            {
                using (var resized = new Bitmap(image, resizedDimensions.Width, resizedDimensions.Height))
                {
                    save(resized);
                }
            }
            else
            {
                //== wasn't working.  but Equals did.  this way we don't recompress files that are in web format and sized within bounds
                if (originalFormat.Equals(System.Drawing.Imaging.ImageFormat.Png) ||
                    originalFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif) ||
                    originalFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
                {
                    if (originalFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
                        extension = "png";
                    else if (originalFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
                        extension = "gif";
                    else
                        extension = "jpg";

                    inputStream.Seek(0, SeekOrigin.Begin);
                    inputStream.CopyTo(outputStream);
                }
                else
                {
                    save(image);
                }
            }

            outputStream.Seek(0, SeekOrigin.Begin);
            var fileLocation = new S3FileLocation { Location = uploadLocation, FileName = Guid.NewGuid().StripDashes() + "." + extension };
            
            S3.UploadFileToS3AsPublicCached(outputStream, fileLocation.Bucket, fileLocation.Key, false);

            image.Dispose();
            return new Tuple<string, Size>(fileLocation.FileName, resizedDimensions);
        }


    }
}