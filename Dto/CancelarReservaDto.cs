using System.ComponentModel.DataAnnotations;

namespace VistaAzul.Dto
{
    public class CancelarReservaDto
    {
        [Required(ErrorMessage = "El motivo de la cancelación es obligatorio.")]
        [StringLength(500)]
        public string Motivo { get; set; } = null!;
    }
}