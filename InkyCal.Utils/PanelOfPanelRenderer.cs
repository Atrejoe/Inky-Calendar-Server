using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using StackExchange.Profiling;

namespace InkyCal.Utils
{

	/// <summary>
	/// A simpel cache entry per stored panel, regardless of parameters
	/// </summary>
	/// <seealso cref="PanelCacheKey" />
	public class PerPanelCacheKey : PanelCacheKey
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PerPanelCacheKey"/> class.
		/// </summary>
		/// <param name="expiration">The expiration.</param>
		/// <param name="id">The identifier.</param>
		public PerPanelCacheKey(TimeSpan expiration, Guid id) : base(expiration)
		{
			Id = id;
		}

		/// <summary>
		/// Gets the identifier of the <see cref="Panel"/> (<see cref="Panel.Id"/>)
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		public Guid Id { get; }

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(),Id);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another <see cref="T:InkyCal.Models.PanelCacheKey" /> (or derived class))
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
		/// </returns>
		protected override bool Equals(PanelCacheKey other)
		{
			return other is PerPanelCacheKey ppc
				&& ppc.Id == Id;
		}
	}

	/// <summary>
	/// An image panel, assumes a landscape image, resizes and flips it to portait.
	/// </summary>
	public class PanelOfPanelRenderer : IPanelRenderer
	{
		private PanelOfPanels pp;
		private readonly PanelRenderHelper panelRenderHelper;

		/// <summary>
		/// Gets the cache key. By default returns <see cref="PanelInstanceCacheKey" />, with default <see cref="PanelCacheKey.Expiration" /> (<see cref="PanelInstanceCacheKey.DefaultExpirationInSeconds" /> seconds)
		/// </summary>
		public PanelCacheKey CacheKey { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pp"></param>
		/// <param name="panelRenderHelper"></param>
		public PanelOfPanelRenderer(PanelOfPanels pp, PanelRenderHelper panelRenderHelper)
		{
			this.pp = pp ?? throw new ArgumentNullException(nameof(pp));
			this.panelRenderHelper = panelRenderHelper ?? throw new ArgumentNullException(nameof(panelRenderHelper));

			CacheKey = new PerPanelCacheKey(TimeSpan.FromSeconds(30), pp.Id);
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

			var result = PanelRenderingHelper.CreateImage(width, height, backgroundColor);

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


				var quantizer = new PaletteQuantizer(colors);

				foreach (var parameter in renderParameters.AsParallel()){

					var panel = parameter.Panel;
					var renderer = panelRenderHelper.GetRenderer(panel);

					try
					{
						using (MiniProfiler.Current.Step($"Render panel '{panel.Name}' ({panel.GetType().Name})")) {

							var bytes = await renderer.GetCachedImage(width, parameter.subPanelHeight, colors, log);
							var subImage = Image.Load(bytes);

							result.Mutate(
								operation => 
									operation
										.DrawImage(subImage, new Point(0, parameter.y), opacity: 1)
										.Quantize(quantizer) //when overlaying there is a slight color degradation even when opacity = 1, quantizing corrects it.
										);
						}
					}
					catch (Exception ex)
					{
						ex.Data["PanelType"] = panel.GetType().Name;
						ex.Data["PanelId"] = panel.Id;
						ex.Log();
						result.Mutate(x =>
						{
							x.DrawText(
								new TextGraphicsOptions(new GraphicsOptions() { Antialias = false },
								new TextOptions() { 
									WrapTextWidth = width 
								}), 
								ex.Message, 
								new Font(FontHelper.NotoSans, 16), 
								errorColor, 
								new Point(0, y));
						});
					}
				}
			}

			return result;
		}
	}
}
