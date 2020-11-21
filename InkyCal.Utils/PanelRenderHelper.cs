using System;
using System.Linq;
using System.Text;
using InkyCal.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace InkyCal.Utils
{
	/// <summary>
	/// A helper class for mapping a <see cref="Panel"/> to a <see cref="IPanelRenderer"/>.
	/// </summary>
	public static class PanelRenderHelper
	{

		/// <summary>
		/// Gets a <see cref="IPanelRenderer"/> for the spcified <paramref name="panel"/>.
		/// </summary>
		/// <param name="panel"></param>
		/// <returns></returns>
		public static IPanelRenderer GetRenderer(this Models.Panel panel)
		{
			if (panel is null)
				throw new ArgumentNullException(nameof(panel));

			IPanelRenderer renderer;

			switch (panel)
			{
				case CalendarPanel cp:

					var urls = cp.CalenderUrls.Select(x => new Uri(x.Url));
					
					renderer = new CalendarPanelRenderer(
									iCalUrls: urls.ToArray(), 
									cp.Owner.GoogleOAuthTokens?.ToArray(),
									cp.SubscribedGoogleCalenders?.ToArray()
									);

					break;
				case ImagePanel ip:

					var imageRotation = (RotateMode)(((int)ip.ImageRotation - (int)panel.Rotation + 360) % 360);
					renderer = new ImagePanelRenderer(new Uri(ip.Path), imageRotation);
					break;

				case PanelOfPanels pp:
					renderer = new PanelOfPanelRenderer(pp);
					break;

				case WeatherPanel wp:
					renderer = new WeatherPanelRenderer(wp);
					break;

				default:
					{
						try
						{
							var rendererTypes = Renderers.Value.Where(x => IsSubclassOfRawGeneric(typeof(PanelRenderer<>), x));

							if (rendererTypes is null)
								throw new NotImplementedException($"Rendering of {panel.GetType().Name} has not yet been implemented");

							var rendererType = rendererTypes.SingleOrDefault(x => x.BaseType.GetGenericArguments().First().Equals(panel.GetType()));

							if (rendererType == null)
								throw new NotImplementedException($"Rendering of {panel.GetType().Name} has not yet been implemented");

							var paneltype = rendererType.BaseType.GetGenericArguments().First();
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
							//throw new NotImplementedException($"Rendering of {panel.GetType().Name} has not yet been implemented");
						}
						catch (Exception ex)
						{
							throw new Exception($"Could not determine panel renderer for {panel.GetType().Name}, see inner exception for details: {ex.Message}", ex);
						}
					}
					break;
			}

			return renderer;
		}

		private static readonly Lazy<Type[]> Renderers = new Lazy<Type[]>(GetRenderers);

		private static Type[] GetRenderers()
		{
			return AppDomain.CurrentDomain.GetAssemblies()
														.SelectMany(x => x.GetTypes())
														.Where(x => typeof(IPanelRenderer).IsAssignableFrom(x)
																	&& !x.IsInterface
																	&& !x.IsAbstract)
														.ToArray();
		}

		static bool IsSubclassOfRawGeneric(this Type generic, Type toCheck)
		{
			while (toCheck != null && toCheck != typeof(object))
			{
				var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if (generic == cur)
				{
					return true;
				}
				toCheck = toCheck.BaseType;
			}
			return false;
		}

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
			if (colors is null)
				throw new ArgumentNullException(nameof(colors));


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
		public static Image CreateImage(int width, int height, Color? backgroundColor = null)
		{
			return new Image<Rgba32>(new Configuration() { }, width, height, backgroundColor.GetValueOrDefault(Color.White));
		}

		internal static void RenderErrorMessage(
			this IImageProcessingContext canvas,
			string errorMessage,
			Color errorColor,
			Color backgroundColor,
			ref int y,
			int width,
			Font font)
		{
			var rendererOptions = new TextGraphicsOptions(
				new GraphicsOptions() { Antialias = false }, 
				new TextOptions() { 
					HorizontalAlignment = HorizontalAlignment.Left, 
					VerticalAlignment= VerticalAlignment.Top, 
					WrapTextWidth = width,
					DpiX =96, 
					DpiY = 96 }
				).ToRendererOptions(font);

			canvas.RenderErrorMessage(errorMessage, errorColor, backgroundColor, ref y, width, rendererOptions);
		}

		internal static void RenderErrorMessage(
		this IImageProcessingContext canvas,
		string errorMessage,
		Color errorColor,
		Color backgroundColor,
		ref int y,
		int width,
		RendererOptions renderOptions)
		{

			renderOptions.WrappingWidth = width - 4;
			var textDrawOptions_Error = renderOptions.ToTextGraphicsOptions(false);

			var errorMessageHeight = TextMeasurer.MeasureBounds(errorMessage, renderOptions);

			canvas.Fill(errorColor, 
				new Rectangle(
					(int)errorMessageHeight.X, 
					(int)errorMessageHeight.Y, 
					(int)errorMessageHeight.Width, 
					(int)errorMessageHeight.Height+4)//Pad 2 px on all sides
				);

			var pError = new PointF(2, 2);//Adhere to padding

			var trimmedErrorMessage = new StringBuilder();
			foreach (var line in errorMessage.Split(Environment.NewLine))
				trimmedErrorMessage.AppendLine(line.Limit(width, "..."));

			canvas.DrawText(textDrawOptions_Error, trimmedErrorMessage.ToString().ToSafeChars(renderOptions.Font), renderOptions.Font, backgroundColor, pError);

			y += (int)errorMessageHeight.Height;
		}
	}
}
