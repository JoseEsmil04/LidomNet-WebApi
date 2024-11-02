using LidomNet.Data;
using LidomNet.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LidomNet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EquiposController : ControllerBase
    {
        private readonly LidomNetDbContext _context;

        // Inyeccion de dependencias
        public EquiposController(LidomNetDbContext context)
        {
            _context = context;
        }

        [HttpGet("getAll")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Equipo>>> GetTeams()
        {
            var teams = await _context.Equipos.ToListAsync();

            return Ok(teams);
        }

        [HttpGet("getOne/{name}")]
        public async Task<IActionResult> GetTeam([FromQuery] string name)
        {
            var equipo = await _context.Equipos.FirstOrDefaultAsync(e => e.Nombre == name);

            equipo.Jugadores = await _context.Jugadores.ToListAsync()!;

            if(equipo == null)
            {
                return NotFound();
            }

            return Ok(equipo);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTeam([FromBody] Equipo equipo)
        {
            _context.Equipos.Add(equipo);
            await _context.SaveChangesAsync();

            return Created();
        }

        [HttpPut("update/{name}")]
        public async Task<IActionResult> UpdateTeam(string name, [FromBody] Equipo equipo)
        {
            if (name != equipo.Nombre)
            {
                return BadRequest("El ID del equipo no coincide");
            }

            var existingTeam = await _context.Equipos.FindAsync(name);
            if (existingTeam == null)
            {
                return NotFound("Equipo no encontrado");
            }

            existingTeam.Nombre = equipo.Nombre;
            existingTeam.Ciudad = equipo.Ciudad;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("remove/{name}")]
        public async Task<IActionResult> RemoveTeam(string name)
        {
            var teamToRemove = await _context.Equipos.FindAsync(name);

            if (teamToRemove == null) return NotFound("Equipo no encontrado");

            _context.Equipos.Remove(teamToRemove!);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
