using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ImperioCalaveraMVC.Models
{
    // NUEVO: ViewModel para crear un nuevo usuario
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo Email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico no válido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El campo Contraseña es obligatorio.")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
            ErrorMessage = "La contraseña debe contener al menos una mayúscula, una minúscula, un dígito y un carácter especial.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
        public string ConfirmPassword { get; set; }

        [Phone(ErrorMessage = "Formato de número de teléfono no válido.")]
        [Display(Name = "Número de Teléfono")]
        public string? PhoneNumber { get; set; }

        [Phone(ErrorMessage = "Formato de número de teléfono de emergencia no válido.")]
        [Display(Name = "Teléfono de Emergencia")]
        public string? TelefonoEmergencia { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un rol para el usuario.")]
        [Display(Name = "Rol")]
        public string SelectedRole { get; set; }

        // Para poblar el DropdownList de roles en la vista
        public List<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();
    }
}
