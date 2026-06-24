namespace WebLibrary.AgenticApi.Models
{
    public class MetadataAgentOutput
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double ConfidenceScore { get; set; }
    }

    public class PlagiarismAgentOutput
    {
        public bool IsFlagged { get; set; }
        public double SimilarityScore { get; set; }
        public string? MatchedBookTitle { get; set; }
        public string? MatchedBookId { get; set; }
    }

    public class PriceAndCoverAgentOutput
    {
        public decimal EstimatedPrice { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
        public string DataSource { get; set; } = string.Empty;
    }
}
