using System;
using System.ComponentModel.DataAnnotations;

namespace VistaAzul.Dto
{
    public class ReservaCrearDto
    {
        [Required(ErrorMessage = "La fecha de entrada es obligatoria.")]
        public DateTime FechaEntrada { get; set; }

        [Required(ErrorMessage = "La fecha de salida es obligatoria.")]
        public DateTime FechaSalida { get; set; }

        [Required(ErrorMessage = "El ID del cliente es obligatorio.")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "El número de habitación es obligatorio.")]
        public int HabitacionNumero { get; set; }
    }
}