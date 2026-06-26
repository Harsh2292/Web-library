using WebLibrary.AgenticApi.Models;

namespace WebLibrary.AgenticApi.Workflows
{
    public interface IBookMetadataWorkflow
    {
        Task<BookMetadataResult> RunAsync(
        AnalyzePdfRequest request,
        CancellationToken cancellationToken = default);
    }
}
