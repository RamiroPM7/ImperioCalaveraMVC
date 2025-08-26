using System.ComponentModel.DataAnnotations;

namespace ImperioCalaveraMVC.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico no válido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [Phone(ErrorMessage = "Formato de número de teléfono no válido.")]
        [Display(Name = "Número de Teléfono")]
        public string PhoneNumber { get; set; } // Puede ser nulo

        [Required(ErrorMessage = "El número de teléfono de Emergencia es obligatorio.")]
        [Phone(ErrorMessage = "Formato de número de teléfono de emergencia no válido.")]
        [Display(Name = "Teléfono de Emergencia")]
        public string TelefonoEmergencia { get; set; } // Puede ser nulo

        // Propiedad para la confirmación de contraseña al editar
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña Actual")]
        public string? CurrentPassword { get; set; }

        // Propiedades para cambiar la contraseña (opcional, si quieres incluirlo aquí)
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres de longitud.", MinimumLength = 6)]
        [Display(Name = "Nueva Contraseña")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nueva Contraseña")]
        [Compare("NewPassword", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string? ConfirmNewPassword { get; set; }
    }
}
