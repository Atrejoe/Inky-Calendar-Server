using System;
using System.Threading.Tasks;
using InkyCal.Models;

namespace InkyCal.Data
{
	/// <summary>
	/// Interface for panel repository operations
	/// </summary>
	public interface IPanelRepository
	{
		/// <summary>
		/// Gets the specified panel by identifier.
		/// </summary>
		/// <typeparam name="TPanel">The type of the panel.</typeparam>
		/// <param name="id">The identifier.</param>
		/// <param name="markAsAccessed">if set to <c>true</c> mark as accessed.</param>
		/// <returns></returns>
		Task<TPanel> GetPanel<TPanel>(Guid id, bool markAsAccessed = false) where TPanel : Panel;

		/// <summary>
		/// Gets the specified panel by identifier and user.
		/// </summary>
		/// <typeparam name="TPanel">The type of the panel.</typeparam>
		/// <param name="id">The identifier.</param>
		/// <param name="user">The user.</param>
		/// <returns></returns>
		Task<TPanel> GetPanel<TPanel>(Guid id, User user) where TPanel : Panel;

		/// <summary>
		/// Gets the panels the user owns.
		/// </summary>
		/// <typeparam name="TPanel">The type of the panel.</typeparam>
		/// <param name="user">The user.</param>
		/// <returns></returns>
		Task<TPanel[]> List<TPanel>(User user) where TPanel : Panel;

		/// <summary>
		/// Updates the specified panel.
		/// </summary>
		/// <typeparam name="TPanel">The type of the panel.</typeparam>
		/// <param name="panel">The panel.</param>
		/// <returns></returns>
		Task<TPanel> Update<TPanel>(TPanel panel) where TPanel : Panel;

		/// <summary>
		/// Toggles the star for a panel.
		/// </summary>
		/// <param name="panel">The panel.</param>
		/// <returns></returns>
		Task<bool> ToggleStar(Panel panel);

		/// <summary>
		/// Deletes the specified panel.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="idOwner">The identifier of the owner.</param>
		Task Delete(Guid id, int idOwner);

		/// <summary>
		/// Returns ALL panels.
		/// </summary>
		/// <returns></returns>
		Task<Panel[]> All();
	}
}
