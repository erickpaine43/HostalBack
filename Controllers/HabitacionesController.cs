using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VistaAzul.Dto;
using VistaAzul.Modelos;

namespace VistaAzul.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HabitacionesController : ControllerBase
    {
        private readonly VistaAzulDbContext _context;

        public HabitacionesController(VistaAzulDbContext context)
        {
            _context = context;
        }

        // GET: api/Habitaciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HabitacionDto>>> GetHabitaciones()
        {
            var habitacionesDb = await _context.Habitaciones
                .Include(h => h.AmasDeLlaves)
                .Select(h => new
                {
                    h.Numero,
                    h.EstaFueraDeServicio,
                    IdsAmas    = h.AmasDeLlaves.Select(a => a.Id).ToList(),
                    NombresAmas = h.AmasDeLlaves.Select(a => a.NombreApellidos).ToList()
                })
                .ToListAsync();

            var habitacionesDtos = habitacionesDb.Select(h => new HabitacionDto
            {
                Numero              = h.Numero,
                EstaFueraDeServicio = h.EstaFueraDeServicio,
                AmasDeLlavesIds     = h.IdsAmas,
                AmasDeLlavesNombres = h.NombresAmas
            }).ToList();

            return Ok(habitacionesDtos);
        }

        // GET: api/Habitaciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HabitacionDto>> GetHabitacion(int id)
        {
            var habitacionDb = await _context.Habitaciones
                .Include(h => h.AmasDeLlaves)
                .Where(h => h.Numero == id)
                .Select(h => new
                {
                    h.Numero,
                    h.EstaFueraDeServicio,
                    IdsAmas    = h.AmasDeLlaves.Select(a => a.Id).ToList(),
                    NombresAmas = h.AmasDeLlaves.Select(a => a.NombreApellidos).ToList()
                })
                .FirstOrDefaultAsync();

            if (habitacionDb == null)
            {
                return NotFound($"La habitacion numero {id} no existe.");
            }

            var habitacionDto = new HabitacionDto
            {
                Numero              = habitacionDb.Numero,
                EstaFueraDeServicio = habitacionDb.EstaFueraDeServicio,
                AmasDeLlavesIds     = habitacionDb.IdsAmas,
                AmasDeLlavesNombres = habitacionDb.NombresAmas
            };

            return Ok(habitacionDto);
        }

        // PUT: api/Habitaciones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHabitacion(int id, HabitacionActualizarDto dto)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id);
            if (habitacion == null)
                return NotFound($"La habitacion numero {id} no existe.");

            // Si se intenta poner fuera de servicio, validar que no tenga reservas activas
            if (dto.EstaFueraDeServicio && !habitacion.EstaFueraDeServicio)
            {
                var tieneReservasActivas = await _context.Reservas
                    .AnyAsync(r => r.HabitacionNumero == id && !r.EstaCancelada && r.FechaSalida.Date >= DateTime.Today);

                if (tieneReservasActivas)
                    return BadRequest("No se puede poner fuera de servicio una habitación que tiene reservas activas.");
            }

            habitacion.EstaFueraDeServicio = dto.EstaFueraDeServicio;

            try
            {
                await _context.SaveChangesAsync();

                var traza = new Traza
                {
                    FechaHora = DateTime.Now,
                    Operacion = dto.EstaFueraDeServicio ? "DESHABILITAR_HABITACION" : "HABILITAR_HABITACION",
                    TablaAfectada = "Habitaciones",
                    RegistroId = habitacion.Numero.ToString(),
                    Detalles = dto.EstaFueraDeServicio
                        ? $"La habitacion {habitacion.Numero} fue puesta fuera de servicio."
                        : $"La habitacion {habitacion.Numero} fue habilitada nuevamente."
                };
                _context.Trazas.Add(traza);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HabitacionExists(id))
                    return NotFound();
                throw;
            }

            return Ok($"Habitacion {id} actualizada con exito.");
        }

        // POST: api/Habitaciones
        [HttpPost]
        public async Task<ActionResult<HabitacionDto>> PostHabitacion(HabitacionDto dto)
        {
            int numero = dto.Numero;
            int piso = numero / 10;
            int hab  = numero % 10;
            // Formato 0XY: X es el piso (1-3), Y es la habitación en el piso (1-5)
            // Representado como entero: 11-15 (piso 1), 21-25 (piso 2), 31-35 (piso 3)
            if (piso < 1 || piso > 3 || hab < 1 || hab > 5)
            {
                return BadRequest("El número de habitación debe seguir el formato 0XY: piso X entre 1 y 3, habitación Y entre 1 y 5 (ej: 11, 23, 35).");
            }

            var habitacion = new Habitacion
            {
                Numero              = dto.Numero,
                EstaFueraDeServicio = dto.EstaFueraDeServicio
            };

            _context.Habitaciones.Add(habitacion);
            try
            {
                await _context.SaveChangesAsync();

                var traza = new Traza
                {
                    FechaHora    = DateTime.Now,
                    Operacion    = "CREAR_HABITACION",
                    TablaAfectada = "Habitaciones",
                    RegistroId   = habitacion.Numero.ToString(),
                    Detalles     = $"Se registro la nueva habitacion numero {habitacion.Numero} en el sistema."
                };
                _context.Trazas.Add(traza);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (HabitacionExists(habitacion.Numero))
                {
                    return Conflict($"La habitacion numero {habitacion.Numero} ya se encuentra registrada.");
                }
                else
                {
                    throw;
                }
            }

            var resultadoDto = new HabitacionDto
            {
                Numero              = habitacion.Numero,
                EstaFueraDeServicio = habitacion.EstaFueraDeServicio,
                AmasDeLlavesIds     = new List<int>(),
                AmasDeLlavesNombres = new List<string>()
            };

            return CreatedAtAction("GetHabitacion", new { id = resultadoDto.Numero }, resultadoDto);
        }

        // POST: api/Habitaciones/5/asignar-ama/3
        [HttpPost("{habitacionId}/asignar-ama/{amaDeLlavesId}")]
        public async Task<IActionResult> AsignarAmaDeLlaves(int habitacionId, int amaDeLlavesId)
        {
            var habitacion = await _context.Habitaciones
                .Include(h => h.AmasDeLlaves)
                .FirstOrDefaultAsync(h => h.Numero == habitacionId);

            if (habitacion == null)
            {
                return NotFound($"La habitacion numero {habitacionId} no existe.");
            }

            var amaDeLlaves = await _context.AmasDeLlaves.FindAsync(amaDeLlavesId);
            if (amaDeLlaves == null)
            {
                return NotFound($"El Ama de Llaves con ID {amaDeLlavesId} no existe.");
            }

            if (habitacion.AmasDeLlaves.Any(a => a.Id == amaDeLlavesId))
            {
                return Conflict($"El Ama de Llaves ID {amaDeLlavesId} ya esta asignada a la habitacion {habitacionId}.");
            }

            habitacion.AmasDeLlaves.Add(amaDeLlaves);
            await _context.SaveChangesAsync();

            var traza = new Traza
            {
                FechaHora     = DateTime.Now,
                Operacion     = "ASIGNAR_AMA_DE_LLAVES",
                TablaAfectada = "HabitacionAmaDeLlaves",
                RegistroId    = $"{habitacionId}-{amaDeLlavesId}",
                Detalles      = $"Ama de Llaves '{amaDeLlaves.NombreApellidos}' (ID {amaDeLlavesId}) asignada a la habitacion {habitacionId}."
            };
            _context.Trazas.Add(traza);
            await _context.SaveChangesAsync();

            return Ok($"Ama de Llaves '{amaDeLlaves.NombreApellidos}' asignada correctamente a la habitacion {habitacionId}.");
        }

        // DELETE: api/Habitaciones/5/desasignar-ama/3
        [HttpDelete("{habitacionId}/desasignar-ama/{amaDeLlavesId}")]
        public async Task<IActionResult> DesasignarAmaDeLlaves(int habitacionId, int amaDeLlavesId)
        {
            var habitacion = await _context.Habitaciones
                .Include(h => h.AmasDeLlaves)
                .FirstOrDefaultAsync(h => h.Numero == habitacionId);

            if (habitacion == null)
            {
                return NotFound($"La habitacion numero {habitacionId} no existe.");
            }

            var amaDeLlaves = habitacion.AmasDeLlaves.FirstOrDefault(a => a.Id == amaDeLlavesId);
            if (amaDeLlaves == null)
            {
                return NotFound($"El Ama de Llaves ID {amaDeLlavesId} no esta asignada a la habitacion {habitacionId}.");
            }

            habitacion.AmasDeLlaves.Remove(amaDeLlaves);
            await _context.SaveChangesAsync();

            var traza = new Traza
            {
                FechaHora     = DateTime.Now,
                Operacion     = "DESASIGNAR_AMA_DE_LLAVES",
                TablaAfectada = "HabitacionAmaDeLlaves",
                RegistroId    = $"{habitacionId}-{amaDeLlavesId}",
                Detalles      = $"Ama de Llaves '{amaDeLlaves.NombreApellidos}' (ID {amaDeLlavesId}) desasignada de la habitacion {habitacionId}."
            };
            _context.Trazas.Add(traza);
            await _context.SaveChangesAsync();

            return Ok($"Ama de Llaves '{amaDeLlaves.NombreApellidos}' desasignada correctamente de la habitacion {habitacionId}.");
        }

        private bool HabitacionExists(int id)
        {
            return _context.Habitaciones.Any(e => e.Numero == id);
        }
    }
}