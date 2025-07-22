using ImperioCalaveraMVC.Models.Entities;
using Microsoft.AspNetCore.Identity;
using static ImperioCalaveraMVC.Models.Enums.Enums;
using System.ComponentModel.DataAnnotations;

public class Usuario : IdentityUser
{
    public Usuario()
    {
        CitasComoCliente = new List<Cita>();
        CitasComoBarbero = new List<Cita>();
        Disponibilidades = new List<Disponibilidad>();
    }

    

    [Required, MaxLength(100)]
    public  required string Nombre { get; set; }

    [Required, MaxLength(20)]
    public required string TelefonoEmergencia { get; set; }

    public ICollection<Cita> CitasComoCliente { get; set; }
    public ICollection<Cita> CitasComoBarbero { get; set; }
    public ICollection<Disponibilidad> Disponibilidades { get; set; }
}
