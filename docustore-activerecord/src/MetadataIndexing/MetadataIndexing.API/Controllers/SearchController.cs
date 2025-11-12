using Microsoft.AspNetCore.Mvc;

namespace MetadataIndexing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public IActionResult SearchDocuments([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        return Ok(new
        {
            message = "MetadataIndexing module is working!",
            query = query,
            page = page,
            pageSize = pageSize,
            totalResults = 42,
            results = new[]
            {
                new { id = 1, title = "Matching Document 1", relevance = 0.95 },
                new { id = 2, title = "Matching Document 2", relevance = 0.87 },
                new { id = 3, title = "Matching Document 3", relevance = 0.75 }
            }
        });
    }

    [HttpPost("reindex/{documentId}")]
    public IActionResult ReindexDocument(int documentId)
    {
        return Ok(new
        {
            message = "Document reindexed successfully!",
            documentId = documentId,
            indexedAt = DateTime.UtcNow
        });
    }
}