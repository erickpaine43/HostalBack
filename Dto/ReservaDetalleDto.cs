using System;

namespace VistaAzul.Dto
{
    public class ReservaDetalleDto
    {
        public int Id { get; set; }
        public DateTime FechaReservacion { get; set; }
        public DateTime FechaEntrada { get; set; }
        public DateTime FechaSalida { get; set; }
        public double Importe { get; set; }

        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = null!;

        public int HabitacionNumero { get; set; }

        public bool EstaElClienteEnHostal { get; set; }
        public bool EstaCancelada { get; set; }
        public DateTime? FechaCancelacion { get; set; }
        public string? MotivoCancelacion { get; set; }
    }
}