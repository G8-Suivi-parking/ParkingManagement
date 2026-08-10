namespace ParkingManagement.API.Models;

public class AbonnementHistorique
{
    public int Id { get; set; }

    public int AbonnementId { get; set; }

    public string Action { get; set; } = string.Empty;

    public DateTime DateAction { get; set; }

    public string? Details { get; set; }
}