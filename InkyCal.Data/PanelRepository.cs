﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using Microsoft.EntityFrameworkCore;

namespace InkyCal.Data
{


	/// <summary>
	/// A repo for pane configuration
	/// </summary>
	public static class PanelRepository
	{

		/// <summary>
		/// Toggles the star for a panel
		/// </summary>
		/// <param name="panel">The panel.</param>
		/// <returns></returns>
		public static async Task<bool> ToggleStar(this Panel panel)
		{
			using (var c = new ApplicationDbContext())
			{
				var p = await c.Set<Panel>().SingleAsync(x => x.Id == panel.Id);

				p.Starred = !p.Starred;

				c.Update(p);

				await c.SaveChangesAsync();

				return p.Starred;
			}
		}


		/// <summary>
		/// Updates the specified panel.
		/// </summary>
		/// <typeparam name="TPanel">The type of the panel.</typeparam>
		/// <param name="panel">The panel.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">panel</exception>
		public static async Task<TPanel> Update<TPanel>(this TPanel panel) where TPanel : Panel
		{
			ArgumentNullException.ThrowIfNull(panel);

			using (var c = new ApplicationDbContext())
			{
				//Should not modify, nor create a user
				c.Entry(panel.Owner).State = EntityState.Unchanged;

				//Should not modify, nor create a referenced panels
				if (panel is PanelOfPanels pop)
				{
					var ids = new HashSet<Guid>();
					foreach (var referencedPanel in pop.Panels.Select(x => x.Panel))
					{
						if (!ids.Contains(referencedPanel.Id))
						{
							c.Entry(referencedPanel).State = EntityState.Unchanged;
							ids.Add(referencedPanel.Id);
						}
					}
				}

				//When updating
				if (panel.Id != Guid.Empty)
				{
					if (panel is PanelOfPanels pp)
					{
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
								x => !pp.Panels.Any(y => comparison(x, y))));

						//Update existing links
						foreach (var linkedPanel in existingLinks.Where(x => pp.Panels.Any(y => comparison(x, y))))
							linkedPanel.Ratio = pp.Panels.Single(x => comparison(x, linkedPanel)).Ratio;

						//Add new links
						foreach (var linkedPanel in pp.Panels.Where(x => !existingLinks.Exists(y => comparison(x, y))))
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

						var entities = existingLinks.Where(x => !cp.CalenderUrls.Any(y => y.Url == x.Url));

						c.RemoveRange(entities);
					}
				}

				c.Update(panel);

