using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using Microsoft.EntityFrameworkCore;

namespace InkyCal.Data
{
	/// <summary>
	/// Database-backed implementation of panel repository
	/// </summary>
	public class DatabasePanelRepository : IPanelRepository
	{
		/// <inheritdoc/>
		public async Task<TPanel> GetPanel<TPanel>(Guid id, bool markAsAccessed = false) where TPanel : Panel
		{
			return await PanelRepository.Get<TPanel>(id, markAsAccessed);
		}

		/// <inheritdoc/>
		public async Task<TPanel> GetPanel<TPanel>(Guid id, User user) where TPanel : Panel
		{
			return await PanelRepository.Get<TPanel>(id, user);
		}

		/// <inheritdoc/>
		public async Task<TPanel[]> List<TPanel>(User user) where TPanel : Panel
		{
			return await PanelRepository.List<TPanel>(user);
		}

		/// <inheritdoc/>
		public async Task<TPanel> Update<TPanel>(TPanel panel) where TPanel : Panel
		{
			return await PanelRepository.Update(panel);
		}

		/// <inheritdoc/>
		public async Task<bool> ToggleStar(Panel panel)
		{
			return await PanelRepository.ToggleStar(panel);
		}

		/// <inheritdoc/>
		public async Task Delete(Guid id, int idOwner)
		{
			await PanelRepository.Delete(id, idOwner);
		}

		/// <inheritdoc/>
		public async Task<Panel[]> All()
		{
			return await PanelRepository.All();
		}
	}
}
