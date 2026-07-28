using VistaAzul.Modelos;

namespace VistaAzul.Dto
{
    public class AmaDeLlavesDetalleDto
    {
        public int Id { get; set; }
        public string NombreApellidos { get; set; } = null!;
        public string CI { get; set; } = null!;
        public string NumeroTelefono { get; set; } = null!;
        public List<HabitacionAsignadaDto> HabitacionesAsignadas { get; set; } = null!;
    }
}