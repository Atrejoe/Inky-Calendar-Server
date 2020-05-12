using System.Threading.Tasks;
using InkyCal.Models;
using SixLabors.ImageSharp;

namespace InkyCal.Utils
{
	/// <summary>
	/// Signature for a helper class for rendering a panel.
	/// </summary>
	public interface IPanelRenderer

	{
		/// <summary>
		/// Returns an image in portrait mode, while width and height are in landscape mode
		/// </summary>
		/// <param name="width">The height of the panel (in landscape mode).</param>
		/// <param name="height">The width of the panel (in landscape mode).</param>
		/// <param name="colors">The number of colors to render in.</param>
		/// <returns>An image</returns>
		/// <remarks>Maybe it wise to keep evrything in portrait mode</remarks>
		Task<Image> GetImage(int width, int height, Color[] colors);
	}

	/// <summary>
	/// Signature for a helper class for rendering a specific type of <see cref="Panel"/>
	/// </summary>
	public interface IPanelRenderer<TPanel> : IPanelRenderer where TPanel:Panel
	{
		/// <summary>
		/// Configures the specified panel.
		/// </summary>
		/// <param name="panel">The panel.</param>
		void Configure(TPanel panel);
	}
}
