using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using Microsoft.EntityFrameworkCore;

namespace InkyCal.Data
{

	/// <summary>
	/// Storage for calendar configuration
	/// </summary>
	public static class CalenderRepository
	{

		/// <summary>
		/// Saves the subscribed calenders.
		/// </summary>
		/// <param name="panel">The panel.</param>
		/// <param name="calenders">The calenders.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">panel</exception>
		public static async Task<int> SaveSubscribedCalenders(this CalendarPanel panel, HashSet<(int IdAccessToken, string Calender)> calenders)
		{
			if (panel is null)
				throw new ArgumentNullException(nameof(panel));

			var set = await panel.SubscribedCalenders();

			using var c = new ApplicationDbContext();
			//Remove items from DB not present in selection
			foreach (var item in set.Where(x => !calenders.Contains((x.IdAccessToken, x.Calender))))
				c.Remove(item);

			//Add new items to DB
			foreach (var item in calenders.Where(x => !set.Any(y => y.Calender == x.Calender && y.Panel == panel.Id && y.IdAccessToken == x.IdAccessToken)))
				c.Add(new SubscribedGoogleCalender()
				{
					Panel = panel.Id,
					IdAccessToken = item.IdAccessToken,
					Calender = item.Calender
				});

			return await c.SaveChangesAsync();
		}


		/// <summary>
		/// Gets subscribed calendars
		/// </summary>
		/// <param name="panel">The panel.</param>
		/// <returns></returns>
		public static async Task<SubscribedGoogleCalender[]> SubscribedCalenders(this CalendarPanel panel)
		{
			using var c = new ApplicationDbContext();
			return await c.Set<SubscribedGoogleCalender>()
						.Where(x => x.Panel == panel.Id)
						.AsNoTracking()
						.ToArrayAsync();

		}
	}
}
