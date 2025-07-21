using static ImperioCalaveraMVC.Models.Enums.Enums;
using System.ComponentModel.DataAnnotations;

namespace ImperioCalaveraMVC.Models.Entities
{
    public class Reporte
    {
        [Key]
        public int ReporteId { get; set; }

        [Required]
        public ReporteTipo TipoReporte { get; set; }

        [Required]
        public DateTime FechaGenerado { get; set; }

        [Required]
        public required string DatosJSON { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
