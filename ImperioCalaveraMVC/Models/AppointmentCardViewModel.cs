using static ImperioCalaveraMVC.Models.Enums.Enums;
namespace ImperioCalaveraMVC.Models

{
    public class AppointmentCardViewModel
    {
      
            public int CitaId { get; set; }
            public required string ClienteName { get; set; }
            public required string BarberoName { get; set; }
            public required DateTime FechaHora { get; set; }
            public EstadoCita Estado{ get; set; }
        

    }
}
