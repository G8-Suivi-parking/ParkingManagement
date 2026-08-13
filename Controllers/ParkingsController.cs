using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingManagement.API.Data;
using ParkingManagement.API.Models;

namespace ParkingManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParkingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ParkingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // GET : api/Parkings
    // Liste paginée + recherche
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetParkings(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        var query = _context.Parkings
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        // Recherche par nom, code ou adresse
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(p =>
                p.Nom.Contains(search) ||
                p.Code.Contains(search) ||
                (p.Adresse != null &&
                 p.Adresse.Contains(search)));
        }

        var total = await query.CountAsync();

        var parkings = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Nom,
                p.Code,
                p.Adresse,
                p.Latitude,
                p.Longitude,
                p.HeureOuverture,
                p.HeureFermeture,
                p.Ouvert24h,
                p.ImageUrl,
                p.IsActive,
                p.CreatedAt,
                p.UpdatedAt,

                // Somme des capacités des zones
                CapaciteTotale = p.Zones
                    .Where(z => !z.IsDeleted)
                    .Sum(z => z.Capacite),

                // Nombre de zones
                NombreZones = p.Zones
                    .Count(z => !z.IsDeleted)
            })
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling(
                (double)total / pageSize),
            data = parkings
        });
    }


    // ============================================================
    // GET : api/Parkings/historique
    // Historique des suppressions
    // ============================================================

    [HttpGet("historique")]
    public async Task<IActionResult> GetHistorique()
    {
        var historique = await _context.ParkingHistoriques
            .OrderByDescending(h => h.DateAction)
            .Select(h => new
            {
                h.Id,
                h.ParkingId,
                h.NomParking,
                h.CodeParking,
                h.Action,
                h.Raison,
                h.DateAction
            })
            .ToListAsync();

        return Ok(historique);
    }


    // ============================================================
    // GET : api/Parkings/5
    // Détail d'un parking + zones + occupation
    // ============================================================

    [HttpGet("{id}")]
    public async Task<IActionResult> GetParking(int id)
    {
        var parkingExiste = await _context.Parkings
            .AnyAsync(p =>
                p.Id == id &&
                !p.IsDeleted);

        if (!parkingExiste)
        {
            return NotFound(new
            {
                message = "Parking introuvable."
            });
        }

        var parking = await _context.Parkings
            .Where(p =>
                p.Id == id &&
                !p.IsDeleted)
            .Select(p => new
            {
                p.Id,
                p.Nom,
                p.Code,
                p.Adresse,
                p.Latitude,
                p.Longitude,
                p.HeureOuverture,
                p.HeureFermeture,
                p.Ouvert24h,
                p.ImageUrl,
                p.IsActive,
                p.CreatedAt,
                p.UpdatedAt,

                // Capacité totale des zones actives
                CapaciteTotale = p.Zones
                    .Where(z =>
                        !z.IsDeleted &&
                        z.IsActive)
                    .Sum(z => z.Capacite),

                // Nombre de zones
                NombreZones = p.Zones
                    .Count(z => !z.IsDeleted),

                // Places actuellement occupées
                PlacesOccupees = _context.Acces
                    .Count(a =>
                        a.ParkingId == p.Id &&
                        a.EstPresent),

                // Zones
                Zones = p.Zones
                    .Where(z => !z.IsDeleted)
                    .Select(z => new
                    {
                        z.Id,
                        z.Nom,
                        z.Capacite,
                        z.Type,
                        z.IsActive,

                        // Places occupées dans la zone
                        PlacesOccupees = _context.Acces
                            .Count(a =>
                                a.ZoneId == z.Id &&
                                a.EstPresent),

                        // Places disponibles
                        PlacesDisponibles =
                            z.Capacite -
                            _context.Acces.Count(a =>
                                a.ZoneId == z.Id &&
                                a.EstPresent),

                        // Taux d'occupation de la zone
                        TauxOccupation =
                            z.Capacite > 0
                                ? Math.Round(
                                    (double)
                                    _context.Acces.Count(a =>
                                        a.ZoneId == z.Id &&
                                        a.EstPresent)
                                    / z.Capacite * 100,
                                    2)
                                : 0,

                        z.CreatedAt,
                        z.UpdatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (parking == null)
        {
            return NotFound(new
            {
                message = "Parking introuvable."
            });
        }

        // Places disponibles globalement
        var placesDisponibles =
            parking.CapaciteTotale -
            parking.PlacesOccupees;

        // Taux d'occupation global
        double tauxOccupation = 0;

        if (parking.CapaciteTotale > 0)
        {
            tauxOccupation =
                (double)parking.PlacesOccupees
                / parking.CapaciteTotale
                * 100;
        }

        return Ok(new
        {
            parking.Id,
            parking.Nom,
            parking.Code,
            parking.Adresse,
            parking.Latitude,
            parking.Longitude,
            parking.HeureOuverture,
            parking.HeureFermeture,
            parking.Ouvert24h,
            parking.ImageUrl,
            parking.IsActive,
            parking.CreatedAt,
            parking.UpdatedAt,

            parking.CapaciteTotale,
            parking.NombreZones,

            parking.PlacesOccupees,
            PlacesDisponibles = placesDisponibles,

            TauxOccupation = Math.Round(
                tauxOccupation,
                2),

            parking.Zones
        });
    }


    // ============================================================
    // POST : api/Parkings
    // Création d'un parking avec au moins une zone
    // ============================================================

    [HttpPost]
    public async Task<IActionResult> CreateParking(
        Parking parking)
    {
        // Vérifier le nom
        if (string.IsNullOrWhiteSpace(parking.Nom))
        {
            return BadRequest(new
            {
                message =
                    "Le nom du parking est obligatoire."
            });
        }

        // Vérifier le code
        if (string.IsNullOrWhiteSpace(parking.Code))
        {
            return BadRequest(new
            {
                message =
                    "Le code du parking est obligatoire."
            });
        }

        parking.Code = parking.Code.Trim();

        // Un parking doit avoir au moins une zone
        if (parking.Zones == null ||
            !parking.Zones.Any())
        {
            return BadRequest(new
            {
                message =
                    "Un parking doit avoir au moins une zone."
            });
        }

        // Vérifier les zones
        foreach (var zone in parking.Zones)
        {
            if (string.IsNullOrWhiteSpace(zone.Nom))
            {
                return BadRequest(new
                {
                    message =
                        "Le nom de chaque zone est obligatoire."
                });
            }

            if (zone.Capacite <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "La capacité de chaque zone " +
                        "doit être supérieure à 0."
                });
            }

            if (string.IsNullOrWhiteSpace(zone.Type))
            {
                return BadRequest(new
                {
                    message =
                        "Le type de chaque zone est obligatoire."
                });
            }
        }

        // Vérifier l'unicité du code
        var codeExiste = await _context.Parkings
            .AnyAsync(p =>
                p.Code == parking.Code &&
                !p.IsDeleted);

        if (codeExiste)
        {
            return Conflict(new
            {
                message =
                    "Un parking avec ce code existe déjà."
            });
        }

        // Initialiser le parking
        parking.Id = 0;
        parking.IsDeleted = false;
        parking.CreatedAt = DateTime.UtcNow;
        parking.UpdatedAt = null;

        // Initialiser les zones
        foreach (var zone in parking.Zones)
        {
            zone.Id = 0;
            zone.IsDeleted = false;
            zone.IsActive = true;
            zone.CreatedAt = DateTime.UtcNow;
            zone.UpdatedAt = null;

            // EF Core gère automatiquement
            // la relation Parking -> Zones.
        }

        _context.Parkings.Add(parking);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetParking),
            new { id = parking.Id },
            parking);
    }


    // ============================================================
    // PUT : api/Parkings/5
    // Modification d'un parking
    // ============================================================

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateParking(
        int id,
        Parking parking)
    {
        // Chercher le parking
        var existingParking = await _context.Parkings
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                !p.IsDeleted);

        if (existingParking == null)
        {
            return NotFound(new
            {
                message =
                    "Parking introuvable."
            });
        }

        // Vérifier le nom
        if (string.IsNullOrWhiteSpace(parking.Nom))
        {
            return BadRequest(new
            {
                message =
                    "Le nom du parking est obligatoire."
            });
        }

        // Vérifier le code
        if (string.IsNullOrWhiteSpace(parking.Code))
        {
            return BadRequest(new
            {
                message =
                    "Le code du parking est obligatoire."
            });
        }

        parking.Code = parking.Code.Trim();

        // Vérifier l'unicité du code
        var codeExiste = await _context.Parkings
            .AnyAsync(p =>
                p.Id != id &&
                p.Code == parking.Code &&
                !p.IsDeleted);

        if (codeExiste)
        {
            return Conflict(new
            {
                message =
                    "Un autre parking utilise déjà ce code."
            });
        }

        // Modifier les informations générales
        existingParking.Nom = parking.Nom;
        existingParking.Code = parking.Code;
        existingParking.Adresse = parking.Adresse;
        existingParking.Latitude = parking.Latitude;
        existingParking.Longitude = parking.Longitude;
        existingParking.HeureOuverture =
            parking.HeureOuverture;
        existingParking.HeureFermeture =
            parking.HeureFermeture;
        existingParking.Ouvert24h =
            parking.Ouvert24h;
        existingParking.ImageUrl =
            parking.ImageUrl;
        existingParking.IsActive =
            parking.IsActive;
        existingParking.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(existingParking);
    }


    // ============================================================
    // DELETE : api/Parkings/5
    // Suppression logique + journalisation
    // ============================================================

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteParking(int id)
    {
        var parking = await _context.Parkings
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                !p.IsDeleted);

        if (parking == null)
        {
            return NotFound(new
            {
                message =
                    "Parking introuvable."
            });
        }

        // Vérifier les accès récents
        // Règle : accès des dernières 24 heures
        var dateLimite =
            DateTime.UtcNow.AddHours(-24);

        var accesRecent = await _context.Acces
            .AnyAsync(a =>
                a.ParkingId == id &&
                a.DateEntree >= dateLimite);

        if (accesRecent)
        {
            return Conflict(new
            {
                message =
                    "Impossible de supprimer ce parking : " +
                    "des accès récents sont enregistrés."
            });
        }

        // ========================================================
        // Journalisation
        // ========================================================

        var historique = new ParkingHistorique
        {
            ParkingId = parking.Id,
            NomParking = parking.Nom,
            CodeParking = parking.Code,
            Action = "SUPPRESSION",
            Raison =
                "Suppression logique du parking",
            DateAction = DateTime.UtcNow
        };

        _context.ParkingHistoriques
            .Add(historique);

        // ========================================================
        // Suppression logique
        // ========================================================

        parking.IsDeleted = true;
        parking.IsActive = false;
        parking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Parking supprimé avec succès.",
            parkingId = parking.Id,
            historiqueId = historique.Id
        });
    }
}