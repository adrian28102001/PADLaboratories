using ApplicationManagementService.Entities.Enums;

namespace JobManagementService.Entities;

public class Application
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public int JobOfferId { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedDate { get; set; }
    public DateTime? InterviewDate { get; set; }
    public DateTime? LastUpdated { get; set; }

    // Feedback or notes added by the recruiter
    public string Feedback { get; set; }
}