using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VistaAzul.Modelos;
using VistaAzul.Dto;

namespace VistaAzul.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultasController : ControllerBase
    {
        private readonly VistaAzulDbContext _context;

        public ConsultasController(VistaAzulDbContext context)
        {
            _context = context;
        }

        
        [HttpGet("habitaciones-disponibles")]
        public async Task<ActionResult<IEnumerable<HabitacionDto>>> GetHabitacionesDisponibles(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            if (fechaInicio >= fechaFin)
            {
                return BadRequest("La fecha de inicio debe ser menor que la fecha de fin.");
            }

            var habitacionesOcupadasIds = await _context.Reservas
                .Where(r => !r.EstaCancelada &&
                            fechaInicio.Date <= r.FechaSalida.Date &&
                            fechaFin.Date    >= r.FechaEntrada.Date)
                .Select(r => r.HabitacionNumero)
                .Distinct()
                .ToListAsync();

            var habitacionesDb = await _context.Habitaciones
                .Include(h => h.AmasDeLlaves)
                .Where(h => !h.EstaFueraDeServicio && !habitacionesOcupadasIds.Contains(h.Numero))
                .Select(h => new
                {
                    h.Numero,
                    h.EstaFueraDeServicio,
                    IdsAmas    = h.AmasDeLlaves.Select(a => a.Id).ToList(),
                    NombresAmas = h.AmasDeLlaves.Select(a => a.NombreApellidos).ToList()
                })
                .ToListAsync();

            var habitacionesDisponibles = habitacionesDb.Select(h => new HabitacionDto
            {
                Numero              = h.Numero,
                EstaFueraDeServicio = h.EstaFueraDeServicio,
                AmasDeLlavesIds     = h.IdsAmas,
                AmasDeLlavesNombres = h.NombresAmas
            }).ToList();

            return Ok(habitacionesDisponibles);
        }

        [HttpGet("por-ama-de-llaves/{amaDeLlavesId}")]
        public async Task<ActionResult<IEnumerable<HabitacionDto>>> GetHabitacionesPorAmaDeLlaves(int amaDeLlavesId)
        {
            var existeAma = await _context.AmasDeLlaves.AnyAsync(a => a.Id == amaDeLlavesId);
            if (!existeAma)
            {
                return NotFound($"El Ama de Llaves con ID {amaDeLlavesId} no existe.");
            }

            var habitacionesDb = await _context.Habitaciones
                .Include(h => h.AmasDeLlaves)
                .Where(h => h.AmasDeLlaves.Any(a => a.Id == amaDeLlavesId))
                .Select(h => new
                {
                    h.Numero,
                    h.EstaFueraDeServicio,
                    IdsAmas    = h.AmasDeLlaves.Select(a => a.Id).ToList(),
                    NombresAmas = h.AmasDeLlaves.Select(a => a.NombreApellidos).ToList()
                })
                .ToListAsync();

            var habitaciones = habitacionesDb.Select(h => new HabitacionDto
            {
                Numero              = h.Numero,
                EstaFueraDeServicio = h.EstaFueraDeServicio,
                AmasDeLlavesIds     = h.IdsAmas,
                AmasDeLlavesNombres = h.NombresAmas
            }).ToList();

            return Ok(habitaciones);
        }

        [HttpGet("clientes-activos")]
        public async Task<ActionResult<IEnumerable<ClienteActivoDto>>> GetActivos([FromQuery]DateTime dia)
        {
            DateTime fechaLimpia = dia.Date;
            var clienteActivo = await _context.Reservas
                .Include(r => r.Cliente)
                .Where(r => !r.EstaCancelada &&
                            fechaLimpia >= r.FechaEntrada.Date &&
                            fechaLimpia <= r.FechaSalida.Date)
                .Select(r => new ClienteActivoDto
                {
                    NombreApellidos = r.Cliente.NombreApellidos,
                    NumeroHabitacion = r.HabitacionNumero
                })
                .ToListAsync();
            return Ok(clienteActivo);
        }

        [HttpGet("auditoria-trazas")]
        public async Task<ActionResult<IEnumerable<Traza>>> GetTrazas()
        {
            var trazas = await _context.Trazas
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();

            return Ok(trazas);
        }
    }
}