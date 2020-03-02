using System.ComponentModel;

namespace InkyCal.Models
{
	public enum Rotation : short
	{
		[Description("None")]
		None = 0,
		Zero = None,

		[Description("Clockwise")]
		Clockwise = 90,
		CW = Clockwise,

		[Description("Upside down")]
		UpsideDown = 180,

		[Description("Counter-clockwise")]
		CounterClockwise = 270,
		CCW = CounterClockwise
	}
}
