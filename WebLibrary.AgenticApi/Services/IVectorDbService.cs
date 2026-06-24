using WebLibrary.AgenticApi.Models;

namespace WebLibrary.AgenticApi.Services
{
    public interface IVectorDbService
    {
        Task<PlagiarismAgentOutput> CheckPlagiarismAsync(
       string extractedText,
       double threshold,
       CancellationToken cancellationToken = default);

        Task StoreBookEmbeddingAsync(
            string bookId,
            string extractedText,
            CancellationToken cancellationToken = default);
    }
}
