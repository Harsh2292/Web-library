using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace WebLibrary.AgenticApi.Services
{
    public class PdfExtractorService : IPdfExtractorService
    {
        private readonly ILogger<PdfExtractorService> _logger;

        public PdfExtractorService(ILogger<PdfExtractorService> logger)
        {
            _logger = logger;
        }

        public PdfExtractionResult ExtractText(byte[] pdfBytes, int maxPages)
        {
            if (pdfBytes is null || pdfBytes.Length == 0)
            {
                throw new ArgumentException("PDF bytes cannot be empty", nameof(pdfBytes));
            }

            using var stream = new MemoryStream(pdfBytes);
            using var pdfReader = new PdfReader(stream);
            using var pdfDocument = new PdfDocument(pdfReader);

            var totalPageCount = pdfDocument.GetNumberOfPages();

            _logger.LogInformation(
                "PDF opened, total pages: {TotalPages}", totalPageCount);

            if (totalPageCount > 100)
            {
                _logger.LogWarning(
                    "PDF rejected, page count {TotalPages} exceeds 100 page limit",
                    totalPageCount);

                throw new ArgumentException(
                    $"PDF has {totalPageCount} pages, which exceeds the maximum allowed " +
                    "100 pages");
            }

            var pagesToExtract = Math.Min(maxPages, totalPageCount);
            var textBuilder = new System.Text.StringBuilder();

            for (var pageNum = 1; pageNum <= pagesToExtract; pageNum++)
            {
                var page = pdfDocument.GetPage(pageNum);
                var pageText = PdfTextExtractor.GetTextFromPage(page);

                textBuilder.AppendLine($"--- Page {pageNum} ---");
                textBuilder.AppendLine(pageText);
            }

            var extractedText = textBuilder.ToString();

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                _logger.LogWarning(
                    "No extractable text found, PDF may be scanned images only");

                throw new InvalidOperationException(
                    "No text could be extracted from the PDF. It may be a scanned " +
                    "document without OCR text layer.");
            }

            _logger.LogInformation(
                "Extracted {CharCount} characters from {PagesExtracted} pages",
                extractedText.Length, pagesToExtract);

            return new PdfExtractionResult(extractedText, totalPageCount, pagesToExtract);
        }
    }
}
