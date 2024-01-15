using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ZipBlobFilesFunction
{
    public class StringArrayRequiredAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is not IEnumerable<string> array || array.All(string.IsNullOrEmpty))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
