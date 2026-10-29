using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using Sentry;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using StackExchange.Profiling;

namespace InkyCal.Utils
{

	/// <summary>
	/// A simpel cache entry per stored panel, regardless of parameters
	/// </summary>
	/// <seealso cref="PanelCacheKey" />
	/// <remarks>
	/// Initializes a new instance of the <see cref="PerPanelCacheKey"/> class.
	/// </remarks>
	/// <param name="expiration">The expiration.</param>
	/// <param name="id">The identifier.</param>
	public class PerPanelCacheKey(TimeSpan expiration, Guid id) : PanelCacheKey(expiration)
	{

		/// <summary>
		/// Gets the identifier of the <see cref="Panel"/> (<see cref="Panel.Id"/>)
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		public Guid Id { get; } = id;

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Id);

		/// <summary>
		/// Refers to <see cref="Equals(PanelCacheKey)"/>.
		/// </summary>
		public override bool Equals(object obj)
			=> Equals(obj as PerPanelCacheKey);

		/// <summary>
		/// Indicates whether the current object is equal to another <see cref="T:InkyCal.Models.PanelCacheKey" /> (or derived class))
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
		/// </returns>
		protected override bool Equals(PanelCacheKey other) => other is PerPanelCacheKey ppc
				&& ppc.Id == Id;
	}

	/// <summary>
	/// An image panel, assumes a landscape image, resizes and flips it to portait.
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	/// <param name="pp"></param>
	/// <param name="panelRenderHelper"></param>
	public class PanelOfPanelRenderer(PanelOfPanels pp, PanelRenderHelper panelRenderHelper) : IPanelRenderer
	{
		private readonly PanelOfPanels pp = pp ?? throw new ArgumentNullException(nameof(pp));
		private readonly PanelRenderHelper panelRenderHelper = panelRenderHelper ?? throw new ArgumentNullException(nameof(panelRenderHelper));

		/// <summary>
		/// Gets the cache key. By default returns <see cref="PanelInstanceCacheKey" />, with default <see cref="PanelCacheKey.Expiration" /> (<see cref="PanelInstanceCacheKey.DefaultExpirationInSeconds" /> seconds)
		/// </summary>
		public PanelCacheKey CacheKey { get; } = new PerPanelCacheKey(TimeSpan.FromSeconds(30), pp.Id);


		/// <inheritdoc/>
		/// <returns>A panel with nested panels</returns>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Contains catch-all-log and do something logic")]
		public async Task<Image> GetImage(int width, int height, Color[] colors, IPanelRenderer.Log log)
		{
			colors.ExtractMeaningFullColors(
				primaryColor: out _ // Image generation is pass to sub-panels, primary color is not used
				, supportColor: out _ //Image generation is pass to sub-panels, support color is not used
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

				// Render all sub-panels concurrently: most of their time is spent waiting on I/O (calendars, weather, images),
				// while the CPU-bound part (drawing, GIF encoding) benefits from the available cores.
				// NB: 'foreach (... in x.AsParallel())' with an await inside does not run in parallel, it enumerates sequentially.
				var parentSpan = SentrySdk.GetSpan();
				var rendered = await Task.WhenAll(renderParameters // Enumerated once (by WhenAll), the projection keeps track of y
					.Select(parameter => Task.Run(async () =>
					{
						var panel = parameter.Panel;
						var span = parentSpan?.StartChild("panel.render.subpanel", $"{panel.GetType().Name} '{panel.Name}' ({width}x{parameter.subPanelHeight})");
						try
						{
							using (MiniProfiler.Current.Step($"Render panel '{panel.Name}' ({panel.GetType().Name})"))
							{
								var renderer = panelRenderHelper.GetRenderer(panel);
								var bytes = await renderer.GetCachedImage(width, parameter.subPanelHeight, colors, log);
								span?.Finish(SpanStatus.Ok);
								return (parameter.y, bytes, error: (Exception)null);
							}
						}
						catch (Exception ex)
						{
							span?.Finish(ex);
							ex.Data["PanelType"] = panel.GetType().Name;
							ex.Data["PanelId"] = panel.Id;
							ex.Log();
							return (parameter.y, bytes: (byte[])null, error: ex);
						}
					})));

				// Compose sequentially, in sort order
				using (MiniProfiler.Current.Step("Composing sub-panels"))
				{
					var composeSpan = parentSpan?.StartChild("panel.render.compose", $"{rendered.Length:n0} sub-panels");

					foreach (var (subPanelY, bytes, error) in rendered)
					{
						if (error is null)
						{
							using var subImage = Image.Load<Rgba32>(bytes);
							result.Mutate(operation => operation.DrawImage(subImage, new Point(0, subPanelY), opacity: 1));
						}
						else
							result.Mutate(x =>
							{
								x.DrawText(
									textOptions: new RichTextOptions(new Font(FontHelper.NotoSans, 16))
									{
										WrappingLength = width,
										Origin = new(0, subPanelY)
									},
									error.Message,
									errorColor);
							});
					}

					//when overlaying there is a slight color degradation even when opacity = 1, quantizing corrects it.
					//Quantizing once, after all sub-panels have been drawn, suffices.
					result.Mutate(operation => operation.Quantize(quantizer));

					composeSpan?.Finish(SpanStatus.Ok);
				}
			}

			return result;
		}
	}
}
