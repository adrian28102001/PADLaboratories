namespace JobManagementService.Entities;

public enum ApplicationStatus
{
    Applied,           // When the candidate first submits the application
    UnderReview,       // When the recruiter is reviewing the application
    InterviewScheduled,// When an interview has been scheduled
    Interviewed,       // After the interview has taken place
    Offered,           // Job offered to the candidate
    Rejected,          // Not selected for further rounds or the job
    Accepted,          // Candidate has accepted the job offer
    Declined           // Candidate declined the job offer  
}