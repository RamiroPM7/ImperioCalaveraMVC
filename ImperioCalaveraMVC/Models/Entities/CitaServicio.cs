using System.ComponentModel.DataAnnotations;

namespace ImperioCalaveraMVC.Models.Entities
{
    public class CitaServicio
    {
        //Clase que nos permite tener una relación muchos a muchos con cita y servicio

        public int CitaId { get; set; }//Foereign key
        public required Cita Cita { get; set; }//Object of cita who is related to this service

        public int ServicioId { get; set; }//Foereign key
        public required Servicio Servicio { get; set; }//object of service who is related to this cita

    }
}
