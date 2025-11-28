using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace InkyCal.Utils.Caching
{
	/// <summary>
	/// Redis string cache implementation
	/// </summary>
	public class RedisStringCacheService : IStringCacheService
	{
		private readonly IDatabase _database;

		/// <summary>
		/// Initializes a new instance of the <see cref="RedisStringCacheService"/> class.
		/// </summary>
		/// <param name="connectionString">The Redis connection string.</param>
		public RedisStringCacheService(string connectionString)
		{
			ArgumentNullException.ThrowIfNull(connectionString);

			var redis = ConnectionMultiplexer.Connect(connectionString);
			_database = redis.GetDatabase();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RedisStringCacheService"/> class.
		/// </summary>
		/// <param name="redis">The Redis connection multiplexer.</param>
		public RedisStringCacheService(IConnectionMultiplexer redis)
		{
			ArgumentNullException.ThrowIfNull(redis);
			_database = redis.GetDatabase();
		}

		/// <inheritdoc/>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Redis errors should not crash the application")]
		public async Task<(bool Found, string Value)> TryGetValueAsync(string key)
		{
			try
			{
				var value = await _database.StringGetAsync(key);
				if (value.HasValue)
					return (true, value.ToString());

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
		public async Task SetAsync(string key, string value, TimeSpan expiration)
		{
			try
			{
				await _database.StringSetAsync(key, value, expiration);
			}
			catch (Exception ex)
			{
				ex.Log(severity: Severity.Warning);
			}
		}

		/// <inheritdoc/>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Redis errors should not crash the application")]
		public async Task<string> GetOrCreateAsync(string key, Func<Task<string>> factory, TimeSpan expiration)
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
