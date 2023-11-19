namespace JobManagementService.Models;

public class ApplicationModel
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public int JobOfferId { get; set; }
    public string CVPath { get; set; }
    public DateTime AppliedDate { get; set; }
    public DateTime? InterviewDate { get; set; }
    public string Feedback { get; set; }
    public IFormFile? CVFile { get; set; }
}