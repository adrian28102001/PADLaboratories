namespace JobManagementService.Services.JobOffer;

public interface IJobOfferService
{
    Task<Entities.JobOffer?> GetById(int id);
    Task DeleteJob(int jobOfferId);
}