using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace InkyCal.Utils.Caching
{
	/// <summary>
	/// Redis cache implementation using StackExchange.Redis
	/// </summary>
	public class RedisCacheService : IImageCacheService
	{
		private readonly IDatabase _database;
		private static readonly TimeSpan DefaultLockTimeout = TimeSpan.FromSeconds(30);
		private static readonly TimeSpan LockRetryDelay = TimeSpan.FromMilliseconds(100);

		/// <summary>
		/// Initializes a new instance of the <see cref="RedisCacheService"/> class.
		/// </summary>
		/// <param name="redis">The Redis connection multiplexer.</param>
		public RedisCacheService(IConnectionMultiplexer redis)
		{
			ArgumentNullException.ThrowIfNull(redis);
			_database = redis.GetDatabase();
		}

		/// <summary>
		/// Gets the Redis connection, for diagnostics (e.g. health checks).
		/// </summary>
		public IConnectionMultiplexer Multiplexer => _database.Multiplexer;

		/// <inheritdoc/>
		public async Task<(bool Found, byte[] Value)> TryGetValueAsync<T>(T key) where T : IEquatable<T>
		{
			return await TryGetValueAsync(key.SerializeToJson());
		}

		/// <inheritdoc/>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Redis errors should not crash the application")]
		public async Task<(bool Found, byte[] Value)> TryGetValueAsync(string key)
		{
			try
			{
				// Use CommandFlags.PreferReplica to allow reading from replicas
				// This distributes read load across master and replicas
				var value = await _database.StringGetAsync(key, CommandFlags.PreferReplica);
				if (value.HasValue)
					return (true, (byte[])value);

				return (false, null);
			}
			catch (Exception ex)
			{
				ex.Log(severity: Severity.Warning);
				return (false, null);
			}
		}

		/// <inheritdoc/>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Redis errors should not crash the application")]
		public async Task SetAsync<T>(T key, byte[] value, TimeSpan expiration) where T : IEquatable<T>
		{
			await SetAsync(key.SerializeToJson(), value, expiration);
		}

		/// <inheritdoc/>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Redis errors should not crash the application")]
		public async Task SetAsync(string key, byte[] value, TimeSpan expiration)
		{
			try
			{
				// Write operations always go to master (PreferMaster is more resilient than DemandMaster)
				// PreferMaster will use master if available, but won't fail if temporarily unavailable
				await _database.StringSetAsync(key, value, expiration, flags: CommandFlags.PreferMaster);
			}
			catch (Exception ex)
			{
				ex.Log(severity: Severity.Warning);
			}
		}

		/// <inheritdoc/>
		public Task<byte[]> GetOrCreateAsync<T>(T key, Func<Task<byte[]>> factory, TimeSpan expiration) where T : IEquatable<T>
			=> GetOrCreateAsync(key.SerializeToJson(), factory, expiration);

		/// <inheritdoc/>
		/// <remarks>
		/// Uses a distributed lock, so concurrent requests (across pods) for the same key create the value once.
		/// Requests that do not get the lock wait for the value to appear in the cache, up to <see cref="DefaultLockTimeout"/>,
		/// after which they create the value themselves.
		/// When Redis is unavailable, the value is created without caching.
		/// Exceptions thrown by <paramref name="factory"/> are not caught.
		/// </remarks>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Redis errors should not crash the application")]
		public async Task<byte[]> GetOrCreateAsync(string key, Func<Task<byte[]>> factory, TimeSpan expiration)
		{
			ArgumentNullException.ThrowIfNull(factory);

			var lockKey = $"lock:{key}";
			var lockValue = Guid.NewGuid().ToString();
			bool lockAcquired;

			try
			{
				var waitUntil = DateTime.UtcNow + DefaultLockTimeout;
				while (true)
				{
					// Prefer replica for read load distribution
					var (found, value) = await TryGetValueAsync(key);
					if (found)
						return value;

					// Acquire distributed lock (must go to master, but use PreferMaster for resilience)
					lockAcquired = await _database.LockTakeAsync(lockKey, lockValue, DefaultLockTimeout, CommandFlags.PreferMaster);
					if (lockAcquired || DateTime.UtcNow >= waitUntil)
						break;

					// Another request is creating the value, wait for it
					await Task.Delay(LockRetryDelay);
				}
			}
			catch (Exception ex)
			{
				// If Redis fails, still return the generated value
				ex.Log(severity: Severity.Warning);
				return await factory();
			}

			if (!lockAcquired)
				return await factory(); // Waited long enough, create without lock (fallback)

			try
			{
				// Double-check: the value may have been created between the cache read and taking the lock
				var (found, value) = await TryGetValueAsync(key);
				if (found)
					return value;

				value = await factory();

				// Cache it (write to master)
				await SetAsync(key, value, expiration);

				return value;
			}
			finally
			{
				await ReleaseLockAsync(lockKey, lockValue);
			}
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Redis errors should not crash the application, the lock expires anyway")]
		private async Task ReleaseLockAsync(string lockKey, string lockValue)
		{
			try
			{
				// Must go to master
				await _database.LockReleaseAsync(lockKey, lockValue, CommandFlags.PreferMaster);
			}
			catch (Exception ex)
			{
				ex.Log(severity: Severity.Warning);
			}
		}

		/// <inheritdoc/>
		public int Count() => -1; // Redis doesn't provide an efficient way to count keys

		/// <inheritdoc/>
		public long GetApproximateSize() => -1; // Redis manages memory independently
	}
}
