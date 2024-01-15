using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ZipBlobFilesFunction
{
    public static class ModelValidation
    {
        public static bool TryValidateObject(object request, out List<ValidationResult> validationResults)
        {
            validationResults = new List<ValidationResult>();
            return Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true);
        }
    }
}
