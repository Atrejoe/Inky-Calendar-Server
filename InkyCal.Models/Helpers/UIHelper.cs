using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace InkyCal.Models.Helpers
{

	/// <summary>
	/// A helper class for UI related tasks
	/// </summary>
	public static class UIHelper
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetDisplayName<T>(this T value) where T : Enum
		{
			// Read the Display attribute name
			var member = typeof(T).GetMember(value.ToString())[0];
			var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();
			if (displayAttribute != null)
				return displayAttribute.GetName();

			// Require the NuGet package Humanizer.Core
			// <PackageReference Include = "Humanizer.Core" Version = "2.8.26" />
			return value.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetDescription<T>(this T value) where T : Enum
		{
			// Read the Display attribute name
			var member = typeof(T).GetMember(value.ToString())[0];
			var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();
			if (displayAttribute != null)
				return displayAttribute.GetDescription();

			// Require the NuGet package Humanizer.Core
			// <PackageReference Include = "Humanizer.Core" Version = "2.8.26" />
			return string.Empty;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetShortName<T>(this T value) where T : Enum
		{
			// Read the Display attribute name
			var member = typeof(T).GetMember(value.ToString())[0];
			var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();
			if (displayAttribute != null)
				return displayAttribute.GetShortName();

			// Require the NuGet package Humanizer.Core
			// <PackageReference Include = "Humanizer.Core" Version = "2.8.26" />
			return string.Empty;
		}
	}
}
