using static ImperioCalaveraMVC.Models.Enums.Enums;
using System.ComponentModel.DataAnnotations;

namespace ImperioCalaveraMVC.Models.Entities
{
    public class Disponibilidad
    {
        [Key]
        public int DisponibilidadId { get; set; }

        [Required]
        public required string BarberoId { get; set; }
        public required Usuario Barbero { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public bool Desocupado { get; set; } = true;

        /*[Required]
        public BloqueoTipo TipoBloqueo { get; set; }*/
    }
}
