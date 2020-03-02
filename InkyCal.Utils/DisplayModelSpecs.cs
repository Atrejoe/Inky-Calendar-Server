using InkyCal.Models;
using SixLabors.ImageSharp;

namespace InkyCal.Utils
{
	/// <summary>
	/// Specs for a <see cref="DisplayModel"/>
	/// </summary>
	public struct DisplayModelSpecs
	{
		/// <summary>
		/// Gets or sets the width.
		/// </summary>
		/// <value>
		/// The width.
		/// </value>
		public int Width { get; set; }
		/// <summary>
		/// Gets or sets the height.
		/// </summary>
		/// <value>
		/// The height.
		/// </value>
		public int Height { get; set; }
		/// <summary>
		/// Gets or sets the colors.
		/// </summary>
		/// <value>
		/// The colors.
		/// </value>
		public Color[] Colors { get; set; }
	}
}
