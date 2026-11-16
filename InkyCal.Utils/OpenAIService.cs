using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using OpenAI.Models;
using SixLabors.ImageSharp.Processing;
using StackExchange.Profiling;

namespace InkyCal.Utils
{
	/// <summary>
	/// Calls the real OpenAI API, enforcing a sliding-window rate limit on image generation.
	/// </summary>
	public sealed class OpenAIService : IOpenAIService
	{
		private readonly string _apiKey;

		/// <summary>
		/// Returns <see cref="ResizeMode.Crop"/>, which fits the entire generated image (text) inside the target bounding box,
		/// </summary>
		/// <value>
		/// The resize mode.
		/// </value>
		public ResizeMode ResizeMode => ResizeMode.Crop;

		// Serialises the full chat→image pipeline so at most one request is in-flight at a time.
		private static readonly SemaphoreSlim _semaphore = new(initialCount: 1, maxCount: 1);

		// Limits image generation to 2 requests per 30-second window.
		private static readonly RateLimiter _rateLimiter = new SlidingWindowRateLimiter(
			new SlidingWindowRateLimiterOptions
			{
				Window = TimeSpan.FromSeconds(30),
				PermitLimit = 2,
				QueueLimit = 100,
				QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
				SegmentsPerWindow = 60
			});

		/// <summary>
		/// Initialises the service with the given API key.
		/// When <paramref name="apiKey"/> is null or empty <see cref="IsAvailable"/> returns <c>false</c>.
		/// </summary>
		/// <param name="apiKey">The OpenAI API key.</param>
		/// <param name="imageModel">The image generation model id, see <see href="https://platform.openai.com/docs/models"/>.</param>
		public OpenAIService(string apiKey, string imageModel = DefaultImageModel)
		{
			_apiKey = apiKey;
			_imageModel = string.IsNullOrWhiteSpace(imageModel) ? DefaultImageModel : imageModel;
		}

		/// <summary>
		/// The image generation model used when none is configured.
		/// </summary>
		public const string DefaultImageModel = "gpt-image-2";

		private readonly string _imageModel;

		/// <inheritdoc/>
		public bool IsAvailable => !string.IsNullOrEmpty(_apiKey);

		/// <inheritdoc/>
		public async Task<string> GetChatCompletionAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
		{
			using var client = new OpenAIClient(_apiKey);
			var messages = new List<Message>
			{
				new Message(Role.System, systemPrompt),
				new Message(Role.User, userPrompt)
			};
			try
			{
				using (MiniProfiler.Current.Step("Getting prompt for image generation"))
				{
					var response = await client.ChatEndpoint.GetCompletionAsync(new ChatRequest(messages), cancellationToken);
					return (string)response.FirstChoice.Message;
				}
			}
			catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
			{
				Trace.TraceError("Too many requests to OpenAI chat endpoint");
				throw;
			}
		}

		/// <inheritdoc/>
		public async Task<Stream> GenerateImageAsync(string prompt, CancellationToken cancellationToken)
		{
			await _semaphore.WaitAsync(cancellationToken);
			try
			{
				using (MiniProfiler.Current.Step("Waiting for image generation slot"))
				{
					var lease = await _rateLimiter.AcquireAsync(cancellationToken: cancellationToken);
					if (!lease.IsAcquired)
						throw new InvalidOperationException("Could not acquire image-generation rate-limit slot.");
				}

				using var client = new OpenAIClient(_apiKey);
				// OpenAI retires image models (dall-e-3, then gpt-image-1 in October 2026), so the model is configurable (OpenAIImageModel).
				// The library passes the model id through as-is, newer models need no library update.
				// GPT image models return the image base64-encoded, which also saves downloading it from a url.
				// Medium quality: low quality yields noticeably rougher images, high quality is mostly lost when dithering down
				// to the few colors of an e-ink display. Images are cached for hours, so generation time and cost matter little.
				var request = new ImageGenerationRequest(prompt, new Model(_imageModel, "openai"), quality: "medium");

				ImageResult imageResult;
				using (MiniProfiler.Current.Step("Generating image"))
					imageResult = (await client.ImagesEndPoint.GenerateImageAsync(request, cancellationToken))[0];

				if (string.IsNullOrEmpty(imageResult.B64_Json))
					throw new InvalidOperationException("Image generation did not return image data.");

				return new MemoryStream(Convert.FromBase64String(imageResult.B64_Json));
			}
			finally
			{
				_semaphore.Release();
			}
		}
	}
}
