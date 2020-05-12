using System.Threading.Tasks;
using InkyCal.Models;
using SixLabors.ImageSharp;

namespace InkyCal.Utils
{
	/// <summary>
	/// First step for 
	/// </summary>
	/// <typeparam name="TPanel">The type of the panel.</typeparam>
	/// <seealso cref="InkyCal.Utils.IPanelRenderer" />
	public abstract class PanelRenderer<TPanel> : IPanelRenderer<TPanel> where TPanel:Panel{

		/// <summary>
		/// Initializes a new instance of the <see cref="PanelRenderer{TPanel}"/> class.
		/// </summary>
		public PanelRenderer()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PanelRenderer{TPanel}"/> class.
		/// </summary>
		/// <param name="panel">The panel to pass to <see cref="Configure(TPanel)"/>.</param>
		public PanelRenderer(TPanel panel)
		{
			Configure(panel);
		}

		/// <summary>
		/// Configures the specified panel.
		/// </summary>
		/// <param name="panel">The panel.</param>
		public void Configure(TPanel panel) {
			ReadConfig(panel);
		}
		

		/// <summary>
		/// Allow reading panel config
		/// </summary>
		/// <param name="panel">The panel.</param>
		/// <remarks>A some attributes can be manipulated and sent to <see cref="GetImage(int, int, Color[])"/>, when overriding it not recommended storing the <see cref="Panel.Width"/>, <see cref="Panel.Height"/> or other attributes derived .</remarks>
		protected abstract void ReadConfig(TPanel panel);

		/// <inheritdoc/>
		public abstract Task<Image> GetImage(int width, int height, Color[] colors);
	}
}
