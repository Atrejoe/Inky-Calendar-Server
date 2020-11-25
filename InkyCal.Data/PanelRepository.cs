using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using Microsoft.EntityFrameworkCore;

namespace InkyCal.Data
{
	public static class PanelRepository
	{
		public static async Task<bool> ToggleStar(this Panel panel)
		{
			using (var c = new ApplicationDbContext())
			{
				var p = c.Set<Panel>().Single(x => x.Id == panel.Id);

				p.Starred = !p.Starred;

				c.Update(p);

				await c.SaveChangesAsync();

				return p.Starred;
			}
		}

		public static async Task<TPanel> Update<TPanel>(this TPanel panel) where TPanel : Panel
		{
			if (panel is null)
				throw new ArgumentNullException(nameof(panel));

			using (var c = new ApplicationDbContext())
			{
				//Prevent tracking of referenced existing items

				{
					//Should not modify, nor create a user
					c.Entry(panel.Owner).State = EntityState.Unchanged;

					//Should not modify, nor create a referenced panels
					if (panel is PanelOfPanels pp)
					{
						var ids = new HashSet<Guid>();
						//foreach (var referencedPanel in pp.Panels)
						//{
						//	c.Entry(referencedPanel).State = EntityState.Unchanged;
						//}
						foreach (var referencedPanel in pp.Panels.Select(x => x.Panel))
						{
							if (!ids.Contains(referencedPanel.Id))
							{
								c.Entry(referencedPanel).State = EntityState.Unchanged;
								ids.Add(referencedPanel.Id);
							}
						}
					}
				}

				//When updating
				if (panel.Id != default)
				{
					if (panel is PanelOfPanels pp)
					{
						//
						var comparison = new Func<SubPanel, SubPanel, bool>((x, y) =>
						{
							return x.IdPanel.Equals(y.IdPanel)
								&& x.IdParent.Equals(y.IdParent)
								&& x.SortIndex.Equals(y.SortIndex);
						});

						//Ensure sub-panels are to be updated
						var existingLinks = await c.Set<SubPanel>()
													.Where(x =>
														x.IdParent == panel.Id).ToListAsync();

						//Remove existing links which are not present in posted data
						c.RemoveRange(
							existingLinks.Where(
								x => !pp.Panels.ToList().Any(y => comparison(x, y))));

						//Update existing links
						foreach (var linkedPanel in existingLinks.Where(x => pp.Panels.Any(y => comparison(x, y))))
							linkedPanel.Ratio = pp.Panels.Single(x => comparison(x, linkedPanel)).Ratio;

						//Add new links
						foreach (var linkedPanel in pp.Panels.Where(x => !existingLinks.Any(y => comparison(x, y))))
							await c.AddAsync(new SubPanel()
							{
								SortIndex = linkedPanel.SortIndex,
								IdParent = pp.Id,
								IdPanel = linkedPanel.IdPanel,
								Ratio = linkedPanel.Ratio
							});


						pp.Panels.Clear();
					}
					else if (panel is CalendarPanel cp)
					{
						//Remove existing calendar links
						var existingLinks = await c.Set<CalendarPanelUrl>()
							.AsNoTracking()
							.Where(x => x.IdPanel == panel.Id).ToListAsync();

						var entities = existingLinks.Where(x => !cp.CalenderUrls.ToList().Any(y => y.Url == x.Url));

						c.RemoveRange(entities);
					}
				}

				c.Update(panel);

				await c.SaveChangesAsync();
			}
			//This should have updated the id
			return panel;
		}

		public static async Task<TPanel[]> List<TPanel>(User user) where TPanel : Panel
		{
			using var c = new ApplicationDbContext();
			var queryable = c.Set<TPanel>()
							.Include(x => (x as CalendarPanel).CalenderUrls)
							.Include(x => (x as PanelOfPanels).Panels)
							.Where(x => x.Owner.Id.Equals(user.Id))
							.AsNoTracking();

			//queryable.OfType<PanelOfPanels>().Include(x => x.Panels).AsNoTracking();
			//queryable.OfType<CalendarPanel>().Include(x => x.CalenderUrls);

			return await queryable.ToArrayAsync();
		}

		public static async Task<TPanel> Get<TPanel>(Guid id, User user) where TPanel : Panel
		{
			using var c = new ApplicationDbContext();

			var result = await c.Set<TPanel>()
								.EagerLoad()
								.AsNoTracking()
								.SingleOrDefaultAsync(x =>
								   x.Id == id
								&& x.Owner.Id.Equals(user.Id));

			return result;
		}

		public static async Task Delete(Guid id)
		{
			using var c = new ApplicationDbContext();

			//var panel = await Get<Panel>(id, user);
			c.Set<Panel>().RemoveRange(c.Set<Panel>().Where(x => x.Id == id));
			await c.SaveChangesAsync();
		}

		public static async Task<TPanel> Get<TPanel>(Guid id) where TPanel : Panel
		{
			using var c = new ApplicationDbContext();

			var result = await c.Set<TPanel>()
								.EagerLoad()
								.AsNoTracking()
								.SingleOrDefaultAsync(x => x.Id == id);

			return result;
		}

		public static IQueryable<TPanel> EagerLoad<TPanel>(this DbSet<TPanel> set) where TPanel : class
		{
			return set
					.Include(x => (x as CalendarPanel).CalenderUrls)
					.Include(x => (x as PanelOfPanels).Panels)
						.ThenInclude(x => x.Panel)
						.ThenInclude(x => (x as CalendarPanel).CalenderUrls);

		}
	}
}
