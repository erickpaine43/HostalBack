using System;
using System.ComponentModel.DataAnnotations;

namespace VistaAzul.Modelos
{
    public class Traza
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Required]
        [StringLength(100)]
        public string Operacion { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string TablaAfectada { get; set; } = null!; 

        [Required]
        public string RegistroId { get; set; } = null!; 

        [Required]
        public string Detalles { get; set; } = null!; 
    }
}