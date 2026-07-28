using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VistaAzul.Dto;
using VistaAzul.Modelos;

namespace VistaAzul.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ClientesController : ControllerBase
    {
        private readonly VistaAzulDbContext _context;

        public ClientesController(VistaAzulDbContext context)
        {
            _context = context;
        }

        // GET: api/Clientes
        [HttpGet]
        public async Task<ActionResult> GetClientes(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? ci = null)
        {
            var query = _context.Clientes.AsQueryable();

            // Filtro exacto por CI
            if (!string.IsNullOrEmpty(ci))
                query = query.Where(c => c.CI == ci);

            // Búsqueda por nombre, CI o teléfono
            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.NombreApellidos.Contains(search) ||
                                         c.CI.Contains(search) ||
                                         c.NumeroTelefono.Contains(search));

            int total = await query.CountAsync();

            var clientes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ClienteDetalleDto
                {
                    Id = c.Id,
                    NombreApellidos = c.NombreApellidos,
                    CI = c.CI,
                    NumeroTelefono = c.NumeroTelefono,
                    EsVIP = c.EsVIP
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                pagina = page,
                tamanioPagina = pageSize,
                totalPaginas = (int)Math.Ceiling((double)total / pageSize),
                datos = clientes
            });
        }

        // GET: api/Clientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDetalleDto>> GetCliente(int id)
        {
            var cliente = await _context.Clientes
                .Where(c => c.Id == id)
                .Select(c => new ClienteDetalleDto
                {
                    Id = c.Id,
                    NombreApellidos = c.NombreApellidos,
                    CI = c.CI,
                    NumeroTelefono = c.NumeroTelefono,
                    EsVIP = c.EsVIP
                })
                .FirstOrDefaultAsync();

            if (cliente == null)
                return NotFound(new { mensaje = "El cliente no existe." });

            return Ok(cliente);
        }

        // PUT: api/Clientes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, ClienteCrearDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound(new { mensaje = "El cliente no existe." });

            // Validar CI duplicado ANTES de modificar el objeto
            var ciOcupado = await _context.Clientes.AnyAsync(c => c.CI == dto.CI && c.Id != id);
            if (ciOcupado)
                return Conflict(new { mensaje = "El CI introducido ya pertenece a otro cliente registrado." });

            cliente.NombreApellidos = dto.NombreApellidos;
            cliente.CI = dto.CI;
            cliente.NumeroTelefono = dto.NumeroTelefono;
            cliente.EsVIP = dto.EsVIP;

            try
            {
                await _context.SaveChangesAsync();

                var traza = new Traza
                {
                    FechaHora = DateTime.Now,
                    Operacion = "MODIFICAR_CLIENTE",
                    TablaAfectada = "Clientes",
                    RegistroId = cliente.Id.ToString(),
                    Detalles = $"Se modificaron los datos del cliente {cliente.NombreApellidos} (ID: {cliente.Id})."
                };
                _context.Trazas.Add(traza);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExists(id))
                    return NotFound();
                throw;
            }

            return Ok(new { mensaje = "Datos del cliente actualizados correctamente." });
        }

        // POST: api/Clientes
        [HttpPost]
        public async Task<ActionResult<ClienteDetalleDto>> PostCliente(ClienteCrearDto dto)
        {
            bool existeCi = await _context.Clientes.AnyAsync(c => c.CI == dto.CI);
            if (existeCi)
                return Conflict(new { mensaje = "Ya existe un cliente con este CI registrado." });

            var cliente = new Cliente
            {
                NombreApellidos = dto.NombreApellidos,
                CI = dto.CI,
                NumeroTelefono = dto.NumeroTelefono,
                EsVIP = dto.EsVIP
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var traza = new Traza
            {
                FechaHora = DateTime.Now,
                Operacion = "CREAR_CLIENTE",
                TablaAfectada = "Clientes",
                RegistroId = cliente.Id.ToString(),
                Detalles = $"Cliente {cliente.NombreApellidos} (CI: {cliente.CI}) registrado con éxito."
            };
            _context.Trazas.Add(traza);
            await _context.SaveChangesAsync();

            var resultadoDto = new ClienteDetalleDto
            {
                Id = cliente.Id,
                NombreApellidos = cliente.NombreApellidos,
                CI = cliente.CI,
                NumeroTelefono = cliente.NumeroTelefono,
                EsVIP = cliente.EsVIP
            };

            return CreatedAtAction("GetCliente", new { id = resultadoDto.Id }, resultadoDto);
        }

        // DELETE: api/Clientes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound(new { mensaje = "El cliente no existe." });

            var tieneReservas = await _context.Reservas.AnyAsync(r => r.ClienteId == id);
            if (tieneReservas)
                return BadRequest(new { mensaje = "No se puede eliminar el cliente porque tiene reservas registradas." });

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            var traza = new Traza
            {
                FechaHora = DateTime.Now,
                Operacion = "ELIMINAR_CLIENTE",
                TablaAfectada = "Clientes",
                RegistroId = id.ToString(),
                Detalles = $"Se eliminó al cliente {cliente.NombreApellidos} (ID: {id}) de la base de datos."
            };
            _context.Trazas.Add(traza);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cliente eliminado exitosamente." });
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }
    }
}
