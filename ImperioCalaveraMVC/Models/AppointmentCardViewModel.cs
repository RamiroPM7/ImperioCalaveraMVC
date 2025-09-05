using System.ComponentModel.DataAnnotations;
using static ImperioCalaveraMVC.Models.Enums.Enums;
namespace ImperioCalaveraMVC.Models

{
    public class AppointmentCardViewModel
    {
      
            public int CitaId { get; set; }
            public required string ClienteName { get; set; }
            public required string BarberoName { get; set; }
            public required DateTime FechaHora { get; set; }
            public required EstadoCita Estado { get; set; }
            public required string ClientePhone { get; set; }
            public required string? ClienteTelEmergencia { get; set; }//opcional
            
            // === ADD THESE MISSING PROPERTIES ===
            public string? WalkInCustomerName { get; set; }
            public string? WalkInCustomerPhone { get; set; }
            public string? WalkInCustomerNotes { get; set; }//opcional


    }
}
