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
    public class ReservasController : ControllerBase
    {
        private readonly VistaAzulDbContext _context;

        public ReservasController(VistaAzulDbContext context)
        {
            _context = context;
        }

        // GET: api/Reservas
        [HttpGet]
        public async Task<ActionResult> GetReservas(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Reservas.Include(r => r.Cliente);

            int total = await query.CountAsync();

            var reservas = await query
                .OrderByDescending(r => r.FechaEntrada)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReservaDetalleDto
                {
                    Id = r.Id,
                    FechaReservacion = r.FechaReservacion,
                    FechaEntrada = r.FechaEntrada,
                    FechaSalida = r.FechaSalida,
                    Importe = r.Importe,
                    ClienteId = r.ClienteId,
                    ClienteNombre = r.Cliente.NombreApellidos,
                    HabitacionNumero = r.HabitacionNumero,
                    EstaElClienteEnHostal = r.EstaElClienteEnHostal,
                    EstaCancelada = r.EstaCancelada,
                    FechaCancelacion = r.FechaCancelacion,
                    MotivoCancelacion = r.MotivoCancelacion
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                pagina = page,
                tamanioPagina = pageSize,
                totalPaginas = (int)Math.Ceiling((double)total / pageSize),
                datos = reservas
            });
        }

        // GET: api/Reservas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservaDetalleDto>> GetReserva(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .Where(r => r.Id == id)
                .Select(r => new ReservaDetalleDto
                {
                    Id = r.Id,
                    FechaReservacion = r.FechaReservacion,
                    FechaEntrada = r.FechaEntrada,
                    FechaSalida = r.FechaSalida,
                    Importe = r.Importe,
                    ClienteId = r.ClienteId,
                    ClienteNombre = r.Cliente.NombreApellidos,
                    HabitacionNumero = r.HabitacionNumero,
                    EstaElClienteEnHostal = r.EstaElClienteEnHostal,
                    EstaCancelada = r.EstaCancelada,
                    FechaCancelacion = r.FechaCancelacion,
                    MotivoCancelacion = r.MotivoCancelacion
                })
                .FirstOrDefaultAsync();

            if (reserva == null)
                return NotFound("La reserva no existe.");

            return Ok(reserva);
        }

        // PUT: api/Reservas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReserva(int id, ReservaCrearDto dto)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
                return NotFound("La reserva no existe.");

            // No se puede modificar una reserva cancelada
            if (reserva.EstaCancelada)
                return BadRequest("No se puede modificar una reserva cancelada.");

            // No se puede modificar si el cliente ya está en el hostal
            if (reserva.EstaElClienteEnHostal)
                return BadRequest("No se puede modificar la reserva porque el cliente ya se encuentra registrado en el hostal.");

            // No se puede modificar si la fecha de entrada ya pasó
            if (DateTime.Now.Date > reserva.FechaEntrada.Date)
                return BadRequest("No se puede modificar la reserva porque la fecha de entrada ya ha pasado.");

            var habitacion = await _context.Habitaciones.FindAsync(dto.HabitacionNumero);
            if (habitacion == null)
                return NotFound("La habitacion especificada no existe.");

            // Validar que la habitación no esté fuera de servicio
            if (habitacion.EstaFueraDeServicio)
                return BadRequest("No se puede asignar una habitacion que está fuera de servicio.");

            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null)
                return NotFound("El cliente especificado no existe.");

            if (dto.FechaEntrada >= dto.FechaSalida)
                return BadRequest("La fecha de entrada debe ser menor que la fecha de salida.");

            int cantidadDias = (dto.FechaSalida.Date - dto.FechaEntrada.Date).Days + 1;
            if (cantidadDias < 3)
                return BadRequest("El periodo minimo de reserva es de tres dias.");

            // Verificar disponibilidad de habitación (excluye esta misma reserva)
            bool habitacionOcupada = await _context.Reservas
                .AnyAsync(r => r.Id != id
                               && r.HabitacionNumero == dto.HabitacionNumero
                               && !r.EstaCancelada
                               && dto.FechaEntrada.Date <= r.FechaSalida.Date
                               && dto.FechaSalida.Date >= r.FechaEntrada.Date);

            if (habitacionOcupada)
                return BadRequest("La habitacion no esta disponible para las fechas seleccionadas.");

            // Verificar que el cliente no tenga otra reserva en el mismo periodo
            bool clienteTieneReserva = await _context.Reservas
                .AnyAsync(r => r.Id != id
                               && r.ClienteId == dto.ClienteId
                               && !r.EstaCancelada
                               && dto.FechaEntrada.Date <= r.FechaSalida.Date
                               && dto.FechaSalida.Date >= r.FechaEntrada.Date);

            if (clienteTieneReserva)
                return BadRequest("El cliente ya tiene otra habitacion reservada en el mismo periodo.");

            double costoTotal = cantidadDias * 10.0;
            if (cliente.EsVIP)
                costoTotal *= 0.90;

            reserva.FechaEntrada = dto.FechaEntrada;
            reserva.FechaSalida = dto.FechaSalida;
            reserva.ClienteId = dto.ClienteId;
            reserva.HabitacionNumero = dto.HabitacionNumero;
            reserva.Importe = costoTotal;

            try
            {
                await _context.SaveChangesAsync();

                var traza = new Traza
                {
                    FechaHora = DateTime.Now,
                    Operacion = "MODIFICAR_RESERVA",
                    TablaAfectada = "Reservas",
                    RegistroId = reserva.Id.ToString(),
                    Detalles = $"Reserva ID {reserva.Id} modificada. Nuevo periodo: {reserva.FechaEntrada:dd/MM/yyyy} - {reserva.FechaSalida:dd/MM/yyyy}. Nuevo importe: {reserva.Importe} USD."
                };
                _context.Trazas.Add(traza);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservaExists(id))
                    return NotFound();
                throw;
            }

            return Ok("Reserva actualizada correctamente.");
        }

        // POST: api/Reservas
        [HttpPost]
        public async Task<ActionResult<ReservaDetalleDto>> PostReserva(ReservaCrearDto dto)
        {
            var habitacion = await _context.Habitaciones
                .FirstOrDefaultAsync(h => h.Numero == dto.HabitacionNumero);
            if (habitacion == null)
                return NotFound("La habitacion no existe.");

            if (habitacion.EstaFueraDeServicio)
                return BadRequest("No se puede reservar porque la habitacion esta fuera de servicio.");

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == dto.ClienteId);
            if (cliente == null)
                return NotFound("El cliente no existe.");
            if (dto.FechaEntrada.Date < DateTime.Today)
                return BadRequest("La fecha de entrada no puede ser anterior a la fecha actual.");

            if (dto.FechaEntrada >= dto.FechaSalida)
                return BadRequest("La fecha de entrada debe ser menor que la fecha de salida.");

            int cantidadDias = (dto.FechaSalida.Date - dto.FechaEntrada.Date).Days + 1;
            if (cantidadDias < 3)
                return BadRequest("El periodo minimo de reserva es de tres dias.");

            // Verificar disponibilidad de habitación (solo reservas no canceladas con fechas solapadas)
            bool habitacionOcupada = await _context.Reservas
                .AnyAsync(r => r.HabitacionNumero == dto.HabitacionNumero
                               && !r.EstaCancelada
                               && dto.FechaEntrada.Date <= r.FechaSalida.Date
                               && dto.FechaSalida.Date >= r.FechaEntrada.Date);

            if (habitacionOcupada)
                return BadRequest("La habitacion no esta disponible para las fechas seleccionadas.");

            // Un cliente no puede tener dos reservas en el mismo periodo
            bool clienteTieneReserva = await _context.Reservas
                .AnyAsync(r => r.ClienteId == dto.ClienteId
                               && !r.EstaCancelada
                               && dto.FechaEntrada.Date <= r.FechaSalida.Date
                               && dto.FechaSalida.Date >= r.FechaEntrada.Date);

            if (clienteTieneReserva)
                return BadRequest("Un cliente no puede reservar dos habitaciones en el mismo periodo.");

            double costoTotal = cantidadDias * 10.0;
            if (cliente.EsVIP)
                costoTotal *= 0.90;

            var nuevaReserva = new Reserva
            {
                FechaReservacion = DateTime.Now,
                FechaEntrada = dto.FechaEntrada,
                FechaSalida = dto.FechaSalida,
                Importe = costoTotal,
                ClienteId = dto.ClienteId,
                HabitacionNumero = dto.HabitacionNumero,
                EstaElClienteEnHostal = false,
                EstaCancelada = false
            };

            try
            {
                _context.Reservas.Add(nuevaReserva);
                await _context.SaveChangesAsync();

                var traza = new Traza
                {
                    FechaHora = DateTime.Now,
                    Operacion = "CREAR_RESERVA",
                    TablaAfectada = "Reservas",
                    RegistroId = nuevaReserva.Id.ToString(),
                    Detalles = $"Reserva creada para el Cliente ID {nuevaReserva.ClienteId} en la habitacion {nuevaReserva.HabitacionNumero} por un importe de {nuevaReserva.Importe} USD."
                };
                _context.Trazas.Add(traza);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno al procesar la reserva.");
            }

            var resultadoDto = new ReservaDetalleDto
            {
                Id = nuevaReserva.Id,
                FechaReservacion = nuevaReserva.FechaReservacion,
                FechaEntrada = nuevaReserva.FechaEntrada,
                FechaSalida = nuevaReserva.FechaSalida,
                Importe = nuevaReserva.Importe,
                ClienteId = nuevaReserva.ClienteId,
                ClienteNombre = cliente.NombreApellidos,
                HabitacionNumero = nuevaReserva.HabitacionNumero,
                EstaElClienteEnHostal = nuevaReserva.EstaElClienteEnHostal,
                EstaCancelada = nuevaReserva.EstaCancelada,
                FechaCancelacion = nuevaReserva.FechaCancelacion,
                MotivoCancelacion = nuevaReserva.MotivoCancelacion
            };

            return CreatedAtAction("GetReserva", new { id = resultadoDto.Id }, resultadoDto);
        }

        // DELETE: api/Reservas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
                return NotFound("La reserva no existe.");

            // No se puede eliminar una reserva si el cliente ya estuvo o está en el hostal
            if (reserva.EstaElClienteEnHostal)
                return BadRequest("No se puede eliminar una reserva de un cliente que se encuentra o estuvo en el hostal.");

            // No se puede eliminar una reserva activa (no cancelada y con fechas vigentes o futuras)
            if (!reserva.EstaCancelada && reserva.FechaSalida.Date >= DateTime.Today)
                return BadRequest("No se puede eliminar una reserva activa. Cancélela primero.");

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            var traza = new Traza
            {
                FechaHora = DateTime.Now,
                Operacion = "ELIMINAR_RESERVA",
                TablaAfectada = "Reservas",
                RegistroId = id.ToString(),
                Detalles = $"Se eliminó definitivamente la reserva ID {id} asociada al Cliente ID {reserva.ClienteId}."
            };
            _context.Trazas.Add(traza);
            await _context.SaveChangesAsync();

            return Ok("La reserva fue eliminada del sistema.");
        }

        // POST: api/Reservas/5/cancelar
        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> CancelarReserva(int id, CancelarReservaDto dto)
        {
            var reserva = await _context.Reservas.FirstOrDefaultAsync(r => r.Id == id);
            if (reserva == null)
                return NotFound("La reserva no existe.");

            if (reserva.EstaCancelada)
                return BadRequest("Esta reserva ya se encuentra cancelada.");

            if (reserva.EstaElClienteEnHostal)
                return BadRequest("No se puede cancelar la reserva porque el cliente ya se encuentra registrado en el hostal.");

            reserva.EstaCancelada = true;
            reserva.FechaCancelacion = DateTime.Now;
            reserva.MotivoCancelacion = dto.Motivo;

            await _context.SaveChangesAsync();

            var traza = new Traza
            {
                FechaHora = DateTime.Now,
                Operacion = "CANCELAR_RESERVA",
                TablaAfectada = "Reservas",
                RegistroId = reserva.Id.ToString(),
                Detalles = $"Reserva ID {reserva.Id} cancelada. Motivo: {dto.Motivo}"
            };
            _context.Trazas.Add(traza);
            await _context.SaveChangesAsync();

            return Ok("La reserva ha sido cancelada exitosamente.");
        }

        // POST: api/Reservas/5/checkin
        [HttpPost("{id}/checkin")]
        public async Task<IActionResult> RegistrarLlegada(int id)
        {
            var reserva = await _context.Reservas.FirstOrDefaultAsync(r => r.Id == id);
            if (reserva == null)
                return NotFound("La reserva no existe.");

            if (reserva.EstaCancelada)
                return BadRequest("No se puede dar entrada porque la reserva está cancelada.");

            if (reserva.EstaElClienteEnHostal)
                return BadRequest("El cliente ya se encuentra registrado en el hostal.");

            // El check-in solo se puede realizar a partir de la fecha de entrada
            if (DateTime.Now.Date < reserva.FechaEntrada.Date)
                return BadRequest($"No se puede registrar la llegada antes de la fecha de entrada ({reserva.FechaEntrada:dd/MM/yyyy}).");

            // No tiene sentido hacer check-in después de la fecha de salida
            if (DateTime.Now.Date > reserva.FechaSalida.Date)
                return BadRequest("La fecha de salida de esta reserva ya pasó.");

            reserva.EstaElClienteEnHostal = true;
            await _context.SaveChangesAsync();

            var traza = new Traza
            {
                FechaHora = DateTime.Now,
                Operacion = "CHECK_IN",
                TablaAfectada = "Reservas",
                RegistroId = reserva.Id.ToString(),
                Detalles = $"El cliente de la reserva ID {reserva.Id} ha ingresado al hostal."
            };
            _context.Trazas.Add(traza);
            await _context.SaveChangesAsync();

            return Ok("Se ha registrado la entrada del cliente exitosamente.");
        }

        // POST: api/Reservas/5/cambiar-habitacion
        [HttpPost("{id}/cambiar-habitacion")]
        public async Task<IActionResult> CambiarHabitacion(int id, [FromBody] int nuevaHabitacion)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
                return NotFound("La reserva no existe.");

            if (reserva.EstaCancelada)
                return BadRequest("No se puede cambiar de habitacion porque la reserva esta cancelada.");

            // Solo se puede cambiar de habitación a un cliente que ya está en el hostal
            if (!reserva.EstaElClienteEnHostal)
                return BadRequest("Solo se puede cambiar de habitacion a un cliente que ya se encuentra en el hostal.");

            var habitacion = await _context.Habitaciones.FindAsync(nuevaHabitacion);
            if (habitacion == null)
                return NotFound("La habitacion especificada no existe.");

            if (habitacion.EstaFueraDeServicio)
                return BadRequest("La nueva habitacion se encuentra fuera de servicio.");

            if (reserva.HabitacionNumero == nuevaHabitacion)
                return BadRequest("El cliente ya se encuentra en esa habitacion.");

            // Verificar que la nueva habitación esté disponible para el período restante de la reserva
            bool habitacionOcupada = await _context.Reservas
                .AnyAsync(r => r.Id != id
                               && r.HabitacionNumero == nuevaHabitacion
                               && !r.EstaCancelada
                               && reserva.FechaEntrada.Date <= r.FechaSalida.Date
                               && reserva.FechaSalida.Date >= r.FechaEntrada.Date);

            if (habitacionOcupada)
                return BadRequest("La nueva habitacion esta ocupada en el periodo de la reserva.");

            int habitacionAnterior = reserva.HabitacionNumero;
            reserva.HabitacionNumero = nuevaHabitacion;

            await _context.SaveChangesAsync();

            var traza = new Traza
            {
                FechaHora = DateTime.Now,
                Operacion = "CAMBIAR_HABITACION",
                TablaAfectada = "Reservas",
                RegistroId = reserva.Id.ToString(),
                Detalles = $"El cliente de la reserva ID {reserva.Id} fue cambiado de la habitacion {habitacionAnterior} a la habitacion {nuevaHabitacion}."
            };
            _context.Trazas.Add(traza);
            await _context.SaveChangesAsync();

            return Ok("Se ha cambiado al cliente de habitacion exitosamente.");
        }

        private bool ReservaExists(int id)
        {
            return _context.Reservas.Any(e => e.Id == id);
        }
    }
}
