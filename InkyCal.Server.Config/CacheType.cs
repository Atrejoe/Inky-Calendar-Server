namespace InkyCal.Server.Config
{
	/// <summary>
	/// Specifies the type of cache to use
	/// </summary>
	public enum CacheType
	{
		/// <summary>
		/// In-memory cache using Microsoft.Extensions.Caching.Memory
		/// </summary>
		Memory = 0,

		/// <summary>
		/// Distributed cache using Redis
		/// </summary>
		Redis = 1
	}
}
