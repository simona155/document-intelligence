using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentIntelligence.Api.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetDocuments()
    {
        return Ok(new[]
        {
            new { Id = 1, Title = "Employment Agreement" },
            new { Id = 2, Title = "Company Policy" }
        });
    }
}