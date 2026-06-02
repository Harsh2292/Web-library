using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.IO;

namespace WebLibrary.Utilities.ValidationAttributes
{
    public class ExternalValidationAttributes : ValidationAttribute
    {
        private readonly string[] _extensions = { ".jpg", ".jpeg", ".png",};
        private readonly string[] _mimeTypes = { "image/jpeg", "image/png", "image/jpg" };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var contentType = file.ContentType.ToLowerInvariant();

                if (!_extensions.Contains(extension) || !_mimeTypes.Contains(contentType))
                {
                    return new ValidationResult(ErrorMessage ?? "Only JPG, JPEG, PNG images are allowed.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
