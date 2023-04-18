using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace InkyCal.Models
{
	/// <summary>
	/// All supported E-Ink panels
	/// </summary>
	[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Panel types are easier to read with underscores")]
	public enum DisplayModel
	{
		/// <summary>
		/// 7.5" higher-res black-white-red/yellow
		/// </summary>
		[DisplayResolution(880, 528, KnownColor.Red)]
		epd_7_in_5_v3_colour,
		/// <summary>
		/// 7.5" higher-res black-white-red/yellow
		/// </summary>
		[DisplayResolution(880, 528)]
		epd_7_in_5_v3,
		/// <summary>
		/// 7.5" high-res black-white-red/yellow
		/// </summary>
		[DisplayResolution(880, 480, KnownColor.Red)]
		epd_7_in_5_v2_colour,
		/// <summary>
		/// 7.5" high-res black-white
		/// </summary>
		[DisplayResolution(880, 480)]
		epd_7_in_5_v2,
		/// <summary>
		/// 7.5" black-white-red/yellow
		/// </summary>
		[DisplayResolution(640, 384, KnownColor.Red)]
		epd_7_in_5_colour,
		/// <summary>
		/// 7.5" black-white
		/// </summary>
		[DisplayResolution(640, 384)]
		epd_7_in_5,
		/// <summary>
		/// 5.83" black-white-red/yellow
		/// </summary>
		[DisplayResolution(600, 448, KnownColor.Red)]
		epd_5_in_83_colour,
		/// <summary>
		/// 5.83" black-white
		/// </summary>
		[DisplayResolution(600, 448)]
		epd_5_in_83,
		/// <summary>
		/// 4.2" black-white-red/yellow
		/// </summary>
		[DisplayResolution(400, 300, KnownColor.Red)]
		epd_4_in_2_colour,
		/// <summary>
		/// 4.2" black-white
		/// </summary>
		[DisplayResolution(400, 300, KnownColor.Red)]
		epd_4_in_2,

		/// <summary>
		/// The 12.48" color
		/// </summary>
		[DisplayResolution(1304, 984, KnownColor.Red)]
		epd_12_in_48_colour,

		/// <summary>
		/// The 12.48" color (with grayscale)
		/// </summary>
		[DisplayResolution(1304, 984, 3, KnownColor.Red)]
		epd_12_in_48_colour_with_grayscale,

		/// <summary>
		/// The 10.3" 16 gray scale, no colour. Identical specs as <see cref="epd_7_8_in_16_grayscale"/>.
		/// </summary>
		/// <remarks>Like <a href="https://www.waveshare.com/10.3inch_e-Paper_HAT">Waveshare &gt; 10.3inch e-Paper HAT</a></remarks>
		[DisplayResolution(1872, 1404, 16)]
		epd_10_3_in_16_grayscale,

		/// <summary>
		/// The 7.8" 16 gray scale, no colour. Identical specs as <see cref="epd_10_3_in_16_grayscale"/>.
		/// </summary>
		/// <remarks>Like <a href="https://www.waveshare.com/7.8inch-e-paper.htm">Waveshare &gt; 7.8inch e-Paper HAT</a></remarks>
		[DisplayResolution(1872, 1404, 16)]
		epd_7_8_in_16_grayscale,

		/// <summary>
		/// The 9.7" 16 gray scale, no colour
		/// </summary>
		/// <remarks>Like <a href="https://www.waveshare.com/9.7inch-e-paper.htm">Waveshare &gt; 9.7inch e-Paper HAT</a></remarks>
		[DisplayResolution(1200, 825, 16)]
		epd_9_7_in_16_grayscale,
	}
}
