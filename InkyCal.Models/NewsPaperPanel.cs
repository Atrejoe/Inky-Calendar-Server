namespace InkyCal.Models
{
	/// <summary>
	/// A panel that display the cover page of a newspaper
	/// </summary>
	/// <seealso cref="CurrentDatePanel" />
	public class NewsPaperPanel : CurrentDatePanel
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NewsPaperPanel"/> class.
		/// </summary>
		public NewsPaperPanel() { }

		/// <summary>
		/// Refers to the identifier of a newspaper
		/// </summary>
		/// <remarks>Depending on the newspaper source, this need to be mapped to a generic identifier. For now this refers to FreedomForum newspaper ids</remarks>
		public string NewsPaperId { get; set; }

	}
}
