using LidomNet.Data;
using LidomNet.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LidomNet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadiosController : ControllerBase
    {
        private readonly LidomNetDbContext _context;

        private async Task<Estadio?> GetEstadioByName(string name) => await _context.Estadios.FirstOrDefaultAsync(e => e.Nombre == name);

        public EstadiosController(LidomNetDbContext context) => _context = context;

        [HttpGet("getAll")]
        public async Task<ActionResult> GetAll()
        {
            var estadios = await _context.Estadios.ToListAsync();
            return Ok(estadios);
        }

        [HttpGet("getOne/{name}")]
        public async Task<ActionResult> GetOne(string name)
        {
            var estadio = await GetEstadioByName(name);

            if(estadio == null)
            {
                return NotFound();
            }

            return Ok(estadio);
        }

        [HttpPost("createOne")]
        public async Task<ActionResult> CreateOne(Estadio estadio)
        {
            var newEstadio = await _context.Estadios.AddAsync(estadio);
            await _context.SaveChangesAsync();

            return Created();
        }

        [HttpPut("updateOne/{name}")]
        public async Task<ActionResult> UpdateOne(Estadio estadio, string name)
        {
            var estadioToUpdate = await GetEstadioByName(name);

            if(estadioToUpdate == null)
            {
                return NotFound();
            }

            estadioToUpdate.Nombre = estadio.Nombre;
            estadioToUpdate.Capacidad = estadio.Capacidad;
            estadioToUpdate.Ciudad = estadio.Ciudad;

            return Ok(estadioToUpdate);
        }

        [HttpDelete("deleteOne/{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            var estadio = await GetEstadioByName(name);

            if (estadio == null) return NotFound();

            _context.Estadios.Remove(estadio);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
