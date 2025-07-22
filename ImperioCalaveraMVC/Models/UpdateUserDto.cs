using static ImperioCalaveraMVC.Models.Enums.Enums;
using System.ComponentModel.DataAnnotations;

namespace ImperioCalaveraMVC.Models
{
    public class UpdateUserDto
    {
    public string? Nombre { get; set; }
    public bool? Eliminado { get; set; } // Nullable to prevent forced updates
    public string? Telefono { get; set; }
    public string? TelefonoEmergencia { get; set; }

    }
}
