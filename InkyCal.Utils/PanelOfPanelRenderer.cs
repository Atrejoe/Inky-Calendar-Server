using System;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace InkyCal.Utils
{
	/// <summary>
	/// An image panel, assumes a landscape image, resizes and flips it to portait.
	/// </summary>
	public class PanelOfPanelRenderer : IPanelRenderer
	{
		private PanelOfPanels pp;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pp"></param>
		public PanelOfPanelRenderer(PanelOfPanels pp)
		{
			this.pp = pp;
		}


		/// <inheritdoc/>
		/// <returns>A panel with nested panels</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Contains catch-all-log and do something logic")]
		public async Task<Image> GetImage(int width, int height, Color[] colors, IPanelRenderer.Log log)
		{
			colors.ExtractMeaningFullColors(
				out var primaryColor
				,out var supportColor
				,out var errorColor
				,out var backgroundColor
				);

			var result = PanelRenderHelper.CreateImage(width, height, backgroundColor);

			if (!(pp.Panels?.Any()).GetValueOrDefault())
			{
				result.Mutate(x =>
				{
					x.DrawText("No sub-panels configured", new Font(FontHelper.NotoSans, 16), errorColor, new Point(0, 0));
				});
			}
			else
			{
				var y = 0;
				var panels = pp.Panels;
				var totalPanelRatio = panels.Sum(x => x.Ratio);
				foreach (var panel in panels.OrderBy(x => x.SortIndex))
				{

					var subPanelHeight = (int)Math.Round((totalPanelRatio == 0)
											? height / panels.Count
											: height * ((float)panel.Ratio / totalPanelRatio));

					if (subPanelHeight == 0)
						continue;

					var renderer = panel.Panel.GetRenderer();

					try
					{
						var subImage = await renderer.GetImage(width, subPanelHeight, colors, log);

						result.Mutate(
							x => x.DrawImage(subImage, new Point(0, y), 1f));

						//result.Mutate(x =>
						//{
						//	x.DrawText(
						//		new TextGraphicsOptions(true) { 
						//			WrapTextWidth = width }, 
						//			panel.Panel.Name ?? string.Empty,
						//			new Font(FontHelper.NotoSans, 16),
						//			primaryColor, 
						//			new Point(0, y));
						//});
					}
					catch (Exception ex)
					{
						ex.Log();
						result.Mutate(x =>
						{
							x.DrawText(new TextGraphicsOptions(true) { WrapTextWidth = width }, ex.Message, new Font(FontHelper.NotoSans, 16), errorColor, new Point(0, y));
						});
					}

					y += subPanelHeight;
				}
			}

			return result;
		}
	}
}
