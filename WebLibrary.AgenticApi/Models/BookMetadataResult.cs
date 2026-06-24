namespace WebLibrary.AgenticApi.Models
{
    public class BookMetadataResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedPrice { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;

        public PlagiarismInfo Plagiarism { get; set; } = new();
        public AgentMetadata Metadata { get; set; } = new();
    }

    public class PlagiarismInfo
    {
        public bool IsFlagged { get; set; }
        public double SimilarityScore { get; set; }
        public string? MatchedBookTitle { get; set; }
    }

    public class AgentMetadata
    {
        public int PagesProcessed { get; set; }
        public double ProcessingTimeSeconds { get; set; }
        public List<string> AgentsUsed { get; set; } = [];
    }
}
