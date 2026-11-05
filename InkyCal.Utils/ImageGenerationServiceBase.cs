using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing;
using StackExchange.Profiling;

namespace InkyCal.Utils
{
	/// <summary>
	/// Base class for remote (or local) image generation providers.
	/// Takes care of the provider-independent plumbing: serialising requests, rate limiting, profiling and error logging,
	/// so a provider only has to implement <see cref="GetChatCompletionCoreAsync"/> and <see cref="GenerateImageCoreAsync"/>.
	/// </summary>
	/// <remarks>
	/// Instances hold their own rate limiter, register them as singletons (see <see cref="ImageGenerationServiceFactory"/>).
	/// </remarks>
	public abstract class ImageGenerationServiceBase : IImageGenerationService, IDisposable
	{
		// Serialises image generation so at most one request is in-flight at a time.
		private readonly SemaphoreSlim _semaphore = new(initialCount: 1, maxCount: 1);
		private readonly RateLimiter _rateLimiter;
		private bool _disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="ImageGenerationServiceBase"/> class.
		/// </summary>
		/// <param name="imagesPerWindow">Maximum number of images generated within <paramref name="window"/>.</param>
		/// <param name="window">The sliding rate-limit window; defaults to 30 seconds.</param>
		protected ImageGenerationServiceBase(int imagesPerWindow = 2, TimeSpan? window = null)
		{
			_rateLimiter = new SlidingWindowRateLimiter(
				new SlidingWindowRateLimiterOptions
				{
					Window = window ?? TimeSpan.FromSeconds(30),
					PermitLimit = imagesPerWindow,
					QueueLimit = 100,
					QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
					SegmentsPerWindow = 30
				});
		}

		/// <summary>A short, human readable provider name, used for logging and profiling.</summary>
		public abstract string ProviderName { get; }

		/// <inheritdoc/>
		public abstract bool IsAvailable { get; }

		/// <summary>
		/// Returns <see cref="ResizeMode.Crop"/>: generated images are illustrations, losing some of the edges is fine.
		/// </summary>
		public virtual ResizeMode ResizeMode => ResizeMode.Crop;

		/// <inheritdoc/>
		public async Task<string> GetChatCompletionAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
		{
			try
			{
				using (MiniProfiler.Current.Step($"Getting prompt for image generation ({ProviderName})"))
					return await GetChatCompletionCoreAsync(systemPrompt, userPrompt, cancellationToken);
			}
			catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
			{
				System.Diagnostics.Trace.TraceError($"Too many requests to {ProviderName} chat endpoint");
				throw;
			}
		}

		/// <inheritdoc/>
		public async Task<Stream> GenerateImageAsync(string prompt, CancellationToken cancellationToken)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);

			await _semaphore.WaitAsync(cancellationToken);
			try
			{
				using (MiniProfiler.Current.Step("Waiting for image generation slot"))
				{
					using var lease = await _rateLimiter.AcquireAsync(cancellationToken: cancellationToken);
					if (!lease.IsAcquired)
						throw new InvalidOperationException("Could not acquire image-generation rate-limit slot.");
				}

				byte[] image;
				using (MiniProfiler.Current.Step($"Generating image ({ProviderName})"))
					image = await GenerateImageCoreAsync(prompt, cancellationToken);

				if (image is null || image.Length == 0)
					throw new InvalidOperationException($"Image generation by {ProviderName} did not return image data.");

				return new MemoryStream(image, writable: false);
			}
			catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
			{
				System.Diagnostics.Trace.TraceError($"Too many requests to {ProviderName} image endpoint");
				throw;
			}
			finally
			{
				_semaphore.Release();
			}
		}

		/// <summary>
		/// Provider specific chat completion. Profiling and error logging is handled by the caller.
		/// </summary>
		protected abstract Task<string> GetChatCompletionCoreAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken);

		/// <summary>
		/// Provider specific image generation, returns the encoded image (PNG, JPEG, WebP, ...).
		/// Serialisation, rate limiting and profiling is handled by the caller.
		/// </summary>
		protected abstract Task<byte[]> GenerateImageCoreAsync(string prompt, CancellationToken cancellationToken);

		/// <summary>
		/// Releases the rate limiter and semaphore.
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_rateLimiter.Dispose();
				_semaphore.Dispose();
			}
			_disposed = true;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
