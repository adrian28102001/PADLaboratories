using JobManagementService.Context;
using JobManagementService.Services.JobOffer;

namespace JobManagementService.Saga;

public class SagaCoordinator : ISagaCoordinator
{
    private readonly HttpClient _apiGatewayClient;
    private readonly IJobOfferService _jobOfferService;
    private readonly ApplicationDbContext _context;

    public SagaCoordinator(IHttpClientFactory clientFactory, IJobOfferService jobOfferService,
        ApplicationDbContext context)
    {
        _jobOfferService = jobOfferService;
        _context = context;
        _apiGatewayClient = clientFactory.CreateClient("APIGateway");
    }

    public async Task CloseJobSaga(int jobId, bool shouldFail)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Step 1: Delete the job (staged, not committed yet)
            await _jobOfferService.DeleteJob(jobId);

            // Step 2: Attempt to delete applications
            var response = await _apiGatewayClient.DeleteAsync($"applicationmanagement/api/applications/{jobId}/{shouldFail}");

            if (response.IsSuccessStatusCode)
            {
                // If applications deletion is successful, commit the transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            else
            {
                // If application deletion fails, the transaction will be rolled back
                throw new Exception($"Failed to delete applications for job {jobId}");
            }
        }
        catch (Exception ex)
        {
            // Rollback the transaction in case of any failure
            await transaction.RollbackAsync();
            Console.WriteLine($"Saga failed for closing job {jobId}. Ex: {ex}");
            throw;
        }
    }
}