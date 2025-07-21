using System.ComponentModel.DataAnnotations;

namespace ImperioCalaveraMVC.Models
{
    public class LoginDto
    {

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public required string Email { get; set; }

        [MinLength(6, ErrorMessage = "Correo o contraseña incorrectos")]
        public required string Password { get; set; }

    }
}
