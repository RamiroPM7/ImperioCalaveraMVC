using System.ComponentModel.DataAnnotations;

namespace ImperioCalaveraMVC.Models.ViewModels // Asegúrate de que el namespace sea correcto
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la contraseña de confirmación no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [MaxLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres.")]
        [Phone(ErrorMessage = "Formato de teléfono inválido.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono de emergencia es obligatorio.")]
        [MaxLength(20, ErrorMessage = "El teléfono de emergencia no puede exceder los 20 caracteres.")]
        [Phone(ErrorMessage = "Formato de teléfono de emergencia inválido.")]
        [Display(Name = "Teléfono de Emergencia")]
        public string TelefonoEmergencia { get; set; } = string.Empty;
    }
}
