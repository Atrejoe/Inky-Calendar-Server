// Ignore Spelling: Utils

using System;

namespace InkyCal.Utils
{
    /// <summary>
    /// A demo panel for <see cref="ImagePanelRenderer"/>
    /// </summary>
    public class TestImagePanelRenderer : ImagePanelRenderer
    {
		/// <summary>
		/// The demo image URL
		/// </summary>
		public const string DemoImageUrl = "https://eskipaper.com/images/beautiful-grayscale-wallpaper-1.jpg";

        /// <summary>
        /// Creates the demo panel, uses <see cref="DemoImageUrl"/>
        /// </summary>
        public TestImagePanelRenderer() : base(new Uri(DemoImageUrl))
        {
        }
    }
}
