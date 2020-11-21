using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using StackExchange.Profiling;

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
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Contains catch-all-log and do something logic")]
		public async Task<Image> GetImage(int width, int height, Color[] colors, IPanelRenderer.Log log)
		{
			colors.ExtractMeaningFullColors(
				out var primaryColor
				, out var supportColor
				, out var errorColor
				, out var backgroundColor
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
				var panels = pp.Panels;
				var totalPanelRatio = panels.Sum(x => x.Ratio);


				var y = 0;
				var renderParameters = panels
										.OrderBy(x => x.SortIndex)
										.Select(panel =>
											{
											//Gather render parameters
											var subPanelHeight = (int)Math.Round((totalPanelRatio == 0)
																					? height / panels.Count
																					: height * ((float)panel.Ratio / totalPanelRatio));

															if (subPanelHeight == 0)
																return null; //Don't render

											var result = new
															{
																y,
																subPanelHeight,
																panel.Panel
															};

											//Keep track of start of next panel
											y += subPanelHeight;

												return result;
											})
										.Where(x => x != null);


				foreach (var parameter in renderParameters.AsParallel()){

					var panel = parameter.Panel;
					var renderer = panel.GetRenderer();

					try
					{
						using (MiniProfiler.Current.Step($"Render panel '{panel.Name}' ({panel.GetType().Name})")) {

							var subImage = await renderer.GetImage(width, parameter.subPanelHeight, colors, log);

							result.Mutate(
								operation => operation.DrawImage(subImage, new Point(0, parameter.y), 1f));
						}
					}
					catch (Exception ex)
					{
						ex.Data["PanelType"] = panel.GetType().Name;
						ex.Data["PanelId"] = panel.Id;

						ex.Log();

						result.Mutate(operation =>
						{
							operation.DrawText(
								options: new TextGraphicsOptions(true) { WrapTextWidth = width }, 
								text: ex.Message.ToSafeChars(FontHelper.NotoSans), 
								font: FontHelper.NotoSans.CreateFont(16), 
								color: errorColor, 
								location: new Point(0, parameter.y));
						});
					}
				}
			}

			return result;
		}
	}
}
