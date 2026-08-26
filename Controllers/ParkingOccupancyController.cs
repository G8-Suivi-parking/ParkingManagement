using Microsoft.AspNetCore.Mvc;
using ParkingManagement.API.Services;

namespace ParkingManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParkingOccupancyController : ControllerBase
{
    private readonly ParkingOccupancyService _occupancyService;
    private readonly ParkingImageService _imageService;
    private readonly IConfiguration _configuration;

    public ParkingOccupancyController(
        ParkingOccupancyService occupancyService,
        ParkingImageService imageService,
        IConfiguration configuration)
    {
        _occupancyService = occupancyService;
        _imageService = imageService;
        _configuration = configuration;
    }

    // =====================================================
    // Analyser l'occupation d'un parking avec le VLM
    // =====================================================

    [HttpGet("analyze/{parkingId}")]
    public async Task<IActionResult> Analyze(int parkingId)
    {
        try
        {
            // ---------------------------------------------
            // 1. Récupérer l'URL de l'image
            // ---------------------------------------------

            var imageUrl =
                _configuration["ParkingImageApi:ImageUrl"];

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return BadRequest(new
                {
                    message =
                        "L'URL de l'image n'est pas configurée."
                });
            }

            // ---------------------------------------------
            // 2. Récupérer l'image
            // ---------------------------------------------

            var image =
                await _imageService.GetParkingImageAsync(
                    imageUrl
                );

            if (image == null || image.Length == 0)
            {
                return BadRequest(new
                {
                    message =
                        "Impossible de récupérer l'image du parking."
                });
            }

            // ---------------------------------------------
            // 3. Vérifier que l'image est valide
            // ---------------------------------------------

            if (!_imageService.IsImageValid(image))
            {
                return BadRequest(new
                {
                    message =
                        "L'image récupérée est invalide."
                });
            }

            // ---------------------------------------------
            // 4. Analyse VLM
            // ---------------------------------------------

            var result =
                await _occupancyService.AnalyzeParkingAsync(
                    parkingId,
                    image
                );

            // ---------------------------------------------
            // 5. Retourner le résultat
            // ---------------------------------------------

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Une erreur est survenue lors de l'analyse du parking.",

                    error = ex.Message
                }
            );
        }
    }
    [HttpPost("analyze-all")]
public async Task<IActionResult> AnalyzeAll()
{
    try
    {
        var imageUrl =
            _configuration["ParkingImageApi:ImageUrl"];

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return BadRequest(new
            {
                message = "L'URL de l'image n'est pas configurée."
            });
        }

        var image =
            await _imageService.GetParkingImageAsync(imageUrl);

        if (image == null || image.Length == 0)
        {
            return BadRequest(new
            {
                message = "Impossible de récupérer l'image."
            });
        }

        if (!_imageService.IsImageValid(image))
        {
            return BadRequest(new
            {
                message = "L'image récupérée est invalide."
            });
        }

        var results = await _occupancyService.AnalyzeAllParkingsAsync(image);

        return Ok(results);
    }
    catch (Exception ex)
    {
        return StatusCode(
            StatusCodes.Status500InternalServerError,
            new
            {
                message = "Erreur lors de l'analyse des parkings.",
                error = ex.Message
            });
    }
}
}