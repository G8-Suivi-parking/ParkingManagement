using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingManagement.API.Data;
using ParkingManagement.API.Models;

namespace ParkingManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ZonesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ZonesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Zones/parking/1
    [HttpGet("parking/{parkingId}")]
    public async Task<IActionResult> GetZones(int parkingId)
    {
        var parkingExiste = await _context.Parkings
            .AnyAsync(p => p.Id == parkingId && !p.IsDeleted);

        if (!parkingExiste)
        {
            return NotFound(new
            {
                message = "Parking introuvable."
            });
        }

        var zones = await _context.Zones
            .Where(z =>
                z.ParkingId == parkingId &&
                !z.IsDeleted)
            .OrderBy(z => z.Nom)
            .Select(z => new
            {
                z.Id,
                z.Nom,
                z.Capacite,
                z.Type,
                z.IsActive,
                z.CreatedAt,
                z.UpdatedAt,
                z.ParkingId
            })
            .ToListAsync();

        return Ok(zones);
    }

    // POST: api/Zones/parking/1
    [HttpPost("parking/{parkingId}")]
    public async Task<IActionResult> CreateZone(
        int parkingId,
        Zone zone)
    {
        var parking = await _context.Parkings
            .FirstOrDefaultAsync(p =>
                p.Id == parkingId &&
                !p.IsDeleted);

        if (parking == null)
        {
            return NotFound(new
            {
                message = "Parking introuvable."
            });
        }

        if (string.IsNullOrWhiteSpace(zone.Nom))
        {
            return BadRequest(new
            {
                message = "Le nom de la zone est obligatoire."
            });
        }

        if (zone.Capacite <= 0)
        {
            return BadRequest(new
            {
                message = "La capacité doit être supérieure à 0."
            });
        }

        if (string.IsNullOrWhiteSpace(zone.Type))
        {
            return BadRequest(new
            {
                message = "Le type de zone est obligatoire."
            });
        }

        zone.Id = 0;
        zone.ParkingId = parkingId;
        zone.IsDeleted = false;
        zone.CreatedAt = DateTime.UtcNow;
        zone.UpdatedAt = null;

        _context.Zones.Add(zone);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetZones),
            new { parkingId = parkingId },
            zone);
    }

    // PUT: api/Zones/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateZone(
        int id,
        Zone zone)
    {
        // 1. Chercher la zone existante
        var existingZone = await _context.Zones
            .FirstOrDefaultAsync(z =>
                z.Id == id &&
                !z.IsDeleted);

        if (existingZone == null)
        {
            return NotFound(new
            {
                message = "Zone introuvable."
            });
        }

        // 2. Vérifier les données
        if (string.IsNullOrWhiteSpace(zone.Nom))
        {
            return BadRequest(new
            {
                message = "Le nom de la zone est obligatoire."
            });
        }

        if (zone.Capacite <= 0)
        {
            return BadRequest(new
            {
                message = "La capacité doit être supérieure à 0."
            });
        }

        if (string.IsNullOrWhiteSpace(zone.Type))
        {
            return BadRequest(new
            {
                message = "Le type de zone est obligatoire."
            });
        }

        // 3. Empêcher la désactivation de la dernière zone active
        if (!zone.IsActive && existingZone.IsActive)
        {
            var nombreZonesActives = await _context.Zones
                .CountAsync(z =>
                    z.ParkingId == existingZone.ParkingId &&
                    !z.IsDeleted &&
                    z.IsActive);

            if (nombreZonesActives <= 1)
            {
                return BadRequest(new
                {
                    message = "Impossible de désactiver cette zone. " +
                              "Le parking doit conserver au moins une zone active."
                });
            }
        }

        // 4. Modifier la zone
        existingZone.Nom = zone.Nom;
        existingZone.Capacite = zone.Capacite;
        existingZone.Type = zone.Type;
        existingZone.IsActive = zone.IsActive;
        existingZone.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(existingZone);
    }

    // DELETE: api/Zones/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteZone(int id)
    {
        // 1. Chercher la zone
        var zone = await _context.Zones
            .FirstOrDefaultAsync(z =>
                z.Id == id &&
                !z.IsDeleted);

        if (zone == null)
        {
            return NotFound(new
            {
                message = "Zone introuvable."
            });
        }

        // 2. Compter les zones du parking
        var nombreZones = await _context.Zones
            .CountAsync(z =>
                z.ParkingId == zone.ParkingId &&
                !z.IsDeleted);

        // 3. Un parking doit conserver au moins une zone
        if (nombreZones <= 1)
        {
            return BadRequest(new
            {
                message = "Impossible de supprimer cette zone. " +
                          "Un parking doit avoir au moins une zone."
            });
        }

        // 4. Suppression logique
        zone.IsDeleted = true;
        zone.IsActive = false;
        zone.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Zone supprimée avec succès."
        });
    }
}