using System;
using System.ComponentModel.DataAnnotations;

namespace InkyCal.Models.Validation
{
	/// <summary>
	/// Validates that the value is a defined enum value.
	/// </summary>
	/// <seealso cref="ValidationAttribute" />
	public class DefinedEnumAttribute : ValidationAttribute {

		/// <summary>
		/// Validates if <paramref name="value"/> is a defined enum value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool IsValid(object value)
		{
			if (value is null)
				return true;

			var type = value.GetType();

			if(!type.IsEnum)
				throw new ArgumentException($"The value for {nameof(value)} should be an enum.");

			return Enum.IsDefined(value.GetType(), value);
		}

		/// <summary>
		/// Validates if <paramref name="value"/> is a defined enum value.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="validationContext"></param>
		/// <returns></returns>
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (IsValid(value))
				return ValidationResult.Success;

			return new ValidationResult(
						validationContext == null
							? $"{value} is not a defined {value.GetType().Name}."
							: $"The value for {validationContext.DisplayName} should be an defined {value.GetType().Name}.");
		}
	}
}
