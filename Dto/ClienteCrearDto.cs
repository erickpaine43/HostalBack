using System.ComponentModel.DataAnnotations;

namespace VistaAzul.Dto
{
    public class ClienteCrearDto
    {
        [Required(ErrorMessage = "El nombre y apellidos son obligatorios.")]
        [StringLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo debe contener letras y espacios.")]
        public string NombreApellidos { get; set; } = null!;

        [Required(ErrorMessage = "El CI es obligatorio.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "El CI debe contener exactamente 11 dígitos numéricos.")]
        public string CI { get; set; } = null!;

        [Required(ErrorMessage = "El número telefónico es obligatorio.")]
        [StringLength(20)]
        [RegularExpression(@"^\+?[1-9]\d{6,14}$", ErrorMessage = "Ingrese un número de teléfono válido con su código de país (Ejemplo: +5351234567 o +13055550123).")]
        public string NumeroTelefono { get; set; } = null!;

        public bool EsVIP { get; set; } = false;
    }
}