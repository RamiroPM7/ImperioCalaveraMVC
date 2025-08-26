using System.ComponentModel.DataAnnotations;

namespace ImperioCalaveraMVC.Models
{
    public class UserEditViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(35, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico no válido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Formato de número de teléfono no válido.")]
        [Display(Name = "Número de Teléfono")]
        public string? PhoneNumber { get; set; }

        [Phone(ErrorMessage = "Formato de número de teléfono de emergencia no válido.")]
        [Display(Name = "Teléfono de Emergencia")]
        public string? TelefonoEmergencia { get; set; }

        [Display(Name = "Fecha de Inicio de Bloqueo (Vacaciones)")]
        [DataType(DataType.Date)]
        public DateTime? LockoutStartDate { get; set; }

        [Display(Name = "Fecha de Fin de Bloqueo (Vacaciones)")]
        [DataType(DataType.Date)]
        public DateTime? LockoutEndDate { get; set; }

        // Propiedades para cambiar la contraseña (opcional)
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres de longitud.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
            ErrorMessage = "La nueva contraseña debe contener al menos una mayúscula, una minúscula, un dígito y un carácter especial.")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nueva Contraseña")]
        [Compare("NewPassword", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string? ConfirmNewPassword { get; set; }

        // Propiedades para gestionar roles (si se decide añadir esta funcionalidad)
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
}
