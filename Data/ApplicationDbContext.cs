using Microsoft.EntityFrameworkCore;
using ParkingManagement.API.Models;

namespace ParkingManagement.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<AbonnementHistorique> AbonnementHistoriques { get; set; }

    public DbSet<Abonnement> Abonnements { get; set; }

    public DbSet<Entreprise> Entreprises { get; set; }

    public DbSet<Parking> Parkings { get; set; }

    public DbSet<Zone> Zones { get; set; }

    public DbSet<Acces> Acces { get; set; }

    public DbSet<ParkingHistorique> ParkingHistoriques { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Abonnement>()
            .HasOne(a => a.Entreprise)
            .WithMany(e => e.Abonnements)
            .HasForeignKey(a => a.EntrepriseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Zone>()
            .HasOne(z => z.Parking)
            .WithMany(p => p.Zones)
            .HasForeignKey(z => z.ParkingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Parking>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<Acces>()
            .HasOne(a => a.Parking)
            .WithMany()
            .HasForeignKey(a => a.ParkingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Acces>()
            .HasOne(a => a.Zone)
            .WithMany()
            .HasForeignKey(a => a.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}