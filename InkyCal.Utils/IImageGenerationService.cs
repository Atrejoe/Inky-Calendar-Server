using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing;

namespace InkyCal.Utils
{
	/// <summary>
	/// Turns a system and user prompt into a single text response. Used to compose an image prompt from calendar events.
	/// </summary>
	public interface IChatCompletionService
	{
		/// <summary>
		/// Returns the text content of a chat completion response for the given system and user prompts.
		/// </summary>
		Task<string> GetChatCompletionAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken);
	}

	/// <summary>
	/// Abstracts an image generation provider (OpenAI, Google, Black Forest Labs, a local model, ...).
	/// Also exposes <see cref="IChatCompletionService"/>, because the calendar image prompt is composed by a language model first.
	/// </summary>
	/// <seealso cref="ImageGenerationServiceBase"/>
	public interface IImageGenerationService : IChatCompletionService
	{
		/// <summary>Whether the service is configured and able to make API calls.</summary>
		bool IsAvailable { get; }

		/// <summary>
		/// Gets the resize mode, how the generated image should fit inside the target bounding box
		/// </summary>
		/// <value>
		/// The resize mode.
		/// </value>
		ResizeMode ResizeMode { get; }

		/// <summary>
		/// Generates an image for <paramref name="prompt"/> and returns its (encoded, e.g. PNG/JPEG/WebP) data as a <see cref="Stream"/>.
		/// </summary>
		Task<Stream> GenerateImageAsync(string prompt, CancellationToken cancellationToken);
	}
}
