using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VistaAzul.Modelos
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre y apellidos son obligatorios.")]
        [StringLength(150)]
        public string NombreApellidos { get; set; } = null!; 

        [Required(ErrorMessage = "El CI es obligatorio.")]
        [StringLength(20)]
        // Configurado como único en el DbContext
        public string CI { get; set; } = null!; 

        [Required(ErrorMessage = "El número telefónico es obligatorio.")]
        [Phone]
        [StringLength(20)]
        public string NumeroTelefono { get; set; } = null!; 

        public bool EsVIP { get; set; } = false;

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}