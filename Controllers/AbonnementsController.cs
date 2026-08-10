using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingManagement.API.Data;
using ParkingManagement.API.Models;

namespace ParkingManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AbonnementsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AbonnementsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET : api/Abonnements
    // Liste de tous les abonnements

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Abonnement>>> GetAbonnements()
    {
        var abonnements = await _context.Abonnements
            .ToListAsync();

        return Ok(abonnements);
    }

    // GET : api/Abonnements/{id}
    // Consulter un abonnement
   
    [HttpGet("{id}")]
    public async Task<ActionResult<Abonnement>> GetAbonnement(int id)
    {
        var abonnement = await _context.Abonnements
            .FindAsync(id);

        if (abonnement == null)
        {
            return NotFound();
        }

        return Ok(abonnement);
    }

    // POST : api/Abonnements
    // Créer un abonnement
  
    [HttpPost]
    public async Task<ActionResult<Abonnement>> CreateAbonnement(
        Abonnement abonnement)
    {
        // Vérifier si l'entreprise possède déjà
        // un abonnement actif
        var abonnementActif = await _context.Abonnements
            .AnyAsync(a =>
                a.EntrepriseId == abonnement.EntrepriseId &&
                a.Statut == "ACTIVE");

        if (abonnementActif)
        {
            return BadRequest(
                "Cette entreprise possède déjà un abonnement actif.");
        }

        // Si aucun abonnement actif n'existe,
        // on crée le nouvel abonnement
        abonnement.Statut = "ACTIVE";

        _context.Abonnements.Add(abonnement);

        await _context.SaveChangesAsync();

        // Ajouter l'action dans l'historique
        var historique = new AbonnementHistorique
        {
            AbonnementId = abonnement.Id,
            Action = "CREATION",
            DateAction = DateTime.UtcNow,
            Details = "Création de l'abonnement"
        };

        _context.AbonnementHistoriques.Add(historique);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetAbonnement),
            new { id = abonnement.Id },
            abonnement);
    }

    
    // PUT : api/Abonnements/{id}/renew
    // Renouveler un abonnement

    [HttpPut("{id}/renew")]
    public async Task<ActionResult<Abonnement>> RenewAbonnement(
        int id,
        DateTime nouvelleDateFin)
    {
        var abonnement = await _context.Abonnements
            .FindAsync(id);

        if (abonnement == null)
        {
            return NotFound();
        }

        // Vérifier que la nouvelle date est valide
        if (nouvelleDateFin <= abonnement.DateFin)
        {
            return BadRequest(
                "La nouvelle date de fin doit être supérieure à l'ancienne.");
        }

        // Modifier l'abonnement
        abonnement.DateFin = nouvelleDateFin;
        abonnement.Statut = "ACTIVE";

        await _context.SaveChangesAsync();

        // Ajouter l'action dans l'historique
        var historique = new AbonnementHistorique
        {
            AbonnementId = abonnement.Id,
            Action = "RENOUVELLEMENT",
            DateAction = DateTime.UtcNow,
            Details = "Abonnement renouvelé jusqu'au "
                      + nouvelleDateFin.ToString("yyyy-MM-dd")
        };

        _context.AbonnementHistoriques.Add(historique);

        await _context.SaveChangesAsync();

        return Ok(abonnement);
    }

 
    // PUT : api/Abonnements/{id}/suspend
    // Suspendre un abonnement avec une raison
    [HttpPut("{id}/suspend")]
    public async Task<ActionResult<Abonnement>> SuspendAbonnement(
        int id,
        string raison)
    {
        var abonnement = await _context.Abonnements
            .FindAsync(id);

        if (abonnement == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(raison))
        {
            return BadRequest(
                "La raison de la suspension est obligatoire.");
        }

        // Modifier le statut
        abonnement.Statut = "SUSPENDED";
        abonnement.Raison = raison;

        await _context.SaveChangesAsync();

        // Ajouter l'action dans l'historique
        var historique = new AbonnementHistorique
        {
            AbonnementId = abonnement.Id,
            Action = "SUSPENSION",
            DateAction = DateTime.UtcNow,
            Details = raison
        };

        _context.AbonnementHistoriques.Add(historique);

        await _context.SaveChangesAsync();

        return Ok(abonnement);
    }

    // PUT : api/Abonnements/{id}/terminate
    // Terminer / résilier un abonnement avec une raison
 
    [HttpPut("{id}/terminate")]
    public async Task<ActionResult<Abonnement>> TerminateAbonnement(
        int id,
        string raison)
    {
        var abonnement = await _context.Abonnements
            .FindAsync(id);

        if (abonnement == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(raison))
        {
            return BadRequest(
                "La raison de la résiliation est obligatoire.");
        }

        // Modifier le statut
        abonnement.Statut = "TERMINATED";
        abonnement.Raison = raison;

        await _context.SaveChangesAsync();

        // Ajouter l'action dans l'historique
        var historique = new AbonnementHistorique
        {
            AbonnementId = abonnement.Id,
            Action = "RESILIATION",
            DateAction = DateTime.UtcNow,
            Details = raison
        };

        _context.AbonnementHistoriques.Add(historique);

        await _context.SaveChangesAsync();

        return Ok(abonnement);
    }

    // GET : api/Abonnements/{id}/historique
    // Consulter l'historique d'un abonnement
   
    [HttpGet("{id}/historique")]
    public async Task<ActionResult<IEnumerable<AbonnementHistorique>>>
        GetHistorique(int id)
    {
        // Vérifier que l'abonnement existe
        var abonnement = await _context.Abonnements
            .FindAsync(id);

        if (abonnement == null)
        {
            return NotFound();
        }

        // Récupérer l'historique
        var historique = await _context.AbonnementHistoriques
            .Where(h => h.AbonnementId == id)
            .OrderByDescending(h => h.DateAction)
            .ToListAsync();

        return Ok(historique);
    }
}