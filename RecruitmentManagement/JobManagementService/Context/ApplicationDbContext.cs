using JobManagementService.Entities;
using JobManagementService.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobManagementService.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Location Configuration
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(l => l.Id);  // Primary Key
        
            entity.Property(l => l.City)
                .IsRequired()
                .HasMaxLength(100);
        
            entity.Property(l => l.State)
                .HasMaxLength(100);
        
            entity.Property(l => l.Country)
                .IsRequired()
                .HasMaxLength(100);
        });
        
        // JobOffer Configuration
        modelBuilder.Entity<JobOffer>(entity =>
        {
            entity.HasKey(j => j.Id);  // Primary Key
        
            entity.Property(j => j.Title)
                .IsRequired()
                .HasMaxLength(200);
        
            entity.Property(j => j.Description)
                .IsRequired()
                .HasColumnType("text");
        
            entity.Property(j => j.Salary)
                .HasColumnType("decimal(18,2)");
        
            entity.Property(j => j.PostedDate)
                .IsRequired();
        
            entity.HasOne(j => j.Location)
                .WithMany()  // This would change if Location had a navigation property to JobOffers
                .HasForeignKey(j => j.LocationId)
                .OnDelete(DeleteBehavior.Cascade);  // This means if a Location is deleted, all related JobOffers will also be deleted.
        });
        
        modelBuilder.Entity<Location>().HasData(
            new Location { Id = 1, City = "New York", State = "NY", Country = "USA" },
            new Location { Id = 2, City = "London", State = "LDN", Country = "UK" },
            new Location { Id = 3, City = "Paris", State = "IDF", Country = "France" }
        );

        modelBuilder.Entity<JobOffer>().HasData(
            new JobOffer { Id = 1, Title = "Software Developer", LocationId = 1, Description = "Develop cutting-edge applications", Type = JobType.FullTime, Salary = 60000, PostedDate = DateTime.UtcNow },
            new JobOffer { Id = 2, Title = "System Administrator", LocationId = 2, Description = "Manage and maintain IT infrastructure", Type = JobType.FullTime, Salary = 50000, PostedDate = DateTime.UtcNow },
            new JobOffer { Id = 3, Title = "Database Administrator", LocationId = 3, Description = "Manage and maintain database systems", Type = JobType.FullTime, Salary = 55000, PostedDate = DateTime.UtcNow }
        );
    }
    
    public DbSet<JobOffer> JobOffers { get; set; }
    public DbSet<Location> Locations { get; set; }
}