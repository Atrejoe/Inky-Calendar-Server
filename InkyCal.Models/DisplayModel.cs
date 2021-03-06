﻿using System.Diagnostics.CodeAnalysis;

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
		epd_7_in_5_v3_colour,
		/// <summary>
		/// 7.5" higher-res black-white-red/yellow
		/// </summary>
		epd_7_in_5_v3,
		/// <summary>
		/// 7.5" high-res black-white-red/yellow
		/// </summary>
		epd_7_in_5_v2_colour,
		/// <summary>
		/// 7.5" high-res black-white
		/// </summary>
		epd_7_in_5_v2,
		/// <summary>
		/// 7.5" black-white-red/yellow
		/// </summary>
		epd_7_in_5_colour,
		/// <summary>
		/// 7.5" black-white
		/// </summary>
		epd_7_in_5,
		/// <summary>
		/// 5.83" black-white-red/yellow
		/// </summary>
		epd_5_in_83_colour,
		/// <summary>
		/// 5.83" black-white
		/// </summary>
		epd_5_in_83,
		/// <summary>
		/// 4.2" black-white-red/yellow
		/// </summary>
		epd_4_in_2_colour,
		/// <summary>
		/// 4.2" black-white
		/// </summary>
		epd_4_in_2,// # 

		/// <summary>
		/// The 12.48" color
		/// </summary>
		epd_12_in_48_colour,

		/// <summary>
		/// The 12.48" color (with grayscale)
		/// </summary>
		epd_12_in_48_colour_with_grayscale
	}
}
