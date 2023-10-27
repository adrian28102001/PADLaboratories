using ApplicationManagementService.Entities;
using ApplicationManagementService.Entities.Enums;
using ApplicationManagementService.Extensions;
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
        modelBuilder.Seed();
    }
    
    public DbSet<Application> Applications { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
}