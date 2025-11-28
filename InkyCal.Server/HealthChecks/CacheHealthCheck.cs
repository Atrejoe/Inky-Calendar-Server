using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InkyCal.Server.Config;
using InkyCal.Utils.Caching;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace InkyCal.Server.HealthChecks
{
	/// <summary>
	/// Health check for the caching system
	/// </summary>
	public class CacheHealthCheck : IHealthCheck
	{
		private readonly IImageCacheService _imageCacheService;
		private readonly IStringCacheService _stringCacheService;
		private readonly CacheType _cacheType;

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheHealthCheck"/> class.
		/// </summary>
		/// <param name="imageCacheService">The image cache service.</param>
		/// <param name="stringCacheService">The string cache service.</param>
		public CacheHealthCheck(IImageCacheService imageCacheService, IStringCacheService stringCacheService)
		{
			_imageCacheService = imageCacheService ?? throw new ArgumentNullException(nameof(imageCacheService));
			_stringCacheService = stringCacheService ?? throw new ArgumentNullException(nameof(stringCacheService));
			_cacheType = Config.Config.CacheType;
		}

		/// <inheritdoc/>
		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
		{
			var data = new Dictionary<string, object>
			{
				["CacheType"] = _cacheType.ToString()
			};

			try
			{
				if (_cacheType == CacheType.Redis)
				{
					// Test Redis connectivity
					var testKey = $"healthcheck_{Guid.NewGuid()}";
					var testValue = BitConverter.GetBytes(DateTime.UtcNow.Ticks);

					await _imageCacheService.SetAsync(testKey, testValue, TimeSpan.FromSeconds(5));
					var (found, _) = await _imageCacheService.TryGetValueAsync(testKey);

					if (!found)
					{
						return HealthCheckResult.Unhealthy(
							"Redis cache is not functioning correctly - test write/read failed",
							data: data);
					}

					// Try to get Redis stats if possible
					if (_imageCacheService is RedisCacheService redisCache)
					{
						try
						{
							var stats = GetRedisStats(redisCache);
							foreach (var stat in stats)
								data[stat.Key] = stat.Value;
						}
						catch (Exception ex)
						{
							data["StatsError"] = ex.Message;
						}
					}

					data["Status"] = "Redis cache is operational";
					return HealthCheckResult.Healthy("Redis cache is working correctly", data: data);
				}
				else
				{
					// Memory cache
					var imageCount = _imageCacheService.Count();
					var stringCount = _stringCacheService.Count();

					data["ImageCacheEntries"] = imageCount;
					data["StringCacheEntries"] = stringCount;
					data["MemorySizeLimit"] = Config.Config.MemoryCacheSizeLimit;
					data["Status"] = "In-memory cache is operational";

					return HealthCheckResult.Healthy("Memory cache is working correctly", data: data);
				}
			}
			catch (RedisConnectionException ex)
			{
				data["Error"] = ex.Message;
				return HealthCheckResult.Degraded(
					"Redis cache connection issue - falling back to memory cache",
					ex,
					data);
			}
			catch (Exception ex)
			{
				data["Error"] = ex.Message;
				return HealthCheckResult.Degraded(
					$"Cache health check failed: {ex.Message}",
					ex,
					data);
			}
		}

		private static Dictionary<string, object> GetRedisStats(RedisCacheService redisCache)
		{
			var stats = new Dictionary<string, object>();

			try
			{
				// Use reflection to access the database field
#pragma warning disable S3011 // Accessibility bypass is safe here - we own both classes
				var databaseField = typeof(RedisCacheService).GetField("_database",
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
#pragma warning restore S3011

				if (databaseField?.GetValue(redisCache) is IDatabase database)
				{
					var multiplexer = database.Multiplexer;
					var endpoints = multiplexer.GetEndPoints();

					if (endpoints.Length > 0)
					{
						var server = multiplexer.GetServer(endpoints[0]);
						var info = server.Info("stats");

						foreach (var section in info)
						{
							var relevantItems = section.Where(item =>
								item.Key.Contains("keyspace") ||
								item.Key.Contains("connections") ||
								item.Key.Contains("memory") ||
								item.Key.Contains("ops"));

							foreach (var item in relevantItems)
							{
								stats[$"Redis_{item.Key}"] = item.Value;
							}
						}

						stats["RedisConnected"] = multiplexer.IsConnected;
						stats["RedisEndpoints"] = string.Join(", ", endpoints.Select(e => e.ToString()));
					}
				}
			}
			catch
			{
				// If we can't get stats, just ignore
			}

			return stats;
		}
	}
}
