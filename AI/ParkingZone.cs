namespace ParkingManagement.API.AI;

public class ParkingZone
{
    public int Id { get; set; }

    public string Nom { get; set; } = string.Empty;

    public int X { get; set; }

    public int Y { get; set; }

    public int Largeur { get; set; }

    public int Hauteur { get; set; }
}