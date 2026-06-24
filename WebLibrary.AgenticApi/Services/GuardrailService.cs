using System.Text.RegularExpressions;
using WebLibrary.AgenticApi.Models;

namespace WebLibrary.AgenticApi.Services
{
    public class GuardrailService : IGuardrailService
    {
        private readonly ILogger<GuardrailService> _logger;
        private const int MinExtractedTextLength = 100;
        private const decimal MaxReasonablePrice = 10000m;
        private const decimal MinReasonablePrice = 0.50m;

        public GuardrailService(ILogger<GuardrailService> logger)
        {
            _logger = logger;
        }

        public GuardrailResult ValidateExtractedText(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length < MinExtractedTextLength)
            {
                _logger.LogWarning(
                    "Extracted text guardrail failed, length: {Length}",
                    text?.Length ?? 0);

                return new GuardrailResult(false,
                    "Extracted text is too short to reliably analyze");
            }

            return new GuardrailResult(true);
        }

        public GuardrailResult ValidateMetadataOutput(MetadataAgentOutput output)
        {
            if (string.IsNullOrWhiteSpace(output.Title))
            {
                return new GuardrailResult(false, "Agent returned an empty title");
            }

            if (output.Title.Length > 300)
            {
                return new GuardrailResult(false, "Agent returned an unreasonably long title");
            }

            if (output.ConfidenceScore < 0.4)
            {
                _logger.LogWarning(
                    "Low confidence metadata output: {Confidence} for title {Title}",
                    output.ConfidenceScore, output.Title);

                return new GuardrailResult(false,
                    $"Metadata confidence too low ({output.ConfidenceScore:P0}) to trust automatically");
            }

            return new GuardrailResult(true);
        }

        public GuardrailResult ValidatePriceOutput(PriceAndCoverAgentOutput output)
        {
            if (output.EstimatedPrice < MinReasonablePrice ||
                output.EstimatedPrice > MaxReasonablePrice)
            {
                _logger.LogWarning(
                    "Price guardrail failed, value: {Price}",
                    output.EstimatedPrice);

                return new GuardrailResult(false,
                    $"Estimated price {output.EstimatedPrice:C} is outside reasonable range");
            }           

            return new GuardrailResult(true);
        }

        public bool IsPlagiarismFlagged(double similarityScore, double threshold)
            => similarityScore >= threshold;       
    }
}
