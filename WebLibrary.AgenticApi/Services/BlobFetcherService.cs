using Azure.Storage.Blobs;

namespace WebLibrary.AgenticApi.Services
{
    public class BlobFetcherService : IBlobFetcherService
    {
        private readonly ILogger<BlobFetcherService> _logger;
        private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB

        public BlobFetcherService(ILogger<BlobFetcherService> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> FetchPdfBytesAsync(
            string blobUrl,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(blobUrl))
            {
                throw new ArgumentException("Blob URL cannot be empty", nameof(blobUrl));
            }

            _logger.LogInformation("Fetching PDF from blob: {BlobUrl}", blobUrl);

            try
            {
                var blobClient = new BlobClient(new Uri(blobUrl));

                var properties = await blobClient.GetPropertiesAsync(
                    cancellationToken: cancellationToken);

                if (properties.Value.ContentLength > MaxFileSizeBytes)
                {
                    throw new ArgumentException(
                        $"PDF file size {properties.Value.ContentLength} bytes exceeds " +
                        $"maximum allowed size of {MaxFileSizeBytes} bytes");
                }

                using var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream, cancellationToken);

                _logger.LogInformation(
                    "Successfully fetched PDF, size: {SizeBytes} bytes",
                    memoryStream.Length);

                return memoryStream.ToArray();
            }
            catch (Azure.RequestFailedException ex)
            {
                _logger.LogError(ex,
                    "Failed to fetch blob {BlobUrl}, status: {StatusCode}",
                    blobUrl, ex.Status);

                throw new HttpRequestException(
                    $"Failed to fetch PDF from blob storage: {ex.Message}", ex);
            }
        }
    }
}
