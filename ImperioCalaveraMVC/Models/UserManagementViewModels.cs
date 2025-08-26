using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ImperioCalaveraMVC.Models.ViewModels
{
    // ViewModel para mostrar los datos del usuario en las tarjetas
    public class UserCardViewModel
    {
        public string Id { get; set; }
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; }
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }
        [Display(Name = "Número de Teléfono")]
        public string? PhoneNumber { get; set; }
        [Display(Name = "Estado de Bloqueo")]
        public bool IsLockedOut { get; set; }
        [Display(Name = "Fin de Bloqueo")]
        public DateTimeOffset? LockoutEnd { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    // ViewModel para el formulario de edición de usuario en el modal
    public class UserEditViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
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
