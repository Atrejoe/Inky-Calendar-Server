using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InkyCal.Server.Config;
using InkyCal.Utils;
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
				return _cacheType == CacheType.Redis
					? await CheckRedisCacheAsync(data)
					: CheckMemoryCache(data);
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

		private async Task<HealthCheckResult> CheckRedisCacheAsync(Dictionary<string, object> data)
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
					await AddRedisStatsAsync(redisCache.Multiplexer, data);
				}
				catch (Exception ex)
				{
					data["StatsError"] = ex.Message;
				}
			}

			data["Status"] = "Redis cache is operational";
			return HealthCheckResult.Healthy("Redis cache is working correctly", data: data);
		}

		private HealthCheckResult CheckMemoryCache(Dictionary<string, object> data)
		{
			var imageCount = _imageCacheService.Count();
			var stringCount = _stringCacheService.Count();
			var cacheSize = _imageCacheService.GetApproximateSize();
			var cacheSizeLimit = Config.Config.MemoryCacheSizeLimit;

			data["ImageCacheEntries"] = imageCount;
			data["StringCacheEntries"] = stringCount;
			data["TotalEntries"] = imageCount + stringCount;

			// Add human-readable sizes
			if (cacheSize >= 0)
			{
				data["CacheSize"] = ByteSizeFormatter.FormatBytesAuto(cacheSize);
				data["CacheSizeBytes"] = cacheSize;
			}

			data["CacheSizeLimit"] = ByteSizeFormatter.FormatBytesAuto(cacheSizeLimit);
			data["CacheSizeLimitBytes"] = cacheSizeLimit;

			// Calculate utilization percentage
			if (cacheSize >= 0 && cacheSizeLimit > 0)
			{
				var utilizationPercent = (double)cacheSize / cacheSizeLimit * 100;
				data["CacheUtilization"] = $"{utilizationPercent:F1}%";
				data["CacheUtilizationPercent"] = Math.Round(utilizationPercent, 1);
			}

			data["Status"] = "In-memory cache is operational";

			return HealthCheckResult.Healthy("Memory cache is working correctly", data: data);
		}

		private static async Task AddRedisStatsAsync(IConnectionMultiplexer multiplexer, Dictionary<string, object> data)
		{
			var endpoints = multiplexer.GetEndPoints();
			if (endpoints.Length == 0)
				return;

			var server = multiplexer.GetServer(endpoints[0]);

			AddStats(await server.InfoAsync("stats"), data);
			AddMemoryStats(await server.InfoAsync("memory"), data);

			data["RedisConnected"] = multiplexer.IsConnected;
			data["RedisEndpoints"] = string.Join(", ", endpoints.Select(e => e.ToString()));
		}

		private static void AddStats(IGrouping<string, KeyValuePair<string, string>>[] info, Dictionary<string, object> data)
		{
			var relevantItems = info
				.SelectMany(section => section)
				.Where(item =>
					item.Key.Contains("keyspace", StringComparison.OrdinalIgnoreCase) ||
					item.Key.Contains("connections", StringComparison.OrdinalIgnoreCase) ||
					item.Key.Contains("ops", StringComparison.OrdinalIgnoreCase));

			foreach (var item in relevantItems)
				data[$"Redis_{item.Key}"] = item.Value;
		}

		private static void AddMemoryStats(IGrouping<string, KeyValuePair<string, string>>[] memoryInfo, Dictionary<string, object> data)
		{
			// Human-readable formatting
			var usedMemoryItems = memoryInfo
				.SelectMany(section => section)
				.Where(item =>
					item.Key.Contains("memory", StringComparison.OrdinalIgnoreCase) &&
					item.Key.Contains("used", StringComparison.OrdinalIgnoreCase));

			foreach (var item in usedMemoryItems)
			{
				if (long.TryParse(item.Value, out var bytes))
				{
					data[$"Redis_{item.Key}"] = ByteSizeFormatter.FormatBytesAuto(bytes);
					data[$"Redis_{item.Key}_bytes"] = bytes;
				}
				else
				{
					data[$"Redis_{item.Key}"] = item.Value;
				}
			}
		}
	}
}
