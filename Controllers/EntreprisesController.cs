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

    // GET: api/Entreprises
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Entreprise>>> GetEntreprises()
    {
        return await _context.Entreprises.ToListAsync();
    }

    // GET: api/Entreprises/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Entreprise>> GetEntreprise(int id)
    {
        var entreprise = await _context.Entreprises.FindAsync(id);

        if (entreprise == null)
        {
            return NotFound();
        }

        return Ok(entreprise);
    }

    // POST: api/Entreprises
    [HttpPost]
    public async Task<ActionResult<Entreprise>> CreateEntreprise(
        Entreprise entreprise)
    {
        _context.Entreprises.Add(entreprise);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEntreprise),
            new { id = entreprise.Id },
            entreprise);
    }
}