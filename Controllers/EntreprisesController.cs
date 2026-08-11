using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingManagement.API.Data;
using ParkingManagement.API.Models;

namespace ParkingManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EntreprisesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EntreprisesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. GET : api/Entreprises
    // Liste avec pagination + recherche
    [HttpGet]
    public async Task<IActionResult> GetEntreprises(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        var query = _context.Entreprises
            .Where(e => !e.IsDeleted)
            .AsQueryable();

        // Recherche par nom ou numéro fiscal
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e =>
                e.Nom.Contains(search) ||
                e.NumeroFiscal.Contains(search));
        }

        var total = await query.CountAsync();

        var entreprises = await query
            .OrderBy(e => e.Nom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling((double)total / pageSize),
            data = entreprises
        });
    }

    // 2. GET : api/Entreprises/1
    // Consulter une entreprise
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEntreprise(int id)
    {
        var entreprise = await _context.Entreprises
            .FirstOrDefaultAsync(e =>
                e.Id == id &&
                !e.IsDeleted);

        if (entreprise == null)
            return NotFound();

        return Ok(entreprise);
    }

    // 3. POST : api/Entreprises
    // Créer une entreprise
    [HttpPost]
    public async Task<IActionResult> CreateEntreprise(
        Entreprise entreprise)
    {
        entreprise.Id = 0;
        entreprise.IsDeleted = false;
        entreprise.CreatedAt = DateTime.UtcNow;

        _context.Entreprises.Add(entreprise);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEntreprise),
            new { id = entreprise.Id },
            entreprise);
    }

    // 4. PUT : api/Entreprises/1
    // Modifier une entreprise
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEntreprise(
        int id,
        Entreprise entreprise)
    {
        if (id != entreprise.Id)
            return BadRequest("L'identifiant ne correspond pas.");

        var existingEntreprise = await _context.Entreprises
            .FirstOrDefaultAsync(e =>
                e.Id == id &&
                !e.IsDeleted);

        if (existingEntreprise == null)
            return NotFound();

        existingEntreprise.Nom = entreprise.Nom;
        existingEntreprise.NumeroFiscal = entreprise.NumeroFiscal;
        existingEntreprise.Contact = entreprise.Contact;
        existingEntreprise.Email = entreprise.Email;
        existingEntreprise.Adresse = entreprise.Adresse;
        existingEntreprise.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // 5. DELETE : api/Entreprises/1
    // Soft-delete
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntreprise(int id)
    {
        var entreprise = await _context.Entreprises
            .FirstOrDefaultAsync(e =>
                e.Id == id &&
                !e.IsDeleted);

        if (entreprise == null)
            return NotFound();

        entreprise.IsDeleted = true;
        entreprise.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}