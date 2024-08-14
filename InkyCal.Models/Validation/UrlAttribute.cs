using System;
using System.ComponentModel.DataAnnotations;

namespace InkyCal.Models.Validation
{

	/// <summary>
	/// Validates a url, also validated if the ulr adheres to <see cref="UriKind"/>
	/// </summary>
	/// <seealso cref="ValidationAttribute" />
	[AttributeUsage(AttributeTargets.Property)]
	public class UrlAttribute : ValidationAttribute
	{
		/// <summary>
		/// Gets the kind of the URI.
		/// </summary>
		public UriKind UriKind { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="UrlAttribute"/> class.
		/// </summary>
		/// <param name="uriKind"></param>
		public UrlAttribute(UriKind uriKind = UriKind.Absolute)
		{
			UriKind = uriKind;
		}

		/// <summary>
		/// Determines whether the specified value is valid.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool IsValid(object value)
		{
			if (value is not string strUrl || string.IsNullOrWhiteSpace(strUrl))
				return true;//This is not a required field attribute

			if (!Uri.TryCreate(strUrl, UriKind, out var url))
				return false;

			return true;
		}

		/// <summary>
		/// Validates the specified value with respect to the current validation context.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="validationContext"></param>
		/// <returns></returns>
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (IsValid(value))
				return ValidationResult.Success;

			return UriKind switch
			{
				UriKind.Absolute => new ValidationResult(
											validationContext == null
												? $"{value} is not a valid absolute url."
												: $"The value for {validationContext.DisplayName} should be an absolute url."),
				UriKind.Relative => new ValidationResult(
											validationContext == null
												? $"{value} is not a valid relative url."
												: $"The value for {validationContext.DisplayName} should be a relative url."),
				_ => new ValidationResult(
											validationContext == null
												? $"{value} is not a valid url."
												: $"The value for {validationContext.DisplayName} should be a valid url."),
			};
		}

		/// <summary>
		/// Gets a value that indicates whether the attribute requires validation context, always returns false.
		/// </summary>
		public override bool RequiresValidationContext => false;
	}
}
