using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VulnerableFunction;

public class HttpTrigger(ILogger<HttpTrigger> logger, IConfiguration config)
{
    private readonly ILogger<HttpTrigger> _logger = logger;
    private readonly IConfiguration _config = config;

    [Function("HttpTrigger")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        var userId = req.Query["userId"];
        if (string.IsNullOrEmpty(userId))
        {
            var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            return response;
        }

        var sqlQuery = $"SELECT * FROM Users WHERE UserId = '{userId}'";
        var connectionString = _config.GetConnectionString("database");
        if (!string.IsNullOrEmpty(connectionString))
        {
            try
            {
                _logger.LogInformation("Executing SQL query: {SqlQuery}", sqlQuery);
                using var sqlConnection = new SqlConnection(connectionString);
                using var command = new SqlCommand(sqlQuery, sqlConnection);
                await sqlConnection.OpenAsync();
                await command.ExecuteReaderAsync();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while processing the request.");
                var response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                return response;
            }
        }
        return req.CreateResponse(System.Net.HttpStatusCode.OK);
    }
}
 