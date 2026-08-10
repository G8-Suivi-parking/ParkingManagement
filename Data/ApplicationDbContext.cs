using Microsoft.EntityFrameworkCore;
using ParkingManagement.API.Models;

namespace ParkingManagement.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AbonnementHistorique> AbonnementHistoriques { get; set; }
    public DbSet<Abonnement> Abonnements { get; set; }
    public DbSet<Entreprise> Entreprises { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Abonnement>()
            .HasOne(a => a.Entreprise)
            .WithMany(e => e.Abonnements)
            .HasForeignKey(a => a.EntrepriseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}