namespace ImperioCalaveraMVC.Models.Entities
{
    public class ServicioPromocion
    {
        public int ServicioId { get; set; }
        public required Servicio Servicio { get; set; }

        public int PromocionId { get; set; }
        public required Promocion Promocion { get; set; }


    }
}
