using System.ComponentModel.DataAnnotations;

namespace ImperioCalaveraMVC.Models
{
    public class VerifyPasswordRequest
    {

        [Required(ErrorMessage = "Debes de colocar tu contraseña actual")]
        public string Password { get; set; }

    }
}
