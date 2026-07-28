using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VistaAzul.Modelos
{
    public class Habitacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] 
        public int Numero { get; set; } 

        public bool EstaFueraDeServicio { get; set; } = false; 

        public ICollection<AmaDeLlaves> AmasDeLlaves { get; set; } = new List<AmaDeLlaves>(); 

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    }
}