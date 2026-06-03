using System.ComponentModel.DataAnnotations;

namespace Hirenix.Application.Validators;

/// <summary>
/// Validation attribute to ensure DateOnly is not in the future
/// </summary>
public class PastDateOnlyAttribute : ValidationAttribute
{
    public PastDateOnlyAttribute() : base("Date cannot be in the future")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            // Null is valid (use [Required] for mandatory fields)
            return ValidationResult.Success;
        }

        if (value is DateOnly dateValue)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            
            if (dateValue > today)
            {
                return new ValidationResult(ErrorMessage ?? "Date cannot be in the future");
            }

            return ValidationResult.Success;
        }

        return new ValidationResult("Invalid date format");
    }
}
