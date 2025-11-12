using Microsoft.AspNetCore.Mvc;

namespace Versioning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Versions")]
public class VersionsController : ControllerBase
{
    [HttpGet("document/{documentId}")]
    public IActionResult GetDocumentVersions(int documentId)
    {
        return Ok(new
        {
            documentId = documentId,
            message = "Versioning module is working!",
            versions = new[]
            {
                new { versionNumber = 1, createdAt = DateTime.UtcNow.AddDays(-2), createdBy = "User1" },
                new { versionNumber = 2, createdAt = DateTime.UtcNow.AddDays(-1), createdBy = "User2" },
                new { versionNumber = 3, createdAt = DateTime.UtcNow, createdBy = "User3" }
            }
        });
    }

    [HttpPost("document/{documentId}")]
    public IActionResult AddVersion(int documentId, [FromBody] object versionData)
    {
        return Ok(new
        {
            message = "Version added successfully!",
            documentId = documentId,
            versionNumber = 4,
            data = versionData
        });
    }
}