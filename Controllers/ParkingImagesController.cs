using Microsoft.AspNetCore.Mvc;
using ParkingManagement.API.AI;
using ParkingManagement.API.Services;

namespace ParkingManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParkingImagesController : ControllerBase
{
    private readonly ParkingImageService _parkingImageService;
    private readonly IConfiguration _configuration;
    private readonly VlmService _vlmService;
    private readonly ParkingCalibrationService _calibrationService;

    public ParkingImagesController(
        ParkingImageService parkingImageService,
        IConfiguration configuration,
        VlmService vlmService,
        ParkingCalibrationService calibrationService)
    {
        _parkingImageService = parkingImageService;
        _configuration = configuration;
        _vlmService = vlmService;
        _calibrationService = calibrationService;
    }

    // ============================================
    // Récupérer et sauvegarder l'image du parking
    // ============================================

    [HttpGet("retrieve")]
    public async Task<IActionResult> RetrieveImage()
    {
        var imageUrl =
            _configuration["ParkingImageApi:ImageUrl"];

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return BadRequest(
                "L'URL de l'image n'est pas configurée."
            );
        }

        var image =
            await _parkingImageService.GetParkingImageAsync(
                imageUrl
            );

        if (!_parkingImageService.IsImageValid(image))
        {
            return BadRequest(
                "L'image récupérée est invalide."
            );
        }

        var fileName =
            $"parking_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";

        var imagesFolder =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "Images"
            );

        Directory.CreateDirectory(imagesFolder);

        var filePath =
            Path.Combine(imagesFolder, fileName);

        await System.IO.File.WriteAllBytesAsync(
            filePath,
            image
        );

        return Ok(new
        {
            message =
                "Image récupérée et sauvegardée avec succès.",
            fileName
        });
    }

    // ============================================
    // Analyser l'image avec le VLM
    // ============================================

    [HttpPost("analyze/{parkingId}")]
    public async Task<IActionResult> AnalyzeImage(
        int parkingId)
    {
        try
        {
            // --------------------------------------------
            // 1. Vérifier l'URL de l'image
            // --------------------------------------------

            var imageUrl =
                _configuration["ParkingImageApi:ImageUrl"];

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return BadRequest(
                    "L'URL de l'image n'est pas configurée."
                );
            }

            // --------------------------------------------
            // 2. Récupérer l'image
            // --------------------------------------------

            var image =
                await _parkingImageService.GetParkingImageAsync(
                    imageUrl
                );

            if (image == null || image.Length == 0)
            {
                return BadRequest(
                    "Impossible de récupérer l'image."
                );
            }

            if (!_parkingImageService.IsImageValid(image))
            {
                return BadRequest(
                    "L'image récupérée est invalide."
                );
            }

            // --------------------------------------------
            // 3. Récupérer les zones du parking
            // --------------------------------------------

            var zones =
                _calibrationService.GetZones(parkingId);

            if (zones.Count == 0)
            {
                return BadRequest(new
                {
                    message =
                        "Aucune zone de calibration n'est configurée pour ce parking.",
                    parkingId
                });
            }

            // --------------------------------------------
            // 4. Envoyer image + zones au VLM
            // --------------------------------------------

            var vlmResponse =
                await _vlmService.AnalyzeImageAsync(
                    image,
                    zones
                );

            // --------------------------------------------
            // 5. Extraire les résultats du VLM
            // --------------------------------------------

            var occupancy =
                _vlmService.ExtractOccupancy(
                    vlmResponse
                );

            var vehicles =
                Math.Max(
                    occupancy.vehicles,
                    0
                );

            var freeSpaces =
                Math.Max(
                    occupancy.freeSpaces,
                    0
                );

            // --------------------------------------------
            // 6. Calculer le total
            // --------------------------------------------

            var totalPlaces =
                zones.Count;

            // Éviter que le VLM retourne
            // plus de véhicules que de zones.

            if (vehicles > totalPlaces)
            {
                vehicles = totalPlaces;
            }

            // Calculer les places libres à partir
            // du nombre réel de zones.

            freeSpaces =
                Math.Max(
                    totalPlaces - vehicles,
                    0
                );

            // --------------------------------------------
            // 7. Calculer le taux d'occupation
            // --------------------------------------------

            var tauxOccupation =
                totalPlaces > 0
                    ? Math.Round(
                        (double)vehicles /
                        totalPlaces *
                        100,
                        2
                    )
                    : 0;

            // --------------------------------------------
            // 8. Retourner le résultat
            // --------------------------------------------

            return Ok(new
            {
                parkingId,
                totalPlaces,
                vehicles,
                placesLibres = freeSpaces,
                tauxOccupation,
                mode = "vlm"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Erreur lors de l'analyse VLM.",
                    error = ex.Message
                }
            );
        }
    }
}