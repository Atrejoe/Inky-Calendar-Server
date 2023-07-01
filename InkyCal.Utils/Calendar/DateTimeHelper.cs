using System;

namespace InkyCal.Utils.Calendar
{
	/// <summary>
	/// 
	/// </summary>
	public static class DateTimeHelper	
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="timeZone"></param>
		/// <returns></returns>
		public static DateTimeOffset? ToSpecificTimeZone(this DateTimeOffset? source, TimeZoneInfo timeZone)
		{
			if (!source.HasValue)
				return null;

			return source.Value.ToSpecificTimeZone(timeZone);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="timeZone"></param>
		/// <returns></returns>
		public static DateTime? ToSpecificTimeZone(this DateTime? source, TimeZoneInfo timeZone)
		{
			if (!source.HasValue)
				return null;

			return source.Value.ToSpecificTimeZone(timeZone);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="timeZone"></param>
		/// <returns></returns>
		public static DateTimeOffset ToSpecificTimeZone(this DateTimeOffset source, TimeZoneInfo timeZone)
		{

			var offset = timeZone.GetUtcOffset(source);
			var newDt = source.Add(offset);
			return newDt;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="timeZone"></param>
		/// <returns></returns>
		public static DateTime ToSpecificTimeZone(this DateTime source, TimeZoneInfo timeZone)
		{

			var offset = timeZone.GetUtcOffset(source);
			var newDt = source.Add(offset);
			return newDt;
		}
	}
}
