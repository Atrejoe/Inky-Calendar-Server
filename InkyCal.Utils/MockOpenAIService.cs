using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static InkyCal.Utils.FontHelper;

namespace InkyCal.Utils
{
	/// <summary>
	/// An <see cref="IOpenAIService"/> that returns deterministic placeholder results without making
	/// any network calls. Registered automatically when no OpenAI API key is configured; also used in unit tests.
	/// </summary>
	public sealed class MockOpenAIService : IOpenAIService
	{
		/// <inheritdoc/>
		public bool IsAvailable => true;

		/// <inheritdoc/>
		public Task<string> GetChatCompletionAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
			=> Task.FromResult("A beautiful 19th century lithograph (mock — no API key configured)");

		/// <inheritdoc/>
		public async Task<Stream> GenerateImageAsync(string prompt, CancellationToken cancellationToken)
		{
			// 1024×1024 matches DALL-E 3's default output size. The renderer crop-resizes every
			// image to fit the panel (ResizeMode.Crop, AnchorPositionMode.Center), so text must
			// be centered in the canvas to survive any panel aspect ratio.
			const int size = 1024;
			const float cx = size / 2f;
			const float textWidth = 900f;
			const float lineGap = 12f;

			var titleFont = NotoSans.CreateFont(36);
			var subtitleFont = NotoSans.CreateFont(20);
			var promptFont = NotoSans.CreateFont(14);

			const string titleText = "Mock OpenAI Service";
			const string subtitleText = "No OpenAI API key configured — placeholder image";
			var promptText = string.IsNullOrEmpty(prompt) ? "(no prompt)" : $"Prompt:\n{prompt}";

			// Pass 1: measure at y=0 to compute total block height for vertical centering.
			var titleH = TextMeasurer.MeasureBounds(titleText, new RichTextOptions(titleFont)
			{
				HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top,
				Dpi = 96, Origin = new PointF(cx, 0), WrappingLength = textWidth
			}).Height;
			var subtitleH = TextMeasurer.MeasureBounds(subtitleText, new RichTextOptions(subtitleFont)
			{
				HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top,
				Dpi = 96, Origin = new PointF(cx, 0), WrappingLength = textWidth
			}).Height;
			var promptH = TextMeasurer.MeasureBounds(promptText, new RichTextOptions(promptFont)
			{
				HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top,
				Dpi = 96, Origin = new PointF(cx, 0), WrappingLength = textWidth
			}).Height;

			var totalH = titleH + lineGap + subtitleH + lineGap * 2 + promptH;

			// Pass 2: place blocks centered vertically within the canvas.
			float y = (size - totalH) / 2f;

			var titleOptions = new RichTextOptions(titleFont)
			{
				HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top,
				Dpi = 96, Origin = new PointF(cx, y), WrappingLength = textWidth
			};
			y += titleH + lineGap;

			var subtitleOptions = new RichTextOptions(subtitleFont)
			{
				HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top,
				Dpi = 96, Origin = new PointF(cx, y), WrappingLength = textWidth
			};
			y += subtitleH + lineGap * 2;

			var promptOptions = new RichTextOptions(promptFont)
			{
				HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top,
				Dpi = 96, Origin = new PointF(cx, y), WrappingLength = textWidth
			};

			using var image = new Image<Rgba32>(size, size, Color.White);
			image.Mutate(ctx =>
			{
				ctx.DrawText(titleOptions, titleText, Color.DarkGray);
				ctx.DrawText(subtitleOptions, subtitleText, Color.Gray);
				ctx.DrawText(promptOptions, promptText, Color.Gray);
			});

			var ms = new MemoryStream();
			await image.SaveAsPngAsync(ms, cancellationToken);
			ms.Position = 0;
			return ms;
		}
	}
}
