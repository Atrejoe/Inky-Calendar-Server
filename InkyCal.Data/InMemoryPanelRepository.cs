using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;

namespace InkyCal.Data
{
	/// <summary>
	/// In-memory implementation of panel repository for database-less operation
	/// </summary>
	public class InMemoryPanelRepository : IPanelRepository
	{
		private readonly ConcurrentDictionary<Guid, Panel> _panels = new ConcurrentDictionary<Guid, Panel>();

		/// <inheritdoc/>
		public Task<TPanel> GetPanel<TPanel>(Guid id, bool markAsAccessed = false) where TPanel : Panel
		{
			if (_panels.TryGetValue(id, out var panel) && panel is TPanel typedPanel)
			{
				if (markAsAccessed)
				{
					typedPanel.AccessCount++;
					typedPanel.Accessed = DateTime.UtcNow;
				}
				return Task.FromResult(typedPanel);
			}
			return Task.FromResult<TPanel>(null);
		}

		/// <inheritdoc/>
		public Task<TPanel> GetPanel<TPanel>(Guid id, User user) where TPanel : Panel
		{
			if (_panels.TryGetValue(id, out var panel) && panel is TPanel typedPanel && panel.Owner?.Id == user?.Id)
			{
				return Task.FromResult(typedPanel);
			}
			return Task.FromResult<TPanel>(null);
		}

		/// <inheritdoc/>
		public Task<TPanel[]> List<TPanel>(User user) where TPanel : Panel
		{
			var panels = _panels.Values
				.OfType<TPanel>()
				.Where(p => p.Owner?.Id == user?.Id)
				.ToArray();
			return Task.FromResult(panels);
		}

		/// <inheritdoc/>
		public Task<TPanel> Update<TPanel>(TPanel panel) where TPanel : Panel
		{
			ArgumentNullException.ThrowIfNull(panel);

			if (panel.Id == Guid.Empty)
			{
				panel.Id = Guid.NewGuid();
				panel.Created = DateTime.UtcNow;
			}

			panel.Modified = DateTime.UtcNow;
			_panels[panel.Id] = panel;

			return Task.FromResult(panel);
		}

		/// <inheritdoc/>
		public Task<bool> ToggleStar(Panel panel)
		{
			ArgumentNullException.ThrowIfNull(panel);

			if (_panels.TryGetValue(panel.Id, out var storedPanel))
			{
				storedPanel.Starred = !storedPanel.Starred;
				return Task.FromResult(storedPanel.Starred);
			}

			throw new InvalidOperationException($"Panel with ID {panel.Id} not found");
		}

		/// <inheritdoc/>
		public Task Delete(Guid id, int idOwner)
		{
			if (_panels.TryGetValue(id, out var panel) && panel.Owner?.Id == idOwner)
			{
				_panels.TryRemove(id, out _);
				return Task.CompletedTask;
			}

			throw new DalException("Not deleted");
		}

		/// <inheritdoc/>
		public Task<Panel[]> All()
		{
			return Task.FromResult(_panels.Values.ToArray());
		}
	}
}
