using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Processing;

namespace SixLabors.ImageSharp.Drawing.Processing
{
	internal static class ImageSharpDrawingCompatibilityExtensions
	{
		public static IImageProcessingContext DrawText(this IImageProcessingContext source, RichTextOptions textOptions, string text, Color color)
			=> source.Paint(canvas => canvas.DrawText(textOptions, text, Brushes.Solid(color), pen: null));

		public static IImageProcessingContext DrawText(this IImageProcessingContext source, string text, Font font, Color color, PointF location)
			=> source.DrawText(new RichTextOptions(font) { Origin = location }, text, color);

		public static IImageProcessingContext DrawText(this IImageProcessingContext source, string text, Font font, Color color, Point location)
			=> source.DrawText(text, font, color, new PointF(location.X, location.Y));

		public static IImageProcessingContext Fill(this IImageProcessingContext source, Color color, Rectangle rectangle)
			=> source.Paint(canvas => canvas.Fill(Brushes.Solid(color), new RectanglePolygon(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height)));

		public static IImageProcessingContext DrawLine(this IImageProcessingContext source, Color color, float thickness, PointF start, PointF end)
			=> source.Paint(canvas => canvas.DrawLine(Pens.Solid(color, thickness), start, end));
	}
}
