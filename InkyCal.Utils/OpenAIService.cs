using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using OpenAI.Models;

namespace InkyCal.Utils
{
	/// <summary>
	/// Generates images using the OpenAI API.
	/// </summary>
	/// <remarks>
	/// The image model is configurable (setting <c>OpenAIImageModel</c>), gpt-image-1 was shut down by OpenAI on 2026-10-23.
	/// </remarks>
	public sealed class OpenAIService : ImageGenerationServiceBase
	{
		private readonly string _apiKey;

		/// <summary>
		/// Initialises the service with the given API key.
		/// When <paramref name="apiKey"/> is null or empty <see cref="IsAvailable"/> returns <c>false</c>.
		/// </summary>
		/// <param name="apiKey">The OpenAI API key.</param>
		/// <param name="imageModel">The image model, defaults to <see cref="DefaultImageModel"/>.</param>
		public OpenAIService(string apiKey, string imageModel = null)
		{
			_apiKey = apiKey;
			_imageModel = string.IsNullOrWhiteSpace(imageModel) ? DefaultImageModel : imageModel;
		}

		/// <summary>The image model used when none is configured.</summary>
		public const string DefaultImageModel = "gpt-image-2";

		private readonly string _imageModel;

		/// <inheritdoc/>
		public override string ProviderName => "OpenAI";

		/// <inheritdoc/>
		public override bool IsAvailable => !string.IsNullOrEmpty(_apiKey);

		/// <inheritdoc/>
		protected override async Task<string> GetChatCompletionCoreAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
		{
			using var client = new OpenAIClient(_apiKey);
			var messages = new List<Message>
			{
				new Message(Role.System, systemPrompt),
				new Message(Role.User, userPrompt)
			};
			var response = await client.ChatEndpoint.GetCompletionAsync(new ChatRequest(messages), cancellationToken);
			return (string)response.FirstChoice.Message;
		}

		/// <inheritdoc/>
		protected override async Task<byte[]> GenerateImageCoreAsync(string prompt, CancellationToken cancellationToken)
		{
			using var client = new OpenAIClient(_apiKey);
			// The API no longer accepts 'response_format' for dall-e-3 ("Unknown parameter: 'response_format'").
			// gpt-image-* models always return the image base64-encoded, which also saves downloading it from a url.
			// Medium quality: low quality yields noticeably rougher images, high quality is mostly lost when dithering down
			// to the few colors of an e-ink display. Images are cached for hours, so generation time and cost matter little.
			var request = new ImageGenerationRequest(prompt, new Model(_imageModel), quality: "medium");

			var imageResult = (await client.ImagesEndPoint.GenerateImageAsync(request, cancellationToken))[0];

			return string.IsNullOrEmpty(imageResult.B64_Json)
				? null
				: Convert.FromBase64String(imageResult.B64_Json);
		}
	}
}
