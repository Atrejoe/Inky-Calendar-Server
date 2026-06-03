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
			const float centerX = size / 2f;

			var titleFont = NotoSans.CreateFont(48);
			var subtitleFont = NotoSans.CreateFont(28);
			var promptFont = NotoSans.CreateFont(20);

			using var image = new Image<Rgba32>(size, size, Color.White);

			image.Mutate(ctx =>
			{
				ctx.DrawText(
					new RichTextOptions(titleFont)
					{
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Top,
						Dpi = 96,
						Origin = new PointF(centerX, padding),
						WrappingLength = size - padding * 2
					},
					"Mock OpenAI Service",
					Color.DarkGray);

				ctx.DrawText(
					new RichTextOptions(subtitleFont)
					{
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Top,
						Dpi = 96,
						Origin = new PointF(centerX, padding + 80),
						WrappingLength = size - padding * 2
					},
					"No OpenAI API key configured — placeholder image",
					Color.Gray);

				ctx.DrawText(
					new RichTextOptions(promptFont)
					{
						HorizontalAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Top,
						Dpi = 96,
						Origin = new PointF(padding, padding + 180),
						WrappingLength = size - padding * 2
					},
					$"Prompt:\n{prompt}",
					Color.Gray);
			});

			var ms = new MemoryStream();
			await image.SaveAsPngAsync(ms, cancellationToken);
			ms.Position = 0;
			return ms;
		}
	}
}
