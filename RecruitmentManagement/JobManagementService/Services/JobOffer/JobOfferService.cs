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
}