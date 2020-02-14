using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SixLabors.Primitives;
using System;
using System.Net.Http;

namespace InkyCal.Utils
{
    /// <summary>
    /// An image panel, assumes a landscape image, resizes and flips it to portait.
    /// </summary>
    public class ImagePanel : IPanel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageUrl"></param>
        public ImagePanel(Uri imageUrl)
        {
            this.imageUrl = imageUrl ?? throw new ArgumentNullException(nameof(imageUrl));
            cachedImage = new Lazy<byte[]>(GetTestImage);
        }

        private readonly Lazy<byte[]> cachedImage;

        private static readonly HttpClient client = new HttpClient();
        private readonly Uri imageUrl;

        private byte[] GetTestImage()
        {
            return client.GetByteArrayAsync(imageUrl.ToString()).Result;
        }

        /// <inheritdoc/>
        public Image GetImage(int width, int height, Color[] colors) {
            if (colors is null)
                colors = new[] { Color.White, Color.Black };
            

            var image = Image.Load(cachedImage.Value);
            image.Mutate(x => x
                .Resize(new ResizeOptions() { Mode = ResizeMode.Crop, Size = new Size(width, height) })
                .BackgroundColor(Color.White)
                .Quantize(new PaletteQuantizer(colors))
                .Rotate(RotateMode.Rotate270)
                );
            return image;
        }
    }
}
