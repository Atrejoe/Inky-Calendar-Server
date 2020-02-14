using System;

namespace InkyCal.Utils
{
    /// <summary>
    /// A demo panel for <see cref="ImagePanel"/>
    /// </summary>
    public class ImagePanelDemo : ImagePanel
    {
        private const string demoImageUrl = "http://eskipaper.com/images/beautiful-grayscale-wallpaper-1.jpg";

        /// <summary>
        /// Creates the demo panel, uses <see cref="demoImageUrl"/>
        /// </summary>
        public ImagePanelDemo() : base(new Uri(demoImageUrl))
        {
        }
    }
}
