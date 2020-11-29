using System;
using System.ComponentModel.DataAnnotations;

namespace InkyCal.Models.Validation
{
	/// <summary>
	/// Validates a url, also validated if the ulr adheres to <see cref="UriKind"/>
	/// </summary>
	/// <seealso cref="ValidationAttribute" />
	public class UrlAttribute : ValidationAttribute
	{
		public UriKind UriKind { get; }

		public UrlAttribute(UriKind uriKind = UriKind.Absolute)
		{
			UriKind = uriKind;
		}

		public override bool IsValid(object value)
		{
			if (!(value is string strUrl) || string.IsNullOrWhiteSpace(strUrl))
				return true;//This is not a required field attribute

			if (!Uri.TryCreate(strUrl, UriKind, out var url))
				return false;

			return true;
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (IsValid(value))
				return ValidationResult.Success;

			switch (UriKind)
			{
				case UriKind.Absolute:
					return new ValidationResult(
							validationContext == null
								? $"{value} is not a valid absolute url."
								: $"The value for {validationContext.DisplayName} should be an absolute url.");
				case UriKind.Relative:
					return new ValidationResult(
							validationContext == null
								? $"{value} is not a valid relative url."
								: $"The value for {validationContext.DisplayName} should be a relative url.");
				case UriKind.RelativeOrAbsolute:
				default:
					return new ValidationResult(
							validationContext == null
								? $"{value} is not a valid url."
								: $"The value for {validationContext.DisplayName} should be a valid url.");
			}

		}

		public override bool RequiresValidationContext => false;
	}
}
