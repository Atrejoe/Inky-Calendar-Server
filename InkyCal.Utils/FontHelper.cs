using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using SixLabors.Fonts;

namespace InkyCal.Utils
{
	/// <summary>
	/// A helper class for font-sizing
	/// </summary>
	public static class FontHelper
	{
		/// <summary>
		/// This is a lame solution for argument out-of-range exception while rending text with non-existing characters
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="font">The font.</param>
		/// <returns></returns>
		/// <seealso cref="ToSafeChars(string, FontFamily)"/>
		internal static string ToSafeChars(this string text, Font font) {
			return text.ToSafeChars(font.Family);
		}

		/// <summary>
		/// This is a lame solution for argument out-of-range exception while rending text with non-existing characters
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="fontFamily">The font.</param>
		/// <returns></returns>
		/// <seealso cref="ToSafeChars(string, Font)"/>
		/// <remarks><see cref="MonteCarlo"/> cannot render some characters</remarks>
		internal static string ToSafeChars(this string text, FontFamily fontFamily)
		{
			if (!fontFamily.Equals(MonteCarlo))
				return text;

			// strip diacritics
			// from https://stackoverflow.com/a/249126
			var normalizedString = text.Normalize(NormalizationForm.FormD);
			var stringBuilder = new StringBuilder();

			foreach (var c in normalizedString)
			{
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
				{
					stringBuilder.Append(c);
				}
			}
			var result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

			//Replace some known remaining enties
			result = result
				.Replace("ẞ", "sz")
				.Replace("ß", "Sz");

			//Bluntly convert to ASCII, leaving questionmarks ofr unknow items
			var tempBytes = Encoding.ASCII.GetBytes(result);
			result = Encoding.ASCII.GetString(tempBytes);

			Trace.WriteLine($"{text} => {result}");

			return result;
		}

		private static readonly FontCollection fonts = new FontCollection();

		/// <summary>
		/// Font family NotoSans-SemiCondensed
		/// </summary>
		public static readonly FontFamily NotoSans;
		/// <summary>
		/// Font family MonteCarloFixed12
		/// </summary>
		/// <remarks>
		/// Obtained from <a href="https://www.bok.net/MonteCarlo/">https://www.bok.net/MonteCarlo/</a>
		/// </remarks>
		public static readonly FontFamily MonteCarlo;

		/// <summary>
		/// Font family ProFontWindowsPL - multi-size, non-anti-aliassed font
		/// </summary>
		/// <value>
		/// The pro font.
		/// </value>
		/// <remarks>Obtained from <a href="https://tobiasjung.name/profont/">https://tobiasjung.name/profont/</a></remarks>
		public static readonly FontFamily ProFont;

		/// <summary>
		/// Font for weather icons
		/// </summary>
		public static readonly FontFamily WeatherIcons;

		/// <summary>
		/// A mapping for mapping OpenWeather weather status codes to glyphs in <see cref="WeatherIcons"/>
		/// </summary>
		public static readonly IReadOnlyDictionary<string, string> WeatherIconsMap = new Dictionary<string, string> {
			{"01d","\uf00d"},{"02d","\uf002"},{"03d","\uf013"},{"04d","\uf012"},{"09d","\uf01a"},{"10d","\uf019"},
			{"11d","\uf01e"},{"13d","\uf01b"},{"50d","\uf014"},{"01n","\uf02e"},{"02n","\uf013"},{"03n","\uf013"},
			{"04n","\uf013"},{"09n","\uf037"},{"10n","\uf036"},{"11n","\uf03b"},{"13n","\uf038"},{"50n","\uf023"}
		};


		[SuppressMessage("Design", "CA1810:Initialize reference type static fields inline", Justification = "Easier to read initialization")]
		static FontHelper()
		{
			var assembly = typeof(CalendarPanelRenderer).GetTypeInfo().Assembly;

			using (var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.fonts.NotoSans-SemiCondensed.ttf"))
				NotoSans = fonts.Install(resource);

			using (var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.fonts.MonteCarloFixed12.ttf"))
				MonteCarlo = fonts.Install(resource);

			using (var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.fonts.weathericons-regular-webfont.ttf"))
				WeatherIcons = fonts.Install(resource);

			using (var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.fonts.ProFontWindows.ttf"))
				ProFont = fonts.Install(resource);
		}

		/// <summary>
		/// Return the width of characters in a fixed-width character font.
		/// </summary>
		/// <param name="font"></param>
		/// <returns></returns>
		public static ushort? GetCharacterWidth(this Font font)
		{
			if (font is null)
				throw new System.ArgumentNullException(nameof(font));


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
