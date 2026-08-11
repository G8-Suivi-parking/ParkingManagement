namespace ParkingManagement.API.Models;

public class Entreprise
{
    public int Id { get; set; }

    public string Nom { get; set; } = string.Empty;

    public string NumeroFiscal { get; set; } = string.Empty;

    public string? Contact { get; set; }

    public string? Email { get; set; }

    public string? Adresse { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Abonnement> Abonnements { get; set; }
        = new List<Abonnement>();
}
