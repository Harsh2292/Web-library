using System.Diagnostics;
using WebLibrary.AgenticApi.Agents;
using WebLibrary.AgenticApi.Models;
using WebLibrary.AgenticApi.Services;

namespace WebLibrary.AgenticApi.Workflows
{
    public class BookMetadataWorkflow : IBookMetadataWorkflow
    {
        private readonly IBlobFetcherService _blobFetcher;
        private readonly IPdfExtractorService _pdfExtractor;
        private readonly IGuardrailService _guardrail;
        private readonly IMetadataExtractorAgent _metadataAgent;
        private readonly IVectorDbService _vectorDb;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BookMetadataWorkflow> _logger;

        public BookMetadataWorkflow(
            IBlobFetcherService blobFetcher,
            IPdfExtractorService pdfExtractor,
            IGuardrailService guardrail,
            IMetadataExtractorAgent metadataAgent,
            IVectorDbService vectorDb,
            IConfiguration configuration,
            ILogger<BookMetadataWorkflow> logger)
        {
            _blobFetcher = blobFetcher;
            _pdfExtractor = pdfExtractor;
            _guardrail = guardrail;
            _metadataAgent = metadataAgent;
            _vectorDb = vectorDb;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<BookMetadataResult> RunAsync(
            AnalyzePdfRequest request,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "BookMetadataWorkflow started for BlobUrl: {BlobUrl}",
                request.BlobUrl);

            // ── Step 1: Fetch PDF bytes from blob ──────────────────────────
            var pdfBytes = await _blobFetcher.FetchPdfBytesAsync(
                request.BlobUrl,
                cancellationToken);

            // ── Step 2: Extract text from PDF ──────────────────────────────
            var extraction = _pdfExtractor.ExtractText(pdfBytes, request.MaxPages);

            // ── Step 3: Input guardrail on extracted text ───────────────────
            var textGuardrail = _guardrail.ValidateExtractedText(extraction.ExtractedText);

            if (!textGuardrail.IsValid)
            {
                _logger.LogWarning(
                    "Input guardrail failed: {Reason}",
                    textGuardrail.FailureReason);

                return new BookMetadataResult
                {
                    Success = false,
                    ErrorMessage = textGuardrail.FailureReason
                };
            }

            // ── Step 4: Run agents concurrently ────────────────────────────
            var plagiarismThreshold = _configuration
                .GetValue<double>("AgentSettings:PlagiarismThreshold");

            var metadataTask = _metadataAgent.ExtractAsync(
                extraction.ExtractedText,
                cancellationToken);

            var plagiarismTask = _vectorDb.CheckPlagiarismAsync(
                extraction.ExtractedText,
                plagiarismThreshold,
                cancellationToken);

            await Task.WhenAll(metadataTask, plagiarismTask);

            var metadataOutput = await metadataTask;
            var plagiarismOutput = await plagiarismTask;

            // ── Step 5: Output guardrail on metadata ────────────────────────
            var metadataGuardrail = _guardrail.ValidateMetadataOutput(metadataOutput);

            if (!metadataGuardrail.IsValid)
            {
                _logger.LogWarning(
                    "Metadata guardrail failed: {Reason}",
                    metadataGuardrail.FailureReason);

                return new BookMetadataResult
                {
                    Success = false,
                    ErrorMessage = metadataGuardrail.FailureReason
                };
            }

            // ── Step 6: Aggregate results ───────────────────────────────────
            stopwatch.Stop();

            var result = new BookMetadataResult
            {
                Success = true,
                Title = metadataOutput.Title,
                Author = metadataOutput.Author,
                Description = metadataOutput.Description,
                Plagiarism = new PlagiarismInfo
                {
                    IsFlagged = plagiarismOutput.IsFlagged,
                    SimilarityScore = plagiarismOutput.SimilarityScore,
                    MatchedBookTitle = plagiarismOutput.MatchedBookTitle
                },
                Metadata = new AgentMetadata
                {
                    PagesProcessed = extraction.PagesExtracted,
                    ProcessingTimeSeconds = stopwatch.Elapsed.TotalSeconds,
                    AgentsUsed = ["MetadataExtractorAgent", "PlagiarismCheck"]
                }
            };

            _logger.LogInformation(
                "BookMetadataWorkflow completed in {Seconds}s. " +
                "Title: {Title}, Plagiarism flagged: {Flagged}",
                stopwatch.Elapsed.TotalSeconds,
                result.Title,
                result.Plagiarism.IsFlagged);

            return result;
        }
    }
}
