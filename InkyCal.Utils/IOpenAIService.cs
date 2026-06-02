using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace InkyCal.Utils
{
	/// <summary>
	/// Abstracts interaction with the OpenAI API for chat completions and image generation.
	/// </summary>
	public interface IOpenAIService
	{
		/// <summary>Whether the service is configured and able to make API calls.</summary>
		bool IsAvailable { get; }

		/// <summary>
		/// Returns the text content of a chat completion response for the given system and user prompts.
		/// </summary>
		Task<string> GetChatCompletionAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken);

		/// <summary>
		/// Generates a DALL-E 3 image for <paramref name="prompt"/> and returns its data as a <see cref="Stream"/>.
		/// </summary>
		Task<Stream> GenerateImageAsync(string prompt, CancellationToken cancellationToken);
	}
}
