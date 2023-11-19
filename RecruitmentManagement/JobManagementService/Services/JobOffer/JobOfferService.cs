using JobManagementService.Repositories;

namespace JobManagementService.Services.JobOffer;

public class JobOfferService : IJobOfferService
{
    private readonly IRepository<Entities.JobOffer> _repository;

    public JobOfferService(IRepository<Entities.JobOffer> repository)
    {
        _repository = repository;
    }

    public async Task<Entities.JobOffer?> GetById(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task DeleteJob(int jobOfferId)
    {
        var jobOffer = await _repository.GetByIdAsync(jobOfferId);

        if (jobOffer == null)
        {
            return;
        }

        await _repository.DeleteAsync(jobOffer);
    }
}