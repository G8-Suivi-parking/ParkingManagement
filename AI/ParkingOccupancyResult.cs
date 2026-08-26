namespace ParkingManagement.API.AI;

public class ParkingOccupancyResult
{
    public int ZoneId { get; set; }

    public string NomZone { get; set; } = string.Empty;

    public bool EstOccupee { get; set; }

    public double TauxOccupation { get; set; }
}