using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
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

		/// <summary>
		/// Initializes a new instance of the <see cref="RedisCacheService"/> class.
		/// </summary>
		/// <param name="connectionString">The Redis connection string.</param>
		public RedisCacheService(string connectionString)
		{
			ArgumentNullException.ThrowIfNull(connectionString);

			var redis = ConnectionMultiplexer.Connect(connectionString);
			_database = redis.GetDatabase();
		}

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
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Redis errors should not crash the application")]
		public async Task<(bool Found, byte[] Value)> TryGetValueAsync<T>(T key) where T : ISerializable, IEquatable<T>
		{
			return await TryGetValueAsync(key.ToString());
		}

		/// <inheritdoc/>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Redis errors should not crash the application")]
		public async Task<(bool Found, byte[] Value)> TryGetValueAsync(string key)
		{
			try
			{
				var value = await _database.StringGetAsync(key);
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
		public async Task SetAsync<T>(T key, byte[] value, TimeSpan expiration) where T : ISerializable, IEquatable<T>
		{
			await SetAsync(key.ToString(), value, expiration);
		}

		/// <inheritdoc/>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Redis errors should not crash the application")]
		public async Task SetAsync(string key, byte[] value, TimeSpan expiration)
		{
			try
			{
				await _database.StringSetAsync(key.ToString(), value, expiration);
			}
			catch (Exception ex)
			{
				ex.Log(severity: Severity.Warning);
			}
		}

		/// <inheritdoc/>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Redis errors should not crash the application")]
		public async Task<byte[]> GetOrCreateAsync<T>(T key, Func<Task<byte[]>> factory, TimeSpan expiration) where T : ISerializable, IEquatable<T>
		{
			ArgumentNullException.ThrowIfNull(factory);

			try
			{
				var (found, value) = await TryGetValueAsync(key);
				if (found)
					return value;

				// Create the value
				value = await factory();

				// Cache it
				await SetAsync(key, value, expiration);

				return value;
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
	}
}
