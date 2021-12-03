using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using Microsoft.EntityFrameworkCore;

namespace InkyCal.Data
{
	public static class CalenderRepository
	{
		public static async Task<int> SaveSubscribedCalenders(this CalendarPanel panel, HashSet<(int IdAccessToken, string Calender)> calenders)
		{
			
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
