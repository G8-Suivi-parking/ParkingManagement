using System.ComponentModel.DataAnnotations;

namespace ParkingManagement.API.Models;

public class Parking
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Adresse { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public TimeSpan? HeureOuverture { get; set; }

    public TimeSpan? HeureFermeture { get; set; }

    public bool Ouvert24h { get; set; } = false;

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Relation avec les zones
    public ICollection<Zone> Zones { get; set; } = new List<Zone>();
}