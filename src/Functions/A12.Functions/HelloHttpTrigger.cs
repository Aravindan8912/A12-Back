using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace A12.Functions;

public class HelloHttpTrigger
{
    [Function("Hello")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "hello")] HttpRequest request)
    {
        var name = request.Query["name"].ToString();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "A12";
        }

        return new OkObjectResult(new
        {
            message = $"Hello, {name}. This response came from an Azure Function HTTP trigger.",
            trigger = "http",
            name
        });
    }
}
