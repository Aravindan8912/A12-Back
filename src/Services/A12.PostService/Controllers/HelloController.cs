using A12.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace A12.PostService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController(A12DbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT CAST(@@SERVERNAME AS nvarchar(128)), CAST(DB_NAME() AS nvarchar(128))
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Hello from A12",
                connected = false
            });
        }

        return Ok(new
        {
            message = "Hello from A12",
            connected = true,
            server = reader.GetString(0),
            database = reader.GetString(1)
        });
    }
}
