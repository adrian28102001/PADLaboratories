namespace ApplicationManagementService.Models;

public class ApplicationModel
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public int JobOfferId { get; set; }
    public string CVPath { get; set; }
    public ApplicationStatusModel Status { get; set; }
    public DateTime AppliedDate { get; set; }
    public DateTime? InterviewDate { get; set; }
    public string Feedback { get; set; }
}
