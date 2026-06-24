using WebLibrary.AgenticApi.Models;

namespace WebLibrary.AgenticApi.Agents
{
    public interface IMetadataExtractorAgent
    {
        Task<MetadataAgentOutput> ExtractAsync(
        string extractedText,
        CancellationToken cancellationToken = default);
    }
}
