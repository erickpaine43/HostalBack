using System.Collections.Generic;

namespace VistaAzul.Dto
{
    public class HabitacionDto
    {
        public int Numero { get; set; }
        public bool EstaFueraDeServicio { get; set; }
        // Many-to-Many: una habitacion puede tener varias amas de llaves
        public List<int> AmasDeLlavesIds { get; set; } = new();
        public List<string> AmasDeLlavesNombres { get; set; } = new();
    }
}