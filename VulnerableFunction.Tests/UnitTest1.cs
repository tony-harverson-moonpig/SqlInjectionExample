using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Specialized;

namespace VulnerableFunction.Tests;

public class HttpTriggerTests
{
    private readonly Mock<ILogger<HttpTrigger>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfig;
    private HttpTrigger _httpTrigger;
    private readonly Mock<FunctionContext> _context;
    private readonly Mock<HttpRequestData> _requestMoq;
    private readonly Mock<HttpResponseData> _responseMoq;

    public HttpTriggerTests()
    {
        _mockLogger = new Mock<ILogger<HttpTrigger>>();
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(x => x.GetSection("ConnectionStrings")).Returns(new ConfigurationSection(new ConfigurationRoot([]), "ConnectionStrings"));
        _httpTrigger = new HttpTrigger(_mockLogger.Object,_mockConfig.Object);
        _context = new Mock<FunctionContext>(MockBehavior.Strict);
        _requestMoq = new Mock<HttpRequestData>(MockBehavior.Strict, _context.Object);
        _requestMoq.Setup(r => r.Method).Returns("GET");
        _responseMoq = new Mock<HttpResponseData>(MockBehavior.Strict, _context.Object);
        _responseMoq.SetupProperty(_ => _.StatusCode);
        _requestMoq.Setup(_ => _.CreateResponse()).Returns(_responseMoq.Object);
    }

    [Fact]
    public async Task GetRequest_ReturnsOkObjectResult()
    {
        var query = new NameValueCollection { { "userid", "123" } };
        _requestMoq.Setup(r => r.Query).Returns(query);

        var result = await _httpTrigger.Run(_requestMoq.Object);
        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetRequest_WithoutUserId_ReturnsBadRequest()
    {
        var query = new NameValueCollection();
        _requestMoq.Setup(r => r.Query).Returns(query);
        var result = await _httpTrigger.Run(_requestMoq.Object);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task GetRequest_WithBrokenSqlConnection_ReturnsServerError()
    {
        var config = new ConfigurationBuilder()
         .AddInMemoryCollection(new Dictionary<string, string?>
         {
             ["ConnectionStrings:database"] = "Server=myServer;Database=myDb;User Id=myUser;Password=myPass;Connect Timeout=1"
         })
         .Build();

        _httpTrigger = new HttpTrigger(_mockLogger.Object, config);
        var query = new NameValueCollection { { "userid", "123" } };
        _requestMoq.Setup(r => r.Query).Returns(query);

        var result = await _httpTrigger.Run(_requestMoq.Object);
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, result.StatusCode);

    }
}
