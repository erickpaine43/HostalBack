namespace VistaAzul.Dto
{
    public class ClienteDetalleDto
    {
        public int Id { get; set; }
        public string NombreApellidos { get; set; } = null!;
        public string CI { get; set; } = null!;
        public string NumeroTelefono { get; set; } = null!;
        public bool EsVIP { get; set; }
    }
}