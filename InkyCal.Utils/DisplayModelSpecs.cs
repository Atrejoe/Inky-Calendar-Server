using System;
using System.Collections.ObjectModel;
using System.Linq;
using InkyCal.Models;
using SixLabors.ImageSharp;

namespace InkyCal.Utils
{
	/// <summary>
	/// Specs for a <see cref="DisplayModel"/>
	/// </summary>
	public struct DisplayModelSpecs : System.IEquatable<DisplayModelSpecs>
	{
		/// <summary>
		/// Gets or sets the width.
		/// </summary>
		/// <value>
		/// The width.
		/// </value>
		public int Width { get; set; }
		/// <summary>
		/// Gets or sets the height.
		/// </summary>
		/// <value>
		/// The height.
		/// </value>
		public int Height { get; set; }
		/// <summary>
		/// Gets or sets the colors.
		/// </summary>
		/// <value>
		/// The colors.
		/// </value>
		public ReadOnlyCollection<Color> Colors { get; set; }

		/// <summary>
		/// Determines whether the specified <see cref="object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if(obj is DisplayModelSpecs other)
				Equals(other);

			return false;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return HashCode.Combine(Width, Height, Colors);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(DisplayModelSpecs left, DisplayModelSpecs right) => left.Equals(right);

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(DisplayModelSpecs left, DisplayModelSpecs right) => !(left == right);

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
		/// </returns>
		public bool Equals(DisplayModelSpecs other)
		{
			return other != null
				&& Width == other.Width
				&& Height == other.Height
				&& (
					((Colors is null) && (other.Colors is null))
					|| (Colors?.SequenceEqual(other.Colors)).GetValueOrDefault()
				);
		}
	}
}
