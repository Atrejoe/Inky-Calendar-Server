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
			const int padding = 20;
			const int maxTextWidth = 800;
			const float lineGap = 8f;

			var titleFont = NotoSans.CreateFont(24);
			var subtitleFont = NotoSans.CreateFont(14);
			var promptFont = NotoSans.CreateFont(12);

			const string titleText = "Mock OpenAI Service";
			const string subtitleText = "No OpenAI API key configured — placeholder image";
			var promptText = string.IsNullOrEmpty(prompt) ? "(no prompt)" : $"Prompt:\n{prompt}";

			// Measure each block at its draw position before creating the image,
			// so the canvas is sized to exactly contain the text.
			float y = padding;

			var titleOptions = new RichTextOptions(titleFont)
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Dpi = 96,
				Origin = new PointF(padding, y),
				WrappingLength = maxTextWidth
			};
			var titleBounds = TextMeasurer.MeasureBounds(titleText, titleOptions);
			y += titleBounds.Height + lineGap;

			var subtitleOptions = new RichTextOptions(subtitleFont)
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Dpi = 96,
				Origin = new PointF(padding, y),
				WrappingLength = maxTextWidth
			};
			var subtitleBounds = TextMeasurer.MeasureBounds(subtitleText, subtitleOptions);
			y += subtitleBounds.Height + lineGap * 2;

			var promptOptions = new RichTextOptions(promptFont)
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Dpi = 96,
				Origin = new PointF(padding, y),
				WrappingLength = maxTextWidth
			};
			var promptBounds = TextMeasurer.MeasureBounds(promptText, promptOptions);
			y += promptBounds.Height;

			var canvasWidth = (int)Math.Ceiling(Math.Max(
				titleBounds.X + titleBounds.Width,
				Math.Max(subtitleBounds.X + subtitleBounds.Width, promptBounds.X + promptBounds.Width))) + padding;
			var canvasHeight = (int)(y + padding);

			using var image = new Image<Rgba32>(canvasWidth, canvasHeight, Color.White);
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
