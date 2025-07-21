using static ImperioCalaveraMVC.Models.Enums.Enums;

namespace ImperioCalaveraBarberShop.Models
{ //This is all I need to create an user
    public class CreateUserDto
    {
        public required string Nombre { get; set; }
        public required string Telefono { get; set; }
        public required string TelefonoEmergencia { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; } // Avoid exposing `PasswordHash` directly
        public Role Rol { get; set; } = Role.Cliente;


    }
}
