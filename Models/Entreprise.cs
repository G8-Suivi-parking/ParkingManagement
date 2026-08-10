namespace ParkingManagement.API.Models;

public class Entreprise
{
    public int Id { get; set; }

    public string Nom { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Telephone { get; set; } = string.Empty;
    public List<Abonnement> Abonnements { get; set; } = new();

}