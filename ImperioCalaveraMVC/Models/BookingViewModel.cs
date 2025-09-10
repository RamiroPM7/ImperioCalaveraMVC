using System.ComponentModel.DataAnnotations;

namespace ImperioCalaveraMVC.Models
{
    // This model is for RECEIVING data to create an appointment
    public class BookingViewModel
    {
        [Required]
        public required string Date { get; set; }
        [Required] 
        public required string Time { get; set; }     // e.g., "10:30"

        // optional Walk-in fields
        public string?  WalkInCustomerName { get; set; }
        public string?  WalkInCustomerPhone { get; set; } // <-- ADD THIS LINE
        public string? WalkInCustomerNotes { get; set; }

    }
}
