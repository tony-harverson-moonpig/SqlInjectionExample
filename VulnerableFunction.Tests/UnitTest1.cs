using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace VulnerableFunction.Tests;

public class HttpTriggerTests
{
    private readonly Mock<ILogger<HttpTrigger>> _mockLogger;
    private readonly HttpTrigger _httpTrigger;
    private readonly Mock<FunctionContext> _context;
    private readonly Mock<HttpRequestData> _requestMoq;
    private readonly Mock<HttpResponseData> _responseMoq;

    public HttpTriggerTests()
    {
        _mockLogger = new Mock<ILogger<HttpTrigger>>();
        _httpTrigger = new HttpTrigger(_mockLogger.Object);
        _context = new Mock<FunctionContext>(MockBehavior.Strict);
        _requestMoq = new Mock<HttpRequestData>(MockBehavior.Strict, _context.Object);
        _requestMoq.Setup(r => r.Method).Returns("GET");
        _responseMoq = new Mock<HttpResponseData>(MockBehavior.Strict, _context.Object);
        _responseMoq.SetupProperty(_ => _.StatusCode);
        _requestMoq.Setup(_ => _.CreateResponse()).Returns(_responseMoq.Object);
    }

    [Fact]
    public async Task Run_WithGetRequest_ReturnsOkObjectResult()
    {
        var result = await _httpTrigger.Run(_requestMoq.Object);
        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
    }
}
