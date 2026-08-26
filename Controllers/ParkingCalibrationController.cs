using Microsoft.AspNetCore.Mvc;
using ParkingManagement.API.AI;
using ParkingManagement.API.Services;

namespace ParkingManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParkingCalibrationController : ControllerBase
{
    private readonly ParkingCalibrationService _calibrationService;

    public ParkingCalibrationController(
        ParkingCalibrationService calibrationService)
    {
        _calibrationService = calibrationService;
    }

    // ============================================
    // Récupérer les zones d'un parking
    // ============================================

    [HttpGet("{parkingId}")]
    public IActionResult GetZones(int parkingId)
    {
        var zones =
            _calibrationService.GetZones(parkingId);

        return Ok(zones);
    }

    // ============================================
    // Ajouter une zone à un parking
    // ============================================

    [HttpPost("{parkingId}")]
    public IActionResult AddZone(
        int parkingId,
        [FromBody] ParkingZone zone)
    {
        if (zone == null)
        {
            return BadRequest(
                "Les données de la zone sont obligatoires."
            );
        }

        if (zone.Largeur <= 0)
        {
            return BadRequest(
                "La largeur doit être supérieure à 0."
            );
        }

        if (zone.Hauteur <= 0)
        {
            return BadRequest(
                "La hauteur doit être supérieure à 0."
            );
        }

        var newZone =
            _calibrationService.AddZone(
                parkingId,
                zone
            );

        return Ok(newZone);
    }

    // ============================================
    // Supprimer les zones d'un parking
    // ============================================

    [HttpDelete("{parkingId}")]
    public IActionResult ClearZones(int parkingId)
    {
        _calibrationService.ClearZones(parkingId);

        return Ok(new
        {
            message =
                "Les zones du parking ont été supprimées."
        });
    }
}