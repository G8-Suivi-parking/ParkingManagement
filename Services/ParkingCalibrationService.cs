using ParkingManagement.API.AI;

namespace ParkingManagement.API.Services;

public class ParkingCalibrationService
{
    // Zones organisées par parking
    private readonly Dictionary<int, List<ParkingZone>> _parkingZones = new();

    // ============================================
    // Récupérer les zones d'un parking
    // ============================================

    public List<ParkingZone> GetZones(int parkingId)
    {
        if (!_parkingZones.ContainsKey(parkingId))
        {
            _parkingZones[parkingId] = new List<ParkingZone>();
        }

        return _parkingZones[parkingId];
    }

    // ============================================
    // Ajouter une zone à un parking
    // ============================================

    public ParkingZone AddZone(
        int parkingId,
        ParkingZone zone)
    {
        var zones = GetZones(parkingId);

        zone.Id = zones.Count + 1;

        zones.Add(zone);

        return zone;
    }

    // ============================================
    // Supprimer toutes les zones d'un parking
    // ============================================

    public void ClearZones(int parkingId)
    {
        if (_parkingZones.ContainsKey(parkingId))
        {
            _parkingZones[parkingId].Clear();
        }
    }

    // ============================================
    // Vérifier si un parking possède des zones
    // ============================================

    public bool HasZones(int parkingId)
    {
        return _parkingZones.ContainsKey(parkingId)
               && _parkingZones[parkingId].Count > 0;
    }

    // ============================================
    // Récupérer tous les parkings avec leurs zones
    // ============================================

    public Dictionary<int, List<ParkingZone>> GetAllZones()
    {
        return _parkingZones;
    }
}