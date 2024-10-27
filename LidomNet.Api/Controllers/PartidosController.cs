//using LidomNet.Data;
//using LidomNet.Domain;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace LidomNet.Api.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PartidosController : ControllerBase
//    {
//        private readonly LidomNetDbContext _context;

//        public PartidosController(LidomNetDbContext context)
//        {
//            _context = context;
//        }

//        // GET: api/Partidos/getAll
//        [HttpGet("getAll")]
//        public async Task<ActionResult<IEnumerable<Partido>>> GetAll()
//        {
//            var partidos = await _context.Partidos
//                .Include(p => p.EquipoLocal) // Incluye la relación con EquipoLocal
//                .Include(p => p.EquipoVisitante) // Incluye la relación con EquipoVisitante
//                .Include(p => p.Estadio) // Incluye la relación con Estadio
//                .ToListAsync();

//            return Ok(partidos);
//        }

//        // GET: api/Partidos/getOne/{id}
//        [HttpGet("getOne/{id}")]
//        public async Task<IActionResult> GetOne(Guid id)
//        {
//            var partido = await _context.Partidos
//                .Include(p => p.EquipoLocal)
//                .Include(p => p.EquipoVisitante)
//                .Include(p => p.Estadio)
//                .FirstOrDefaultAsync(p => p.Id == id);

//            if (partido == null)
//            {
//                return NotFound();
//            }

//            return Ok(partido);
//        }

//        // POST: api/Partidos/createOne
//        [HttpPost("createOne")]
//        public async Task<IActionResult> Create(Partido partido)
//        {
//            var equipoLocal = await _context.Equipos.FindAsync(partido.EquipoLocalId);
//            var equipoVisitante = await _context.Equipos.FindAsync(partido.EquipoVisitanteId);
//            var estadio = await _context.Estadios.FindAsync(partido.EstadioId);

//            if (equipoLocal == null || equipoVisitante == null || estadio == null)
//            {
//                return BadRequest("Uno o más IDs de equipo o estadio no son válidos.");
//            }

//            await _context.Partidos.AddAsync(partido);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetOne), new { id = partido.Id }, partido);
//        }

//        // PUT: api/Partidos/updateOne/{id}
//        [HttpPut("updateOne/{id}")]
//        public async Task<IActionResult> Update(Guid id, Partido partido)
//        {
//            if (id != partido.Id)
//            {
//                return BadRequest("El ID del partido no coincide.");
//            }

//            var existingPartido = await _context.Partidos.FindAsync(id);
//            if (existingPartido == null)
//            {
//                return NotFound();
//            }

//            existingPartido.Fecha = partido.Fecha;
//            existingPartido.EquipoLocalId = partido.EquipoLocalId;
//            existingPartido.EquipoVisitanteId = partido.EquipoVisitanteId;
//            existingPartido.EstadioId = partido.EstadioId;
//            existingPartido.MarcadorLocal = partido.MarcadorLocal;
//            existingPartido.MarcadorVisitante = partido.MarcadorVisitante;

//            await _context.SaveChangesAsync();

//            return NoContent();
//        }

//        // DELETE: api/Partidos/deleteOne/{id}
//        [HttpDelete("deleteOne/{id}")]
//        public async Task<IActionResult> Delete(Guid id)
//        {
//            var partido = await _context.Partidos.FindAsync(id);

//            if (partido == null)
//            {
//                return NotFound();
//            }

//            _context.Partidos.Remove(partido);
//            await _context.SaveChangesAsync();

//            return Ok();
//        }
//    }
//}
