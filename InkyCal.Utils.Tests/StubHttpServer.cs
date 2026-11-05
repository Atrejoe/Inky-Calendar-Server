using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Minimal local HTTP server returning canned responses, so image generation providers
	/// can be tested through their real client libraries without network access or API keys.
	/// </summary>
	public sealed class StubHttpServer : IDisposable
	{
		/// <summary>A request received by the stub.</summary>
		public sealed record Request(string Method, string PathAndQuery, IReadOnlyDictionary<string, string> Headers, string Body);

		/// <summary>A canned response.</summary>
		public sealed record Response(string Body, string ContentType = "application/json", int StatusCode = 200)
		{
			/// <summary>Raw bytes, overrides <see cref="Body"/> when set.</summary>
			public byte[] Bytes { get; init; }
		}

		private readonly HttpListener _listener = new();
		private readonly Func<Request, Response> _handler;
		private readonly CancellationTokenSource _cts = new();
		private readonly Task _loop;

		/// <summary>All requests received so far, in order.</summary>
		public ConcurrentQueue<Request> Requests { get; } = new();

		/// <summary>The base address of the stub, ending with a slash.</summary>
		public Uri BaseAddress { get; }

		/// <summary>Starts the stub on a free local port.</summary>
		public StubHttpServer(Func<Request, Response> handler)
		{
			_handler = handler;
			BaseAddress = new Uri($"http://127.0.0.1:{GetFreePort()}/");
			_listener.Prefixes.Add(BaseAddress.ToString());
			_listener.Start();
			_loop = Task.Run(LoopAsync, CancellationToken.None);
		}

		private static int GetFreePort()
		{
			using var socket = new TcpListener(IPAddress.Loopback, 0);
			socket.Start();
			return ((IPEndPoint)socket.LocalEndpoint).Port;
		}

		private async Task LoopAsync()
		{
			while (!_cts.IsCancellationRequested)
			{
				HttpListenerContext context;
				try
				{
					context = await _listener.GetContextAsync();
				}
				catch (Exception) when (_cts.IsCancellationRequested)
				{
					return;
				}

				string body;
				using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
					body = await reader.ReadToEndAsync(_cts.Token);

				var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				foreach (string key in context.Request.Headers.AllKeys)
					headers[key] = context.Request.Headers[key];

				var request = new Request(context.Request.HttpMethod, context.Request.Url.PathAndQuery, headers, body);
				Requests.Enqueue(request);

				Response response;
				try
				{
					response = _handler(request);
				}
				catch (Exception ex)
				{
					response = new Response(ex.ToString(), "text/plain", 500);
				}

				var bytes = response.Bytes ?? Encoding.UTF8.GetBytes(response.Body ?? string.Empty);
				context.Response.StatusCode = response.StatusCode;
				context.Response.ContentType = response.ContentType;
				context.Response.ContentLength64 = bytes.Length;
				await context.Response.OutputStream.WriteAsync(bytes, _cts.Token);
				context.Response.Close();
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			_cts.Cancel();
			_listener.Close();
			try { _loop.Wait(1000, CancellationToken.None); } catch (AggregateException) { /* shutting down */ }
			_cts.Dispose();
		}

		/// <summary>A tiny valid 2×2 PNG.</summary>
		public static byte[] TinyPng()
		{
			using var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(2, 2);
			using var ms = new MemoryStream();
			SixLabors.ImageSharp.ImageExtensions.SaveAsPng(image, ms);
			return ms.ToArray();
		}
	}
}
