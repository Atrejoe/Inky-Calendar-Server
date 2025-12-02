using System;
using System.Text.Json.Serialization;

namespace InkyCal.Models
{
	/// <summary>
	/// A cache keys for panels that need to be individually cached
	/// </summary>
	/// <seealso cref="PanelCacheKey" />
	public class PanelInstanceCacheKey : PanelCacheKey
	{
		/// <summary>
		/// The default expiration in seconds
		/// </summary>
		public const int DefaultExpirationInSeconds = 30;

		/// <summary>
		/// Initializes a new instance of <see cref="PanelInstanceCacheKey"/> for JSON deserialization.
		/// </summary>
		/// <param name="guid">The unique identifier.</param>
		[JsonConstructor]
		public PanelInstanceCacheKey(Guid guid)
			: this(guid, TimeSpan.FromSeconds(DefaultExpirationInSeconds)) { }

		/// <summary>
		/// Initializes a new instance of <see cref="PanelInstanceCacheKey"/>.
		/// </summary>
		/// <param name="guid"></param>
		/// <param name="expiration"></param>
		public PanelInstanceCacheKey(Guid guid, TimeSpan? expiration) : base(expiration.GetValueOrDefault(TimeSpan.FromSeconds(DefaultExpirationInSeconds)))
		{
			this.Guid = guid;
		}

		/// <summary>
		/// Gets the unique identifier for this panel
		/// </summary>
		/// <value>
		/// The unique identifier.
		/// </value>
		public Guid Guid { get; init; }

		/// <inhgeritdoc/>
		public override int GetHashCode()
			=> HashCode.Combine(Guid, base.GetHashCode());

		/// <inhgeritdoc/>
		public override bool Equals(object obj)
			=> Equals(obj as PanelCacheKey);

		/// <inhgeritdoc/>
		protected override bool Equals(PanelCacheKey other)
			=> other is PanelInstanceCacheKey pic
				&& pic.Guid.Equals(Guid);
	}
}
