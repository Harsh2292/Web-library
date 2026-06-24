namespace WebLibrary.AgenticApi.Services
{
    public record PdfExtractionResult(string ExtractedText, int TotalPageCount, int PagesExtracted);

    public interface IPdfExtractorService
    {
        PdfExtractionResult ExtractText(byte[] pdfBytes, int maxPages);
    }
}