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
			const int size = 1000;
			const int padding = 50;
			const float textWidth = size - padding * 2f;
			const float centerX = size / 2f;

			var titleFont = NotoSans.CreateFont(48);
			var subtitleFont = NotoSans.CreateFont(28);
			var promptFont = NotoSans.CreateFont(20);

			const string titleText = "Mock OpenAI Service";
			const string subtitleText = "No OpenAI API key configured — placeholder image";

			using var image = new Image<Rgba32>(size, size, Color.White);

			image.Mutate(ctx =>
			{
				float y = padding;

				// Title — measure actual rendered height before advancing y
				var titleOptions = new RichTextOptions(titleFont)
				{
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Top,
					Dpi = 96,
					Origin = new PointF(centerX, y),
					WrappingLength = textWidth
				};
				ctx.DrawText(titleOptions, titleText, Color.DarkGray);
				y += TextMeasurer.MeasureBounds(titleText, titleOptions).Height + 20f;

				// Subtitle
				var subtitleOptions = new RichTextOptions(subtitleFont)
				{
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Top,
					Dpi = 96,
					Origin = new PointF(centerX, y),
					WrappingLength = textWidth
				};
				ctx.DrawText(subtitleOptions, subtitleText, Color.Gray);
				y += TextMeasurer.MeasureBounds(subtitleText, subtitleOptions).Height + 30f;

				// Prompt — shrink text until it fits in whatever vertical space is left
				var available = size - padding - y;
				if (available <= 0)
					return;

				var promptText = $"Prompt:\n{prompt}";
				var promptOptions = new RichTextOptions(promptFont)
				{
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top,
					Dpi = 96,
					Origin = new PointF(padding, y),
					WrappingLength = textWidth
				};

				while (promptText.Length > 3
					&& TextMeasurer.MeasureBounds(promptText, promptOptions).Height > available)
				{
					promptText = promptText.Limit(Math.Max(3, (int)(promptText.Length * 0.9f)), "...");
				}

				ctx.DrawText(promptOptions, promptText, Color.Gray);
			});

			var ms = new MemoryStream();
			await image.SaveAsPngAsync(ms, cancellationToken);
			ms.Position = 0;
			return ms;
		}
	}
}
