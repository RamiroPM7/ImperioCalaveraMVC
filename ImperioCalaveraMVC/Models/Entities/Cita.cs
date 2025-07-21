using static ImperioCalaveraMVC.Models.Enums.Enums;
using System.ComponentModel.DataAnnotations;

namespace ImperioCalaveraMVC.Models.Entities
{
    public class Cita
    {

        public Cita()
        {
            CitaServicios = new List<CitaServicio>();
            CitaPromociones = new List<CitaPromocion>();
        }

        [Key]
        public int CitaId { get; set; }

        [Required]
        public required string ClienteId { get; set; }

        public required Usuario Cliente { get; set; }

        [Required]
        public required string BarberoId { get; set; }
        public required Usuario Barbero { get; set; }

       /* [Required]
        public int ServicioId { get; set; }
        public required Servicio Servicio { get; set; }

        public int? PromocionId { get; set; }
        public Promocion? Promocion { get; set; }*/

        [Required]
        public DateTime FechaHora { get; set; }

        [Required]
        public DateTime HoraInicioReal { get; set; }
        
        [Required]
        public DateTime HoraFinReal { get; set; }

        [Required]
        public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;

        [MaxLength(300)]
        public string? Observaciones { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public ICollection<CitaServicio> CitaServicios { get; set; }

        public ICollection<CitaPromocion> CitaPromociones { get; set; }
    }
}
