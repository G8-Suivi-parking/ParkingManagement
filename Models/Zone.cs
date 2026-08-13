using System.ComponentModel.DataAnnotations;

namespace ParkingManagement.API.Models;

public class Zone
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Required]
    public int Capacite { get; set; }

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Clé étrangère vers Parking
    public int ParkingId { get; set; }

    // Relation avec Parking
    public Parking? Parking { get; set; }
}