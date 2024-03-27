// Ignore Spelling: Utils

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InkyCal.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Type = System.Type;

namespace InkyCal.Utils
{
	/// <summary>
	/// A helper class for mapping a <see cref="Panel"/> to a <see cref="IPanelRenderer"/>.
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	public class PanelRenderHelper(Func<GoogleOAuthAccess, Task> saveToken)
	{

		/// <summary>
		/// Gets a <see cref="IPanelRenderer"/> for the spcified <paramref name="panel"/>.
		/// </summary>
		/// <param name="panel"></param>
		/// <returns></returns>
		/// <exception cref="GetRendererException"/>
		public IPanelRenderer GetRenderer(Panel panel)
		{
			ArgumentNullException.ThrowIfNull(panel);

			IPanelRenderer renderer;

			switch (panel)
			{
				case CalendarPanel cp:

					var urls = cp.CalenderUrls.Select(x => new Uri(x.Url));

					renderer = new CalendarPanelRenderer(
									saveToken,
									iCalUrls: urls.ToArray(),
									cp.SubscribedGoogleCalenders?.ToArray(),
									cp.DrawMode
									);

					break;
				case ImagePanel ip:

					var imageRotation = (RotateMode)(((int)ip.ImageRotation - (int)panel.Rotation + 360) % 360);
					renderer = new ImagePanelRenderer(new Uri(ip.Path), imageRotation);
					break;

				case PanelOfPanels pp:
					renderer = new PanelOfPanelRenderer(pp, this);
					break;

				case WeatherPanel wp:
					renderer = new WeatherPanelRenderer(wp);
					break;

				default:
					{
						try
						{
							var rendererTypes = Renderers.Value.Where(x => typeof(PanelRenderer<>).IsSubclassOfRawGeneric(x));

							var rendererType = rendererTypes.SingleOrDefault(x => x.BaseType.GetGenericArguments()[0].Equals(panel.GetType()));

							if (rendererType == null)
								throw new NotImplementedException($"Rendering of {panel.GetType().Name} has not yet been implemented");

							var paneltype = rendererType.BaseType.GetGenericArguments()[0];
							var c = rendererType.GetConstructor(new[] { paneltype });
							if (c is null)
							{
								c = rendererType.GetConstructor(Type.EmptyTypes);
								if (c is null)
									throw new NotImplementedException($"Renderer of {rendererType.Name} does not have a parameterless constructor, nor one that takes a {panel.GetType().Name} are argument");
								else
									renderer = (IPanelRenderer)c.Invoke(Type.EmptyTypes);
							}
							else
								renderer = (IPanelRenderer)c.Invoke(new[] { panel });
						}
						catch (Exception ex)
						{
							throw new GetRendererException($"Could not determine panel renderer for {panel.GetType().Name}, see inner exception for details: {ex.Message}", ex);
						}
					}
					break;
			}

			return renderer;
		}

		private static readonly Lazy<Type[]> Renderers = new Lazy<Type[]>(GetRenderers);
		private readonly Func<GoogleOAuthAccess, Task> saveToken = saveToken;

		internal static Type[] GetRenderers()
		{
			var topLevelNamespace = typeof(PanelRenderHelper).Assembly.FullName[
						..typeof(PanelRenderHelper).Assembly.FullName.IndexOf(".")];

			return AppDomain.CurrentDomain.GetAssemblies()
				.Where(x => x.GetName().FullName.StartsWith(topLevelNamespace))
				.SelectMany(x => x.GetTypes())
				.Where(x => typeof(IPanelRenderer).IsAssignableFrom(x)
							&& !x.IsInterface
							&& !x.IsAbstract)
				.ToArray();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public static class PanelRenderingHelper
	{

		/// <summary>
		/// Returns meaningful colors for drawing purposes
		/// </summary>
		/// <param name="colors"></param>
		/// <param name="primaryColor"></param>
		/// <param name="supportColor"></param>
		/// <param name="errorColor"></param>
		/// <param name="backgroundColor"></param>
		public static void ExtractMeaningFullColors(
			this Color[] colors,
			out Color primaryColor,
			out Color supportColor,
			out Color errorColor,
			out Color backgroundColor)
		{
			ArgumentNullException.ThrowIfNull(colors);

			primaryColor = colors.FirstOrDefault();
			supportColor = (colors.Length > 2) ? colors[2] : primaryColor;
			errorColor = supportColor;
			backgroundColor = colors.Skip(1).FirstOrDefault();
		}

		/// <summary>
		/// Creates an image, caller needs to dispose
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="backgroundColor"></param>
		/// <returns></returns>
		public static Image<Rgba32> CreateImage(int width, int height, Color? backgroundColor = null) => new Image<Rgba32>(width, height, backgroundColor.GetValueOrDefault(Color.White));

		internal static void RenderErrorMessage(
			this IImageProcessingContext canvas,
			string errorMessage,
			Color errorColor,
			Color backgroundColor,
			ref int y,
			int width,
			Font font)
		{
			var textOptions = new RichTextOptions(font)
				{
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top,
					WrappingLength = width,
					Dpi = 96
				};

			canvas.RenderErrorMessage(errorMessage, errorColor, backgroundColor, ref y, width, textOptions);
		}

		internal static IImageProcessingContext RenderErrorMessage(
		this IImageProcessingContext canvas,
		string errorMessage,
		Color errorColor,
		Color backgroundColor,
		ref int y,
		int width,
		RichTextOptions renderOptions)
		{

			var errorTextOptions = new RichTextOptions(renderOptions)
			{
				WrappingLength = width - 4
			};

			var errorMessageHeight = TextMeasurer.MeasureBounds(errorMessage, errorTextOptions);

			canvas.Fill(errorColor,
				new Rectangle(
					(int)errorMessageHeight.X,
					(int)errorMessageHeight.Y,
					(int)errorMessageHeight.Width,
					(int)errorMessageHeight.Height + 4) //Pad 2 px on all sides
				);

			PointF pError = new(errorMessageHeight.X + 2, errorMessageHeight.Y + 2);//Adhere to padding

			var trimmedErrorMessage = new StringBuilder();
			foreach (var line in errorMessage.Split(Environment.NewLine))
				trimmedErrorMessage.AppendLine(line.Limit(width, "..."));

			canvas.DrawText(
				textOptions: new RichTextOptions(errorTextOptions) 
					{ Origin = pError
					},
				text: trimmedErrorMessage.ToString().ToSafeChars(renderOptions.Font),
				color: backgroundColor);

			y += (int)errorMessageHeight.Height;

			return canvas;
		}
	}
}
