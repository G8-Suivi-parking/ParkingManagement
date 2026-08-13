using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingManagement.API.Data;
using ParkingManagement.API.Models;

namespace ParkingManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AccesController(ApplicationDbContext context)
    {
        _context = context;
    }


    // ============================================================
    // GET : api/Acces/parking/1
    // Consulter les accès d'un parking
    // ============================================================
    [HttpGet("parking/{parkingId}")]
    public async Task<IActionResult> GetAccesParking(int parkingId)
    {
        var parkingExiste = await _context.Parkings
            .AnyAsync(p =>
                p.Id == parkingId &&
                !p.IsDeleted);

        if (!parkingExiste)
        {
            return NotFound(new
            {
                message = "Parking introuvable."
            });
        }

        var acces = await _context.Acces
            .Where(a => a.ParkingId == parkingId)
            .OrderByDescending(a => a.DateEntree)
            .Select(a => new
            {
                a.Id,
                a.ParkingId,
                a.ZoneId,
                a.Immatriculation,
                a.DateEntree,
                a.DateSortie,
                a.EstPresent,
                a.CreatedAt,
                a.UpdatedAt
            })
            .ToListAsync();

        return Ok(acces);
    }


    // ============================================================
    // GET : api/Acces/parking/1/presents
    // Véhicules actuellement présents
    // ============================================================
    [HttpGet("parking/{parkingId}/presents")]
    public async Task<IActionResult> GetVehiculesPresents(int parkingId)
    {
        var parkingExiste = await _context.Parkings
            .AnyAsync(p =>
                p.Id == parkingId &&
                !p.IsDeleted);

        if (!parkingExiste)
        {
            return NotFound(new
            {
                message = "Parking introuvable."
            });
        }

        var acces = await _context.Acces
            .Where(a =>
                a.ParkingId == parkingId &&
                a.EstPresent)
            .OrderBy(a => a.DateEntree)
            .Select(a => new
            {
                a.Id,
                a.ZoneId,
                a.Immatriculation,
                a.DateEntree,
                a.EstPresent
            })
            .ToListAsync();

        return Ok(acces);
    }


    // ============================================================
    // POST : api/Acces/entree
    // Enregistrer une entrée
    // ============================================================
    [HttpPost("entree")]
    public async Task<IActionResult> EnregistrerEntree(Acces acces)
    {
        // Vérifier l'immatriculation
        if (string.IsNullOrWhiteSpace(acces.Immatriculation))
        {
            return BadRequest(new
            {
                message = "L'immatriculation est obligatoire."
            });
        }

        acces.Immatriculation =
            acces.Immatriculation.Trim().ToUpper();

        // Vérifier le parking
        var parking = await _context.Parkings
            .FirstOrDefaultAsync(p =>
                p.Id == acces.ParkingId &&
                !p.IsDeleted &&
                p.IsActive);

        if (parking == null)
        {
            return NotFound(new
            {
                message = "Parking introuvable ou inactif."
            });
        }

        // Vérifier la zone
        var zone = await _context.Zones
            .FirstOrDefaultAsync(z =>
                z.Id == acces.ZoneId &&
                z.ParkingId == acces.ParkingId &&
                !z.IsDeleted &&
                z.IsActive);

        if (zone == null)
        {
            return NotFound(new
            {
                message = "Zone introuvable ou inactive."
            });
        }

        // Vérifier si le véhicule est déjà présent
        var dejaPresent = await _context.Acces
            .AnyAsync(a =>
                a.Immatriculation == acces.Immatriculation &&
                a.EstPresent);

        if (dejaPresent)
        {
            return Conflict(new
            {
                message =
                    "Ce véhicule est déjà présent dans un parking."
            });
        }

        // Vérifier la capacité de la zone
        var nombreVehiculesZone = await _context.Acces
            .CountAsync(a =>
                a.ZoneId == acces.ZoneId &&
                a.EstPresent);

        if (nombreVehiculesZone >= zone.Capacite)
        {
            return Conflict(new
            {
                message =
                    "Cette zone est complète. Aucune place disponible."
            });
        }

        // Création de l'accès
        acces.Id = 0;
        acces.DateEntree = DateTime.UtcNow;
        acces.DateSortie = null;
        acces.EstPresent = true;
        acces.CreatedAt = DateTime.UtcNow;
        acces.UpdatedAt = null;

        _context.Acces.Add(acces);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetAcces),
            new { id = acces.Id },
            acces);
    }


    // ============================================================
    // GET : api/Acces/5
    // Consulter un accès
    // ============================================================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAcces(int id)
    {
        var acces = await _context.Acces
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                a.ParkingId,
                a.ZoneId,
                a.Immatriculation,
                a.DateEntree,
                a.DateSortie,
                a.EstPresent,
                a.CreatedAt,
                a.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (acces == null)
        {
            return NotFound(new
            {
                message = "Accès introuvable."
            });
        }

        return Ok(acces);
    }


    // ============================================================
    // PUT : api/Acces/5/sortie
    // Enregistrer la sortie d'un véhicule
    // ============================================================
    [HttpPut("{id}/sortie")]
    public async Task<IActionResult> EnregistrerSortie(int id)
    {
        var acces = await _context.Acces
            .FirstOrDefaultAsync(a =>
                a.Id == id &&
                a.EstPresent);

        if (acces == null)
        {
            return NotFound(new
            {
                message =
                    "Accès introuvable ou véhicule déjà sorti."
            });
        }

        acces.DateSortie = DateTime.UtcNow;
        acces.EstPresent = false;
        acces.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Sortie enregistrée avec succès.",
            acces.Id,
            acces.Immatriculation,
            acces.DateEntree,
            acces.DateSortie,
            acces.EstPresent
        });
    }


    // ============================================================
    // GET : api/Acces/parking/1/occupation
    // Occupation actuelle d'un parking
    // ============================================================
    [HttpGet("parking/{parkingId}/occupation")]
    public async Task<IActionResult> GetOccupation(int parkingId)
    {
        var parking = await _context.Parkings
            .Where(p =>
                p.Id == parkingId &&
                !p.IsDeleted)
            .Select(p => new
            {
                p.Id,
                p.Nom,

                CapaciteTotale = p.Zones
                    .Where(z => !z.IsDeleted && z.IsActive)
                    .Sum(z => z.Capacite)
            })
            .FirstOrDefaultAsync();

        if (parking == null)
        {
            return NotFound(new
            {
                message = "Parking introuvable."
            });
        }

        var placesOccupees = await _context.Acces
            .CountAsync(a =>
                a.ParkingId == parkingId &&
                a.EstPresent);

        var placesDisponibles =
            parking.CapaciteTotale - placesOccupees;

        double tauxOccupation = 0;

        if (parking.CapaciteTotale > 0)
        {
            tauxOccupation =
                (double)placesOccupees /
                parking.CapaciteTotale *
                100;
        }

        return Ok(new
        {
            parkingId = parking.Id,
            parking.Nom,
            capaciteTotale = parking.CapaciteTotale,
            placesOccupees,
            placesDisponibles,
            tauxOccupation = Math.Round(tauxOccupation, 2)
        });
    }


    // ============================================================
    // GET : api/Acces/parking/1/occupation-zones
    // Occupation par zone
    // ============================================================
    [HttpGet("parking/{parkingId}/occupation-zones")]
    public async Task<IActionResult> GetOccupationZones(int parkingId)
    {
        var parkingExiste = await _context.Parkings
            .AnyAsync(p =>
                p.Id == parkingId &&
                !p.IsDeleted);

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
            .Select(z => new
            {
                z.Id,
                z.Nom,
                z.Type,
                z.Capacite,

                PlacesOccupees = _context.Acces
                    .Count(a =>
                        a.ZoneId == z.Id &&
                        a.EstPresent)
            })
            .ToListAsync();

        var resultat = zones
            .Select(z => new
            {
                z.Id,
                z.Nom,
                z.Type,
                z.Capacite,
                z.PlacesOccupees,
                PlacesDisponibles =
                    z.Capacite - z.PlacesOccupees,

                TauxOccupation =
                    z.Capacite > 0
                        ? Math.Round(
                            (double)z.PlacesOccupees /
                            z.Capacite * 100,
                            2)
                        : 0
            })
            .ToList();

        return Ok(resultat);
    }
}