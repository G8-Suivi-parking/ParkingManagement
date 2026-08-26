using Microsoft.EntityFrameworkCore;
using ParkingManagement.API.AI;
using ParkingManagement.API.Data;

namespace ParkingManagement.API.Services;

public class ParkingOccupancyService
{
    private readonly ApplicationDbContext _context;
    private readonly ParkingCalibrationService _calibrationService;
    private readonly VlmService _vlmService;

    public ParkingOccupancyService(
        ApplicationDbContext context,
        ParkingCalibrationService calibrationService,
        VlmService vlmService)
    {
        _context = context;
        _calibrationService = calibrationService;
        _vlmService = vlmService;
    }

    public async Task<object> AnalyzeParkingAsync(
        int parkingId,
        byte[] imageBytes)
    {
        // ============================================
        // 1. Récupérer le parking
        // ============================================

        var parking = await _context.Parkings
            .FirstOrDefaultAsync(p => p.Id == parkingId);

        if (parking == null)
        {
            throw new Exception(
                $"Le parking avec l'ID {parkingId} n'existe pas."
            );
        }

        // ============================================
        // 2. Récupérer les zones calibrées
        // ============================================

        var zones = _calibrationService.GetZones(parkingId);

        if (zones == null || zones.Count == 0)
        {
            throw new Exception(
                "Aucune zone de calibration n'est configurée pour ce parking."
            );
        }

        // ============================================
        // 3. Analyse de l'image avec le VLM
        // ============================================

        var vlmResponse =
            await _vlmService.AnalyzeImageAsync(
                imageBytes,
                zones
            );

        // ============================================
        // 4. Extraire les résultats
        // ============================================

        var occupancy =
            _vlmService.ExtractOccupancy(vlmResponse);

        var vehicles =
            Math.Max(occupancy.vehicles, 0);

        var freeSpaces =
            Math.Max(occupancy.freeSpaces, 0);

        // ============================================
        // 5. Calcul du nombre total de places
        // ============================================

        var totalPlaces =
            vehicles + freeSpaces;

        // Évite une occupation supérieure à 100 %
        if (totalPlaces > 0 && vehicles > totalPlaces)
        {
            vehicles = totalPlaces;
        }

        freeSpaces =
            Math.Max(totalPlaces - vehicles, 0);

        // ============================================
        // 6. Calcul du taux d'occupation
        // ============================================

        var tauxOccupation =
            totalPlaces > 0
                ? Math.Round(
                    (double)vehicles / totalPlaces * 100,
                    2
                )
                : 0;

        // ============================================
        // 7. Mise à jour du Parking
        // ============================================

        parking.PlacesOccupees = vehicles;
        parking.PlacesDisponibles = freeSpaces;
        parking.TauxOccupation = tauxOccupation;

        // ============================================
        // 8. Enregistrer dans PostgreSQL
        // ============================================

        await _context.SaveChangesAsync();

        // ============================================
        // 9. Retourner le résultat
        // ============================================

        return new
        {
            parkingId = parking.Id,
            totalPlaces,
            vehicles,
            placesLibres = freeSpaces,
            tauxOccupation,
            mode = "vlm",
            databaseUpdated = true
        };
    }
    public async Task<object> AnalyzeAllParkingsAsync(byte[] imageBytes)
{
    var parkings = await _context.Parkings
        .Where(p => !p.IsDeleted)
        .ToListAsync();

    var results = new List<object>();

    foreach (var parking in parkings)
    {
        try
        {
            var zones = _calibrationService.GetZones(parking.Id);

            if (zones == null || zones.Count == 0)
            {
                results.Add(new
                {
                    parkingId = parking.Id,
                    message = "Aucune zone de calibration configurée.",
                    success = false
                });

                continue;
            }

            var vlmResponse = await _vlmService.AnalyzeImageAsync(
                imageBytes,
                zones
            );

            var occupancy =
                _vlmService.ExtractOccupancy(vlmResponse);

            var vehicles = Math.Max(
                occupancy.vehicles,
                0
            );

            var freeSpaces = Math.Max(
                occupancy.freeSpaces,
                0
            );

            var totalPlaces =
                vehicles + freeSpaces;

            var tauxOccupation =
                totalPlaces > 0
                    ? Math.Round(
                        (double)vehicles / totalPlaces * 100,
                        2
                    )
                    : 0;

            // Mise à jour PostgreSQL
            parking.PlacesOccupees = vehicles;
            parking.PlacesDisponibles = freeSpaces;
            parking.TauxOccupation = tauxOccupation;

            results.Add(new
            {
                parkingId = parking.Id,
                totalPlaces,
                vehicles,
                placesLibres = freeSpaces,
                tauxOccupation,
                success = true
            });
        }
        catch (Exception ex)
        {
            results.Add(new
            {
                parkingId = parking.Id,
                success = false,
                error = ex.Message
            });
        }
    }

    // Enregistrer tous les changements
    await _context.SaveChangesAsync();

    return new
    {
        totalParkings = parkings.Count,
        results
    };
}
}