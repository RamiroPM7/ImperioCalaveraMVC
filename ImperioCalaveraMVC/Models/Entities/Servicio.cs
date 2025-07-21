using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ImperioCalaveraMVC.Models.Entities
{
    public class Servicio
    {

        public Servicio()
        {
            ServicioPromociones = new List<ServicioPromocion>();
           // Citas = new List<Cita>();
            CitaServicios = new List<CitaServicio>();
        }

        [Key]
        public int ServicioId { get; set; }

        [Required, MaxLength(100)]
        public required string Nombre { get; set; }

        [Required]
        public int DuracionMinutos { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal PrecioBase { get; set; }

        public required bool Eliminado { get; set; } = false;

        // Navegación
        public ICollection<ServicioPromocion> ServicioPromociones { get; set; }
       // public ICollection<Cita> Citas { get; set; }
        public ICollection<CitaServicio> CitaServicios { get; set; }
        

    }
}
