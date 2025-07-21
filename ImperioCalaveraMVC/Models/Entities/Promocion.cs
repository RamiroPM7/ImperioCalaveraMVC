using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ImperioCalaveraMVC.Models.Entities
{
    public class Promocion
    {
        public Promocion()
        {
           // Citas = new List<Cita>();
            CitaPromociones = new List<CitaPromocion>();
            ServicioPromociones = new List<ServicioPromocion>();
        }

        [Key]
        public int PromocionId { get; set; }

      /*  [Required]
        public int ServicioId { get; set; }
        public required Servicio Servicio { get; set; }*/

        [Required, MaxLength(200)]
        public required string Descripcion { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal PrecioPromocional { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public required bool Eliminada { get; set; } = false;

        // Navegación
        // public ICollection<Cita> Citas { get; set; }

        public ICollection<CitaPromocion> CitaPromociones { get; set; }

        public ICollection<ServicioPromocion> ServicioPromociones { get; set; }
    }
}
