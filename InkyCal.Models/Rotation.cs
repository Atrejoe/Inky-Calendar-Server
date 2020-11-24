using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace InkyCal.Models
{
	[SuppressMessage("Design", "CA1028:Enum Storage should be Int32", Justification = "Stored as short")]
	public enum Rotation : short
	{
		[Description("None")]
		None = 0,
		[Description("None")]
		Zero = None,

		[Description("Clockwise")]
		Clockwise = 90,
		[Description("Clockwise")]
		CW = Clockwise,

		[Description("Upside down")]
		UpsideDown = 180,

		[Description("Counter-clockwise")]
		CounterClockwise = 270,
		[Description("Counter-clockwise")]
		CCW = CounterClockwise
	}
}
