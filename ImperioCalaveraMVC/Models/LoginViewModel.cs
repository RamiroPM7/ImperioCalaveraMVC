using System.ComponentModel.DataAnnotations;

namespace ImperioCalaveraMVC.Models.ViewModels // ¡CAMBIO DE NAMESPACE RECOMENDADO!
{
    // Renombrar de LoginDto a LoginViewModel
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")] // Añadir [Required]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido.")]
        [Display(Name = "Correo Electrónico")] // Para etiquetas amigables en la vista
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")] // Añadir [Required]
        [DataType(DataType.Password)] // Para que el input sea type="password"
        [Display(Name = "Contraseña")] // Para etiquetas amigables en la vista
        // El mensaje de MinLength para contraseña debería ser sobre la contraseña, no sobre "Correo o contraseña incorrectos"
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos {1} caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")] // Para la casilla "Recordarme"
        public bool RememberMe { get; set; }

        // Propiedad para manejar la URL de retorno después del login
        public string? ReturnUrl { get; set; }
    }
}
