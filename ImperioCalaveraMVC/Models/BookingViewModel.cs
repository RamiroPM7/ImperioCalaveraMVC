namespace ImperioCalaveraMVC.Models
{
    // This model is for RECEIVING data to create an appointment
    public class BookingViewModel
    {
        public required string Date { get; set; }     // e.g., "2025-09-05"
        public required string Time { get; set; }     // e.g., "10:30"

        // optional Walk-in fields
        public required string WalkInCustomerName { get; set; }
        public required string WalkInCustomerPhone { get; set; } // <-- ADD THIS LINE
        public string? WalkInCustomerNotes { get; set; }

    }
}
