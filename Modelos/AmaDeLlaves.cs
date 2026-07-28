using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VistaAzul.Modelos
{
    public class AmaDeLlaves
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre y apellidos son obligatorios.")]
        [StringLength(150)]
        public string NombreApellidos { get; set; } = null!; 

        [Required(ErrorMessage = "El CI es obligatorio.")]
        [StringLength(20)]
        public string CI { get; set; } = null!; 

        [Required(ErrorMessage = "El número telefónico es obligatorio.")]
        [Phone]
        [StringLength(20)]
        public string NumeroTelefono { get; set; } = null!; 

        public ICollection<Habitacion> Habitaciones { get; set; } = new List<Habitacion>(); 
    }
}