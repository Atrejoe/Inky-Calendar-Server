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
using StackExchange.Profiling;

namespace InkyCal.Utils
{
	/// <summary>
	/// Calls the real OpenAI API, enforcing a sliding-window rate limit on image generation.
	/// </summary>
	public sealed class OpenAIService : IOpenAIService
	{
		private readonly string _apiKey;

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
		public OpenAIService(string apiKey) => _apiKey = apiKey;

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
				var request = new ImageGenerationRequest(prompt, Model.DallE_3, responseFormat: ImageResponseFormat.Url);

				ImageResult imageResult;
				using (MiniProfiler.Current.Step("Generating image"))
					imageResult = (await client.ImagesEndPoint.GenerateImageAsync(request, cancellationToken))[0];

				var ms = new MemoryStream();
				using var http = new HttpClient();
				using (MiniProfiler.Current.Step($"Downloading image url: {imageResult.Url}"))
				{
					using var s = await http.GetStreamAsync(imageResult.Url, cancellationToken);
					await s.CopyToAsync(ms, cancellationToken);
				}
				ms.Position = 0;
				return ms;
			}
			finally
			{
				_semaphore.Release();
			}
		}
	}
}
