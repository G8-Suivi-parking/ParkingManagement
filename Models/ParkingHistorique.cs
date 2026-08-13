namespace ParkingManagement.API.Models;

public class ParkingHistorique
{
    public int Id { get; set; }

    public int ParkingId { get; set; }

    public string NomParking { get; set; } = string.Empty;

    public string CodeParking { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? Raison { get; set; }

    public DateTime DateAction { get; set; }
}