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
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Redis errors should not crash the application")]
		public async Task<byte[]> GetOrCreateAsync<T>(T key, Func<Task<byte[]>> factory, TimeSpan expiration) where T : IEquatable<T>
		{
			ArgumentNullException.ThrowIfNull(factory);

			var stringKey = key.SerializeToJson();
			var lockKey = $"lock:{stringKey}";
			var lockValue = Guid.NewGuid().ToString();

			try
			{
				// Try to get from cache first (prefer replica for read load distribution)
				var (found, value) = await TryGetValueAsync(stringKey);
				if (found)
					return value;

				// Acquire distributed lock (must go to master, but use PreferMaster for resilience)
				var lockAcquired = await _database.LockTakeAsync(lockKey, lockValue, DefaultLockTimeout, CommandFlags.PreferMaster);
				
				while (lockAcquired) {
					// Could not acquire lock, wait and retry getting from cache
					await Task.Delay(LockRetryDelay);

					lockAcquired = await _database.LockTakeAsync(lockKey, lockValue, DefaultLockTimeout, CommandFlags.PreferMaster);
				}
				
				if (lockAcquired)
				{
					try
					{
						// Double-check if value was created while waiting for lock
						// This read can come from replica
						(found, value) = await TryGetValueAsync(stringKey);
						if (found)
							return value;

						// Create the value
						value = await factory();

						// Cache it (write to master)
						await SetAsync(stringKey, value, expiration);

						return value;
					}
					finally
					{
						// Always release the lock (must go to master)
						await _database.LockReleaseAsync(lockKey, lockValue, CommandFlags.PreferMaster);
					}
				}
				else
				{
					
					(found, value) = await TryGetValueAsync(stringKey);
					if (found)
						return value;

					// If still not found, create without lock (fallback)
					return await factory();
				}
			}
			catch (Exception ex)
			{
				ex.Log(severity: Severity.Warning);
				// If Redis fails, still return the generated value
				return await factory();
			}
		}

		/// <inheritdoc/>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Redis errors should not crash the application")]
		public async Task<byte[]> GetOrCreateAsync(string key, Func<Task<byte[]>> factory, TimeSpan expiration)
		{
			ArgumentNullException.ThrowIfNull(factory);

			var lockKey = $"lock:{key}";
			var lockValue = Guid.NewGuid().ToString();

			try
			{
				// Try to get from cache first (prefer replica for read load distribution)
				var (found, value) = await TryGetValueAsync(key);
				if (found)
					return value;

				// Acquire distributed lock (must go to master, but use PreferMaster for resilience)
				var lockAcquired = await _database.LockTakeAsync(lockKey, lockValue, DefaultLockTimeout, CommandFlags.PreferMaster);
				
				if (lockAcquired)
				{
					try
					{
						// Double-check if value was created while waiting for lock
						// This read can come from replica
						(found, value) = await TryGetValueAsync(key);
						if (found)
							return value;

						// Create the value
						value = await factory();

						// Cache it (write to master)
						await SetAsync(key, value, expiration);

						return value;
					}
					finally
					{
						// Always release the lock (must go to master)
						await _database.LockReleaseAsync(lockKey, lockValue, CommandFlags.PreferMaster);
					}
				}
				else
				{
					// Could not acquire lock, wait and retry getting from cache
					await Task.Delay(LockRetryDelay);
					
					(found, value) = await TryGetValueAsync(key);
					if (found)
						return value;

					// If still not found, create without lock (fallback)
					return await factory();
				}
			}
			catch (Exception ex)
			{
				ex.Log(severity: Severity.Warning);
				// If Redis fails, still return the generated value
				return await factory();
			}
		}

		/// <inheritdoc/>
		public int Count() => -1; // Redis doesn't provide an efficient way to count keys

		/// <inheritdoc/>
		public long GetApproximateSize() => -1; // Redis manages memory independently
	}
}
