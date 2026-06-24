using Pinecone;
using WebLibrary.AgenticApi.Models;

namespace WebLibrary.AgenticApi.Services
{
    public class VectorDbService : IVectorDbService
    {
        private readonly PineconeClient _pinecone;
        private readonly ILogger<VectorDbService> _logger;
        private readonly string _indexName;
        private const string TextField = "text";
        private const string TitleField = "bookTitle";
        private const string BookIdField = "bookId";
        private const string DefaultNamespace = "__default__";

        public VectorDbService(
            IConfiguration configuration,
            ILogger<VectorDbService> logger)
        {
            _logger = logger;

            var apiKey = configuration["VectorDb:ApiKey"]
                ?? throw new InvalidOperationException(
                    "VectorDb:ApiKey is not configured");

            _indexName = configuration["VectorDb:IndexName"]
                ?? throw new InvalidOperationException(
                    "VectorDb:IndexName is not configured");

            _pinecone = new PineconeClient(apiKey);
        }

        public async Task<PlagiarismAgentOutput> CheckPlagiarismAsync(
            string extractedText,
            double threshold,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting plagiarism check via Pinecone");

            var index = _pinecone.Index(_indexName);

            var response = await index.SearchRecordsAsync(
                DefaultNamespace,
                new SearchRecordsRequest
                {
                    Query = new SearchRecordsRequestQuery
                    {
                        TopK = 3,
                        Inputs = new Dictionary<string, object?>
                        {
                        { TextField, extractedText }
                        }
                    },
                    Fields = [TitleField, BookIdField]
                },
                cancellationToken: cancellationToken);

            var topResult = response.Result?.Hits?.FirstOrDefault();

            if (topResult is null)
            {
                _logger.LogInformation("No similar books found in vector DB");

                return new PlagiarismAgentOutput
                {
                    IsFlagged = false,
                    SimilarityScore = 0
                };
            }

            var similarityScore = (double)(topResult.Score);
            var isFlagged = similarityScore >= threshold;

            var fields = topResult.Fields ?? new Dictionary<string, object?>();

            fields.TryGetValue(TitleField, out var matchedTitle);
            fields.TryGetValue(BookIdField, out var matchedBookId);

            _logger.LogInformation(
                "Plagiarism check complete. Score: {Score}, Flagged: {Flagged}, " +
                "MatchedBook: {MatchedBook}",
                similarityScore,
                isFlagged,
                matchedTitle ?? "none");

            return new PlagiarismAgentOutput
            {
                IsFlagged = isFlagged,
                SimilarityScore = similarityScore,
                MatchedBookTitle = matchedTitle?.ToString(),
                MatchedBookId = matchedBookId?.ToString()
            };
        }

        public async Task StoreBookEmbeddingAsync(
            string bookId,
            string extractedText,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Storing embedding for BookId: {BookId}", bookId);

            var index = _pinecone.Index(_indexName);

            await index.UpsertRecordsAsync(
                DefaultNamespace,
                [
                    new UpsertRecord
                {
                    Id = bookId,
                    AdditionalProperties = new AdditionalProperties
                 {
                    { TextField, extractedText },
                    { BookIdField, bookId }
                 }
                }
                ],
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Successfully stored embedding for BookId: {BookId}", bookId);
        }
    }
}
