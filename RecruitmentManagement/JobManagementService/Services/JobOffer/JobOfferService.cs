using ApplicationManagementService.Entities;
using SharedLibrary.Repositories;

namespace JobManagementService.Services.JobOffer;

public class JobOfferService : IJobOfferService
{
    private readonly IRepository<Application> _repository;

    public JobOfferService(IRepository<Application> repository)
    {
        _repository = repository;
    }

    public async Task<Application?> GetById(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}