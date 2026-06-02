using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// A mock <see cref="IOpenAIService"/> for unit tests: always available, returns deterministic results without network calls.
	/// </summary>
	internal sealed class MockOpenAIService : IOpenAIService
	{
		public bool IsAvailable => true;

		public Task<string> GetChatCompletionAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
			=> Task.FromResult("A beautiful 19th century lithograph");

		public async Task<Stream> GenerateImageAsync(string prompt, CancellationToken cancellationToken)
		{
			var ms = new MemoryStream();
			using (var image = new Image<Rgba32>(100, 100, Color.White))
				await image.SaveAsPngAsync(ms, cancellationToken);
			ms.Position = 0;
			return ms;
		}
	}
}
