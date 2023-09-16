using ApplicationManagementService.Entities;
using ApplicationManagementService.Entities.Enums;
using ApplicationManagementService.Models;

namespace ApplicationManagementService.Extensions;

public static class MappingExtensions
{
    public static Application ToModel(this ApplicationModel model)
    {
        return new Application()
        {
            CandidateId = model.CandidateId,
            JobOfferId = model.JobOfferId,
            CVPath = model.CVPath,
            Status = ApplicationStatus.Accepted,
            AppliedDate = model.AppliedDate,
            InterviewDate = model.InterviewDate,
            Feedback = model.Feedback,
        };
    }
}