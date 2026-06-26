using Microsoft.AspNetCore.Mvc;
using WebLibrary.AgenticApi.Models;
using WebLibrary.AgenticApi.Workflows;

namespace WebLibrary.AgenticApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductAnalysisController : Controller
    {
        private readonly IBookMetadataWorkflow _workflow;
        private readonly ILogger<ProductAnalysisController> _logger;

        public ProductAnalysisController(
            IBookMetadataWorkflow workflow,
            ILogger<ProductAnalysisController> logger)
        {
            _workflow = workflow;
            _logger = logger;
        }

        [HttpPost("analyze-pdf")]
        [ProducesResponseType(typeof(BookMetadataResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BookMetadataResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BookMetadataResult), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(BookMetadataResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AnalyzePdf(
            [FromBody] AnalyzePdfRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "AnalyzePdf called for BlobUrl: {BlobUrl}",
                request.BlobUrl);

            var result = await _workflow.RunAsync(request, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning(
                    "Workflow returned failure for BlobUrl: {BlobUrl}, Reason: {Reason}",
                    request.BlobUrl,
                    result.ErrorMessage);

                return UnprocessableEntity(result);
            }

            _logger.LogInformation(
                "AnalyzePdf completed successfully for BlobUrl: {BlobUrl}",
                request.BlobUrl);

            return Ok(result);
        }
    }
}
