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
    public class JugadoresController : ControllerBase
    {
        private readonly LidomNetDbContext _context;
        public JugadoresController(LidomNetDbContext context)
        {
            _context = context;
        }

        private async Task<Jugador?> GetJugadorByNameAsync(string name)
            => await _context.Jugadores.FirstOrDefaultAsync(j => j.Name == name);

        [HttpGet("getAll")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Jugador>>> GetAll()
        {
            var jugadores = await _context.Jugadores.ToListAsync();

            return Ok(jugadores);
        }

        [HttpGet("getOne/{name}")]
        public async Task<IActionResult> GetPlayer(string name)
        {
            var jugador = await GetJugadorByNameAsync(name);

            if (jugador == null)
            {
                return NotFound();
            }

            return Ok(jugador);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePlayer([FromBody] Jugador jugador)
        {
            var equipo = await _context.Equipos.FindAsync(jugador.EquipoId);
            if (equipo == null)
            {
                return BadRequest("El equipo asignado no existe.");
            }

            var existeJugador = await GetJugadorByNameAsync(jugador.Name);

            if (existeJugador != null) return BadRequest(new { Error = "El jugador ya existe" });

            _context.Jugadores.Add(jugador);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("updateOne/{name}")]
        public async Task<IActionResult> Update(Jugador jugador, string name)
        {
            var findJugador = await GetJugadorByNameAsync(name);

            if (findJugador == null)
            {
                return NotFound();
            }

            findJugador.Name = jugador.Name;
            findJugador.Equipo = jugador.Equipo;

            return Ok(findJugador);
        }

        [HttpDelete("deleteOne/{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            var jugador = await GetJugadorByNameAsync(name);

            if (jugador == null) return NotFound();

            _context.Jugadores.Remove(jugador);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
