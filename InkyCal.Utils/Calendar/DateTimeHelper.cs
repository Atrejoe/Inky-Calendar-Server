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
		/// <param name="fromTimeZone"></param>
		/// <param name="targetTimeZone"></param>
		/// <returns></returns>
		public static DateTime? ToSpecificTimeZone(this DateTime? source, TimeZoneInfo fromTimeZone, TimeZoneInfo targetTimeZone)
		{
			if (!source.HasValue)
				return null;

			return source.Value.ToSpecificTimeZone(fromTimeZone, targetTimeZone);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="fromTimeZone"></param>
		/// <param name="targetTimeZone"></param>
		/// <returns></returns>
		public static DateTime ToSpecificTimeZone(this DateTime source, TimeZoneInfo fromTimeZone, TimeZoneInfo targetTimeZone)
		{
			var originalOffSet = fromTimeZone.GetUtcOffset(source);
			var offset = targetTimeZone.GetUtcOffset(source);
			var newDt = source.Add(offset - originalOffSet);
			return newDt;
		}
	}
}
