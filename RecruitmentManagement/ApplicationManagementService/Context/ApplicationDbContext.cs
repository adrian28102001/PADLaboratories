using ApplicationManagementService.Entities;
using ApplicationManagementService.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace ApplicationManagementService.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Candidate>().HasData(
            new Candidate { Id = 1, FullName = "John Doe", Email = "john.doe@example.com", PhoneNumber = "123-456-7890" },
            new Candidate { Id = 2, FullName = "Jane Smith", Email = "jane.smith@example.com", PhoneNumber = "234-567-8901" },
            new Candidate { Id = 3, FullName = "Robert Brown", Email = "robert.brown@example.com", PhoneNumber = "345-678-9012" }
        );

        modelBuilder.Entity<Application>().HasData(
            new Application 
            { 
                Id = 1, 
                CandidateId = 1, 
                JobOfferId = 1, 
                CVPath = "/path/to/cv1.pdf", 
                Status = ApplicationStatus.Applied,
                AppliedDate = DateTime.Now.AddDays(-10),
                LastUpdated = DateTime.Now.AddDays(-9),
                Feedback = "Great resume! Looking forward to the interview."
            },
            new Application 
            { 
                Id = 2, 
                CandidateId = 2, 
                JobOfferId = 2, 
                CVPath = "/path/to/cv2.pdf", 
                Status = ApplicationStatus.UnderReview,
                AppliedDate = DateTime.Now.AddDays(-5),
                LastUpdated = DateTime.Now.AddDays(-4),
                Feedback = "Impressive background!"
            },
            new Application 
            { 
                Id = 3, 
                CandidateId = 3, 
                JobOfferId = 3, 
                CVPath = "/path/to/cv3.pdf", 
                Status = ApplicationStatus.InterviewScheduled,
                AppliedDate = DateTime.Now.AddDays(-2),
                InterviewDate = DateTime.Now.AddDays(2),
                LastUpdated = DateTime.Now.AddDays(-1),
                Feedback = "Schedule for an interview next week."
            }
        );
    }
    
    public DbSet<Application> Applications { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
}