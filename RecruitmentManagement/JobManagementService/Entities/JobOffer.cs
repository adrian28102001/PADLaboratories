using JobManagementService.Entities.Enums;

namespace JobManagementService.Entities;

public class JobOffer
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    public int LocationId { get; set; }
    public virtual Location Location { get; set; }

    public JobType Type { get; set; }
    public decimal Salary { get; set; }

    public DateTime PostedDate { get; set; } = DateTime.Now;
    public DateTime? ClosingDate { get; set; }
}