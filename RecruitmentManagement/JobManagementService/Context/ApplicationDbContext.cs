using JobManagementService.Entities;
using JobManagementService.Entities.Enums;
using JobManagementService.Extensions;
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
        modelBuilder.Seed();
        modelBuilder.ConfigureEntities();
    }
    
    public DbSet<JobOffer> JobOffers { get; set; }
    public DbSet<Location> Locations { get; set; }
}