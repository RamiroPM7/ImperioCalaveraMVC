namespace ImperioCalaveraMVC.Models.Entities
{
    public class CitaPromocion
    {

        public int CitaId { get; set; }//Foereign key
        public required Cita Cita { get; set; }//Object of cita who is related to this service

        public int PromocionId { get; set; }//Foereign key
        public required Promocion Promocion { get; set; }//object of service who is related to this cita

    }
}
