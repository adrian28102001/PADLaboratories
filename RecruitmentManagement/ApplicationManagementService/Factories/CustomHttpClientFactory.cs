namespace ApplicationManagementService.Factories;

public class CustomHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var client = new HttpClient(handler);
        client.Timeout = TimeSpan.FromSeconds(5); // Set the global timeout here
        client.DefaultRequestHeaders.ConnectionClose = true;

        return client;
    }
}