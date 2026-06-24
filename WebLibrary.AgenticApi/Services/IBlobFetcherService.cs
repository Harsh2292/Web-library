namespace WebLibrary.AgenticApi.Services
{
    public interface IBlobFetcherService
    {
        Task<byte[]> FetchPdfBytesAsync(string blobUrl, CancellationToken cancellationToken = default);
    }
}
