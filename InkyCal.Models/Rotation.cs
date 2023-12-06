using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace InkyCal.Models
{
	/// <summary>
	/// 
	/// </summary>
	[SuppressMessage("Design", "CA1028:Enum Storage should be Int32", Justification = "Stored as short")]
	public enum Rotation : short
	{
		/// <summary>
		/// 
		/// </summary>
		[Description("None")]
		None = 0,
		/// <summary>
		/// 
		/// </summary>
		[Description("None")]
		Zero = None,

		/// <summary>
		/// 
		/// </summary>
		[Description("Clockwise")]
		Clockwise = 90,
		/// <summary>
		/// 
		/// </summary>
		[Description("Clockwise")]
		CW = Clockwise,

		/// <summary>
		/// 
		/// </summary>
		[Description("Upside down")]
		UpsideDown = 180,

		/// <summary>
		/// 
		/// </summary>
		[Description("Counter-clockwise")]
		CounterClockwise = 270,

		/// <summary>
		/// 
		/// </summary>
		[Description("Counter-clockwise")]
		CCW = CounterClockwise
	}
}
