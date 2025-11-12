using Microsoft.AspNetCore.Mvc;

namespace Document.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Documents")]
public class DocumentsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllDocuments()
    {
        return Ok(new
        {
            message = "Document module is working!",
            documents = new[]
            {
                new { id = 1, title = "Sample Document 1", createdAt = DateTime.UtcNow },
                new { id = 2, title = "Sample Document 2", createdAt = DateTime.UtcNow }
            }
        });
    }

    [HttpGet("{id}")]
    public IActionResult GetDocumentById(int id)
    {
        return Ok(new
        {
            id = id,
            title = $"Document {id}",
            description = "Sample document description",
            createdAt = DateTime.UtcNow
        });
    }

    [HttpPost]
    public IActionResult CreateDocument([FromBody] object document)
    {
        return CreatedAtAction(nameof(GetDocumentById), new { id = 1 }, new
        {
            id = 1,
            message = "Document created successfully!",
            data = document
        });
    }
}