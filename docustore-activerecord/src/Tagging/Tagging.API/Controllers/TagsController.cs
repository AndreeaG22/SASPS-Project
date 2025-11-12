using Microsoft.AspNetCore.Mvc;

namespace Tagging.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Tags")]
public class TagsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllTags()
    {
        return Ok(new
        {
            message = "Tagging module is working!",
            tags = new[]
            {
                new { id = 1, name = "Urgent", color = "#FF0000" },
                new { id = 2, name = "Finance", color = "#00FF00" },
                new { id = 3, name = "HR", color = "#0000FF" }
            }
        });
    }

    [HttpPost("document/{documentId}")]
    public IActionResult AddTagToDocument(int documentId, [FromBody] object tagData)
    {
        return Ok(new
        {
            message = "Tag added to document successfully!",
            documentId = documentId,
            tag = tagData
        });
    }

    [HttpDelete("document/{documentId}/tag/{tagId}")]
    public IActionResult RemoveTagFromDocument(int documentId, int tagId)
    {
        return Ok(new
        {
            message = "Tag removed from document successfully!",
            documentId = documentId,
            tagId = tagId
        });
    }
}