using WebLibrary.AgenticApi.Models;

namespace WebLibrary.AgenticApi.Services
{
    public record GuardrailResult(bool IsValid, string? FailureReason = null);
    public interface IGuardrailService
    {
        GuardrailResult ValidateExtractedText(string text);

        GuardrailResult ValidateMetadataOutput(MetadataAgentOutput output);

        GuardrailResult ValidatePriceOutput(PriceAndCoverAgentOutput output);

        bool IsPlagiarismFlagged(double similarityScore, double threshold);
    }
}
