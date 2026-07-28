using System.ComponentModel.DataAnnotations;

namespace VistaAzul.Dto
{
    public class ClienteActivoDto
    {
        public string NombreApellidos { get; set; } = null!;

        public int NumeroHabitacion {  get; set; }

    }
}
