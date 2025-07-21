namespace ImperioCalaveraMVC.Models
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string Nombre { get; set; }
        public required string Email { get; set; }
        public required string Telefono { get; set; }
        public required string Rol { get; set; }
    }
}
