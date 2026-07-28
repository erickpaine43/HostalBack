using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VistaAzul.Modelos;
using VistaAzul.Dto;

namespace VistaAzul.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AmasDeLlavesController : ControllerBase
    {
        private readonly VistaAzulDbContext _context;

        public AmasDeLlavesController(VistaAzulDbContext context)
        {
            _context = context;
        }

        // GET: api/AmasDeLlaves
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AmaDeLlavesDetalleDto>>> GetAmasDeLlaves()
        {
            var amas = await _context.AmasDeLlaves
                .Include(a => a.Habitaciones)
                .Select(a => new AmaDeLlavesDetalleDto
                {
                    Id = a.Id,
                    NombreApellidos = a.NombreApellidos,
                    CI = a.CI,
                    NumeroTelefono = a.NumeroTelefono,
                    HabitacionesAsignadas = a.Habitaciones.Select(h => new HabitacionAsignadaDto
                    {
                        Numero = h.Numero
                    }).ToList()
                })
                .ToListAsync();

            return Ok(amas);
        }

        // GET: api/AmasDeLlaves/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AmaDeLlavesDetalleDto>> GetAmaDeLlaves(int id)
        {
            var amaDeLlaves = await _context.AmasDeLlaves
                .Include(a => a.Habitaciones)
                .Where(a => a.Id == id)
                .Select(a => new AmaDeLlavesDetalleDto
                {
                    Id = a.Id,
                    NombreApellidos = a.NombreApellidos,
                    CI = a.CI,
                    NumeroTelefono = a.NumeroTelefono,
                    HabitacionesAsignadas = a.Habitaciones.Select(h => new HabitacionAsignadaDto
                    {
                        Numero = h.Numero
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (amaDeLlaves == null)
                return NotFound("El Ama de Llaves no existe.");

            return Ok(amaDeLlaves);
        }

        // PUT: api/AmasDeLlaves/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAmaDeLlaves(int id, AmaDeLlavesCrearDto dto)
        {
            var amaDeLlaves = await _context.AmasDeLlaves.FindAsync(id);
            if (amaDeLlaves == null)
                return NotFound("El Ama de Llaves no existe.");

            // Validar CI duplicado ANTES de modificar el objeto
            var ciOcupado = await _context.AmasDeLlaves.AnyAsync(a => a.CI == dto.CI && a.Id != id);
            if (ciOcupado)
                return Conflict("El CI introducido ya pertenece a otra Ama de Llaves registrada.");

            amaDeLlaves.NombreApellidos = dto.NombreApellidos;
            amaDeLlaves.CI = dto.CI;
            amaDeLlaves.NumeroTelefono = dto.NumeroTelefono;

            try
            {
                await _context.SaveChangesAsync();

                var traza = new Traza
                {
                    FechaHora = DateTime.Now,
                    Operacion = "MODIFICAR_AMA_DE_LLAVES",
                    TablaAfectada = "AmasDeLlaves",
                    RegistroId = amaDeLlaves.Id.ToString(),
                    Detalles = $"Se modificaron los datos del Ama de Llaves {amaDeLlaves.NombreApellidos} (ID: {id})."
                };
                _context.Trazas.Add(traza);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AmaDeLlavesExists(id))
                    return NotFound();
                throw;
            }

            return Ok("Datos actualizados correctamente.");
        }

        // POST: api/AmasDeLlaves
        [HttpPost]
        public async Task<ActionResult<AmaDeLlavesDetalleDto>> PostAmaDeLlaves(AmaDeLlavesCrearDto dto)
        {
            // Validar CI duplicado
            bool existeCi = await _context.AmasDeLlaves.AnyAsync(a => a.CI == dto.CI);
            if (existeCi)
                return Conflict("Ya existe un Ama de Llaves con este CI registrado.");

            var amaDeLlaves = new AmaDeLlaves
            {
                NombreApellidos = dto.NombreApellidos,
                CI = dto.CI,
                NumeroTelefono = dto.NumeroTelefono
            };

            _context.AmasDeLlaves.Add(amaDeLlaves);
            await _context.SaveChangesAsync();

            var traza = new Traza
            {
                FechaHora = DateTime.Now,
                Operacion = "CREAR_AMA_DE_LLAVES",
                TablaAfectada = "AmasDeLlaves",
                RegistroId = amaDeLlaves.Id.ToString(),
                Detalles = $"Ama de Llaves {amaDeLlaves.NombreApellidos} (CI: {amaDeLlaves.CI}) registrada."
            };
            _context.Trazas.Add(traza);
            await _context.SaveChangesAsync();

            var resultadoDto = new AmaDeLlavesDetalleDto
            {
                Id = amaDeLlaves.Id,
                NombreApellidos = amaDeLlaves.NombreApellidos,
                CI = amaDeLlaves.CI,
                NumeroTelefono = amaDeLlaves.NumeroTelefono,
                HabitacionesAsignadas = new List<HabitacionAsignadaDto>()
            };

            return CreatedAtAction("GetAmaDeLlaves", new { id = resultadoDto.Id }, resultadoDto);
        }

        // DELETE: api/AmasDeLlaves/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAmaDeLlaves(int id)
        {
            var amaDeLlaves = await _context.AmasDeLlaves.FindAsync(id);
            if (amaDeLlaves == null)
                return NotFound("El Ama de Llaves no existe.");

            _context.AmasDeLlaves.Remove(amaDeLlaves);
            await _context.SaveChangesAsync();

            var traza = new Traza
            {
                FechaHora = DateTime.Now,
                Operacion = "ELIMINAR_AMA_DE_LLAVES",
                TablaAfectada = "AmasDeLlaves",
                RegistroId = id.ToString(),
                Detalles = $"Se eliminó del sistema al Ama de Llaves {amaDeLlaves.NombreApellidos} (ID: {id})."
            };
            _context.Trazas.Add(traza);
            await _context.SaveChangesAsync();

            return Ok("Ama de Llaves eliminada exitosamente.");
        }

        private bool AmaDeLlavesExists(int id)
        {
            return _context.AmasDeLlaves.Any(e => e.Id == id);
        }
    }
}
