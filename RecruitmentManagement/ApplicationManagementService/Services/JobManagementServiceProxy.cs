using ApplicationManagementService.Entities;
using Newtonsoft.Json;

namespace ApplicationManagementService.Services;

public class JobManagementServiceProxy
{
    private readonly HttpClient _client;

    public JobManagementServiceProxy(HttpClient client)
    {
        _client = client;
    }

    public async Task<JobOffer?> GetJobByIdAsync(int jobId)
    {
        var response = await _client.GetAsync($"api/joboffers/{jobId}");
        if (!response.IsSuccessStatusCode)
            return null;
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var job = JsonConvert.DeserializeObject<JobOffer>(jsonResponse);

        return job ?? null;
    }
}