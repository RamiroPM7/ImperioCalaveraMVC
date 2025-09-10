namespace ImperioCalaveraMVC.Models
{
    public class PaginatedAppointmentsViewModel
    {

        // The list of appointments for the current page
        public List<AppointmentCardViewModel> Appointments { get; set; }

        // Information for the page number controls
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }

        // Helper properties to make the view code cleaner
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

    }
}
