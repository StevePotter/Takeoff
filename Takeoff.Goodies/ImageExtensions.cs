using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Drawing
{
    public static class ImageExtensions
    {
        #region Images


        /// <summary>
        /// Takes a size (typically the size of an image) and returns a size that will fit within the maxSize bounding box.  If it already fits, the size will be returned untouched.
        /// </summary>
        /// <param name="imageSize"></param>
        /// <param name="maxSize"></param>
        /// <param name="stretchSmallerImages"></param>
        /// <returns></returns>
        /// <remarks>This method is a bit clunky and could be done using a slicker formula but whatever...it works every time!</remarks>
        public static Size Resize(this Size imageSize, Size maxSize)
        {
            var width = imageSize.Width;
            var height = imageSize.Height;
            //if it already fits in the current box and we allow that, just return the image as it
            if (width <= maxSize.Width && height <= maxSize.Height)
            {
                //you could have another parameter like stretchSmallerImages that would stretch it to meet one dimension of the box.
                return imageSize;
            }


            if (width > maxSize.Width)
            {
                height = (height * maxSize.Width) / width;
                width = maxSize.Width;
            }
            if (height > maxSize.Height)//don't do else if because some funky size ratios will have width > maxSize.width but the resulting image size will be still too high
            {
                width = (width * maxSize.Height) / height;
                height = maxSize.Height;
            }
            return new Size(width, height);
        }

        /// <summary>
        /// Indicates whether the given image has some transparency to it.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static bool HasTransparency(this Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            int x;
            int y;
            for (x = 0; x < width; x++)
            {
                for (y = 0; y < height; y++)
                {
                    if (image.GetPixel(x, y).A != 255)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        #endregion
    }
}
