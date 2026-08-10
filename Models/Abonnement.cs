namespace ParkingManagement.API.Models;

public class Abonnement
{
    public int Id { get; set; }

    public int EntrepriseId { get; set; }
    public Entreprise? Entreprise { get; set; }

    public string Type { get; set; } = string.Empty;

    public DateTime DateDebut { get; set; }

    public DateTime DateFin { get; set; }

    public decimal Cout { get; set; }

    public string Statut { get; set; } = string.Empty;

    public string? Raison { get; set; }
}