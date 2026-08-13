using System.ComponentModel.DataAnnotations;

namespace ParkingManagement.API.Models;

public class Acces
{
    public int Id { get; set; }

    // Parking concerné
    public int ParkingId { get; set; }

    public Parking? Parking { get; set; }

    // Zone concernée
    public int ZoneId { get; set; }

    public Zone? Zone { get; set; }

    // Identification du véhicule
    [Required]
    [MaxLength(20)]
    public string Immatriculation { get; set; } = string.Empty;

    // Date et heure d'entrée
    public DateTime DateEntree { get; set; }

    // Date et heure de sortie
    public DateTime? DateSortie { get; set; }

    // true = véhicule actuellement dans le parking
    // false = véhicule sorti
    public bool EstPresent { get; set; }

    // Journalisation
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}