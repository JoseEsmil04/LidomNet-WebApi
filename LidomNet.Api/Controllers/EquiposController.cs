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
            var team = await _context.Equipos.FirstOrDefaultAsync(e => e.Nombre == name);

            if(team == null)
            {
                return NotFound();
            }

            return Ok(team);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTeam(Equipo equipo)
        {
            var newTeam = await _context.Equipos.AddAsync(equipo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTeam), new { name = equipo.Nombre }, equipo);
        }

        [HttpPut("update/{name}")]
        public async Task<IActionResult> UpdateTeam(string name, [FromBody] Equipo equipo)
        {
            // Verifica que el ID del equipo en la URL coincida con el ID del equipo en el cuerpo de la solicitud
            if (name != equipo.Nombre)
            {
                return BadRequest("El ID del equipo no coincide");
            }

            // Busca el equipo en la base de datos
            var existingTeam = await _context.Equipos.FindAsync(name);
            if (existingTeam == null)
            {
                return NotFound("Equipo no encontrado");
            }

            // Actualiza las propiedades del equipo
            existingTeam.Nombre = equipo.Nombre;
            existingTeam.Ciudad = equipo.Ciudad;
            // Actualiza las demás propiedades que necesites

            // Guarda los cambios
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content, porque no devolvemos contenido, solo confirmamos que fue exitoso
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