				await c.SaveChangesAsync();
			}
			//This should have updated the id
			return panel;
		}


		/// <summary>
		/// Gets the panels <paramref name="user"/> owns.
		/// </summary>
		/// <typeparam name="TPanel">The type of the panel.</typeparam>
		/// <param name="user">The user.</param>
		/// <returns></returns>
		public static async Task<TPanel[]> List<TPanel>(User user) where TPanel : Panel
		{
			using var c = new ApplicationDbContext();
			var queryable = c.Set<TPanel>()
							.Include(x => (x as CalendarPanel).CalenderUrls)
							.Include(x => (x as CalendarPanel).SubscribedGoogleCalenders)
							.Include(x => (x as PanelOfPanels).Panels)
							.Where(x => x.Owner.Id.Equals(user.Id))
							.AsNoTracking();

			return await queryable.ToArrayAsync();
		}


		/// <summary>
		/// Gets the specified <see cref="Panel"/> by <see cref="Panel.Id"/> and <see cref="Panel.Owner"/>.
		/// </summary>
		/// <typeparam name="TPanel">The type of the panel.</typeparam>
		/// <param name="id">The identifier.</param>
		/// <param name="user">The user.</param>
		/// <returns></returns>
		public static async Task<TPanel> Get<TPanel>(Guid id, User user) where TPanel : Panel
		{
			using var c = new ApplicationDbContext();

			var result = await c.Set<TPanel>()
								.EagerLoad()
								.AsNoTracking()
								.SingleOrDefaultAsync(x =>
								   x.Id == id
								&& x.Owner.Id.Equals(user.Id));

			await result.AddMissingUntrackedEntities();

			return result;
		}

		/// <summary>
		/// Gets the specified identifier.
		/// </summary>
		/// <typeparam name="TPanel">The type of the panel.</typeparam>
		/// <param name="id">The identifier.</param>
		/// <param name="markAsAccessed">if set to <c>true</c> mark as accessed (<see cref="Panel.AccessCount"/> and <see cref="Panel.Accessed"/>).</param>
		/// <returns></returns>
		public static async Task<TPanel> Get<TPanel>(Guid id, bool markAsAccessed = false) where TPanel : Panel
		{
			using var c = new ApplicationDbContext();

			var result = await c.Set<TPanel>()
								.EagerLoad()
								.AsNoTracking()
								.SingleOrDefaultAsync(x => x.Id == id);

			await result.AddMissingUntrackedEntities();

			if (result != null && markAsAccessed)
			{
				c.Entry(result).Property(x => x.Accessed).CurrentValue = DateTime.UtcNow;
				c.Entry(result).Property(x => x.AccessCount).CurrentValue += 1;

				c.Entry(result).Property(x => x.Accessed).IsModified = true;
				c.Entry(result).Property(x => x.AccessCount).IsModified = true;

				result.SkipModificationTimestamp = true;

				await c.SaveChangesAsync();

				result.SkipModificationTimestamp = false;
			}

			return result;
		}

		private static async Task AddMissingUntrackedEntities<TPanel>(this TPanel result) where TPanel : Panel
		{
			if (result is CalendarPanel cp
				 && cp.SubscribedGoogleCalenders.Any(x => x.AccessToken is null))
			{

				var tokens = await new GoogleOAuthRepository().GetTokens(result.Owner.Id);

				foreach (var gcal in cp.SubscribedGoogleCalenders)
					gcal.AccessToken ??= tokens.SingleOrDefault(x => x.Id == gcal.IdAccessToken);
			}
		}

		/// <summary>
		/// Gets a random <see cref="Panel"/>.
		/// </summary>
		/// <typeparam name="TPanel">The type of the panel.</typeparam>
		/// <returns></returns>
		internal static async Task<TPanel> GetRandom<TPanel>() where TPanel : Panel
		{
			using var c = new ApplicationDbContext();

			var result = await c.Set<TPanel>()
								.EagerLoad()
								.AsNoTracking()
								.OrderBy(x => Guid.NewGuid())
								.FirstOrDefaultAsync();

			await result.AddMissingUntrackedEntities();

			return result;
		}

		/// <summary>
		/// Gets a random <see cref="CalendarPanel"/>.
		/// </summary>
		/// <returns></returns>
		internal static async Task<CalendarPanel> GetRandomGoogleCalendarPanel()
		{
			using var c = new ApplicationDbContext();

			var result = await c.Set<CalendarPanel>()
								.EagerLoad()
								.AsNoTracking()
								.Where(x => x.SubscribedGoogleCalenders.Count != 0)
								.OrderByDescending(x => x.SubscribedGoogleCalenders.Count)
								.FirstOrDefaultAsync();

			await result.AddMissingUntrackedEntities();

			return result;
		}


		/// <summary>
		/// Deletes the specified <see cref="Panel"/> by <see cref="Panel.Id"/> (<paramref name="id"/>).
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="idOwner">The identifier of the autheticated user</param>
		/// <exception cref="DalException">Not deleted</exception>
		public static async Task Delete(Guid id, int idOwner)
		{
			using var c = new ApplicationDbContext();

			c.Set<Panel>().RemoveRange(c.Set<Panel>().Where(x => x.Id == id && x.Owner.Id == idOwner));
			if ((await c.SaveChangesAsync()) != 1)
				throw new DalException("Not deleted");
		}

		/// <summary>
		/// Returns ALL panels (regardless of <see cref="Panel.Owner"/>).
		/// </summary>
		/// <returns></returns>
		public static async Task<Panel[]> All()
		{
			using var c = new ApplicationDbContext();

			var result = await c.Set<Panel>()
								.EagerLoad()
								.AsNoTracking()
								.ToArrayAsync();

			return result;
		}

		internal static IQueryable<TPanel> EagerLoad<TPanel>(this DbSet<TPanel> set) where TPanel : class
		{
			IQueryable<TPanel> x = set
					.Include(x => (x as Panel).Owner);

			if (typeof(TPanel).IsAssignableFrom(typeof(CalendarPanel)))
			{
				x = x.Include(x => (x as CalendarPanel).CalenderUrls)
						.Include(x => (x as CalendarPanel).SubscribedGoogleCalenders).AsNoTracking();
			}

			if (typeof(TPanel).IsAssignableFrom(typeof(PanelOfPanels)))
			{
				x = x.Include(x => (x as PanelOfPanels).Panels)
					.ThenInclude(x => x.Panel)
						.ThenInclude(x => x.Owner).AsNoTracking();

				x = x
						.Include(x => (x as PanelOfPanels).Panels)
							.ThenInclude(x => (x.Panel as CalendarPanel).CalenderUrls).AsNoTracking()
						.Include(x => (x as PanelOfPanels).Panels)
							.ThenInclude(x => (x.Panel as CalendarPanel).SubscribedGoogleCalenders)
								.ThenInclude(x => x.AccessToken).AsNoTracking();
			}

			return x;
		}
	}
}
