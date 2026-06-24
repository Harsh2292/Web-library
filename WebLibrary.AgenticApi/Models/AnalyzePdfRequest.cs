using System.ComponentModel.DataAnnotations;

namespace WebLibrary.AgenticApi.Models
{
    public class AnalyzePdfRequest
    {
        [Required(ErrorMessage = "BlobUrl is required")]
        [Url(ErrorMessage = "BlobUrl must be a valid URL")]
        public string BlobUrl { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "MaxPages must be between 1 and 100")]
        public int MaxPages { get; set; } = 30;
    }
}
