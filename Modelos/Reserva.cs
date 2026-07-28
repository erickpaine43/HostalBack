using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VistaAzul.Modelos
{
    public class Reserva
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime FechaReservacion { get; set; } = DateTime.Now; 

        [Required]
        public DateTime FechaEntrada { get; set; }

        [Required]
        public DateTime FechaSalida { get; set; }

        [Required]
        public double Importe { get; set; }
        
        [Required]
        public int ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public Cliente Cliente { get; set; } = null!; 

        [Required]
        public int HabitacionNumero { get; set; }
        [ForeignKey("HabitacionNumero")]
        public Habitacion Habitacion { get; set; } = null!;

        public bool EstaElClienteEnHostal { get; set; } = false; 

        public bool EstaCancelada { get; set; } = false; 

        public DateTime? FechaCancelacion { get; set; } 

        [StringLength(500)]
        public string? MotivoCancelacion { get; set; } 
    }
}