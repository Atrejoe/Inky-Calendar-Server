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
		public const string DemoImageUrl = "https://cdn.displate.com/artwork/2026-03-07/47160f6f-1621-49ae-9674-29bcb4a08ac4.jpg";

		/// <summary>
		/// Creates the demo panel, uses <see cref="DemoImageUrl"/>
		/// </summary>
		public TestImagePanelRenderer() : base(new Uri(DemoImageUrl))
		{
		}
	}
}
