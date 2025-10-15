using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace VulnerableFunction;

public class HttpTrigger(ILogger<HttpTrigger> logger)
{
    private readonly ILogger<HttpTrigger> _logger = logger;

    [Function("HttpTrigger")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return req.CreateResponse(System.Net.HttpStatusCode.OK);
    }
}
 