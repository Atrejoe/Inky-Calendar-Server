using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace InkyCal.Utils
{
	/// <summary>
	/// A helper class for font-sizing
	/// </summary>
	public static class FontHelper
	{
		private static readonly FontCollection fonts = new FontCollection();

		/// <summary>
		/// Font family NotoSans-SemiCondensed
		/// </summary>
		public static readonly FontFamily NotoSans;
		/// <summary>
		/// Font family MonteCarloFixed12
		/// </summary>
		public static readonly FontFamily MonteCarlo;

		/// <summary>
		/// Font for weather icons
		/// </summary>
		public static readonly FontFamily WeatherIcons;

		/// <summary>
		/// A mapping for mapping OpenWeather weather status codes to glyphs in <see cref="WeatherIcons"/>
		/// </summary>
		public static IReadOnlyDictionary<string, string> WeatherIconsMap = new Dictionary<string, string> {
			{"01d","\uf00d"},{"02d","\uf002"},{"03d","\uf013"},{"04d","\uf012"},{"09d","\uf01a"},{"10d","\uf019"},
			{"11d","\uf01e"},{"13d","\uf01b"},{"50d","\uf014"},{"01n","\uf02e"},{"02n","\uf013"},{"03n","\uf013"},
			{"04n","\uf013"},{"09n","\uf037"},{"10n","\uf036"},{"11n","\uf03b"},{"13n","\uf038"},{"50n","\uf023"}
		};

		static FontHelper()
		{
			var assembly = typeof(CalendarPanelRenderer).GetTypeInfo().Assembly;

			using (var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.fonts.NotoSans-SemiCondensed.ttf"))
				NotoSans = fonts.Install(resource);

			using (var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.fonts.MonteCarloFixed12.ttf"))
				MonteCarlo = fonts.Install(resource);

			using (var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.fonts.weathericons-regular-webfont.ttf"))
				WeatherIcons = fonts.Install(resource);
		}

		/// <summary>
		/// Return the width of characters in a fixed-width character font.
		/// </summary>
		/// <param name="font"></param>
		/// <returns></returns>
		public static ushort? GetCharacterWidth(this Font font)
		{
			if (font.Family.Equals(NotoSans))
				return null;
			if (font.Family.Equals(MonteCarlo))
				switch (font.Size)
				{
					case 12:
						return 7;
					case 24:
						return 7;
					default:
						return null;
				}
			else
				return null;
		}
	}
}
