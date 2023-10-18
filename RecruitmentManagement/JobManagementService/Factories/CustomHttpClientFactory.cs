namespace JobManagementService.Factories;

public class CustomHttpClientFactory : IHttpClientFactory
{
    private readonly IConfiguration _configuration;

    public CustomHttpClientFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public HttpClient CreateClient(string name)
    {
        var apiGatewayUrl = _configuration.GetValue<string>("APIGatewayUrl");

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(apiGatewayUrl)
        };
        client.Timeout = TimeSpan.FromSeconds(35); // Set the global timeout here
        client.DefaultRequestHeaders.ConnectionClose = true;

        return client;
    }
}