using System.ComponentModel.DataAnnotations;

namespace ImperioCalaveraMVC.Models
{
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
}
