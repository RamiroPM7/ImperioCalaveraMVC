using System.Globalization;
using ImperioCalaveraMVC.Data;
using ImperioCalaveraMVC.Hubs;
using ImperioCalaveraMVC.Models;
using ImperioCalaveraMVC.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static ImperioCalaveraMVC.Models.Enums.Enums;

namespace ImperioCalaveraMVC.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _DbContext;
        private readonly IHubContext<AppointmentHub> _hubContext;

        public AppointmentsController(UserManager<Usuario> userManager, 
            RoleManager<IdentityRole> roleManager, ApplicationDbContext context,
            IHubContext<AppointmentHub> hubContext
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _DbContext = context;
            _hubContext = hubContext;
        }

        

       public async Task<IActionResult> Appointment(int pageIndex = 1) // Accept a page number
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Auth");
            }

            var pageSize = 10; // Show 10 appointments per page

            // 1. Start with the base query (same as before)
            IQueryable<Cita> query = _DbContext.Citas.AsQueryable();

            // 2. Apply role-based filter (same as before)
            if (User.IsInRole("Admin")) { /* No filter */ }
            else if (User.IsInRole("Barbero")) { query = query.Where(c => c.BarberoId == user.Id); }
            else { query = query.Where(c => c.ClienteId == user.Id); }

            // 3. Get the TOTAL count of matching appointments BEFORE pagination
            var totalCount = await query.CountAsync();

            // 4. Apply sorting AND pagination to the query
            var citasForPage = await query
                .OrderBy(c => c.Estado == EstadoCita.Finalizada || c.Estado == EstadoCita.Cancelada)
                .ThenBy(c => c.FechaHora)
                .Skip((pageIndex - 1) * pageSize) // Skip the records of previous pages
                .Take(pageSize)                   // Take only the records for the current page
                .Select(c => new AppointmentCardViewModel
                {
                    CitaId = c.CitaId,
                    FechaHora = c.FechaHora,
                    ClienteName = c.Cliente.Nombre,
                    ClientePhone = c.Cliente.PhoneNumber,
                    ClienteTelEmergencia = c.Cliente.TelefonoEmergencia,
                    BarberoName = c.Barbero.Nombre,
                    Estado = c.Estado,
                    WalkInCustomerName = c.WalkInCustomerName,
                    WalkInCustomerPhone = c.WalkInCustomerPhone,
                    WalkInCustomerNotes = c.WalkInCustomerNotes
                })
                .ToListAsync();

            // 5. Create the final model to send to the view
            var paginatedModel = new PaginatedAppointmentsViewModel
            {
                Appointments = citasForPage,
                PageIndex = pageIndex,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return View(paginatedModel);
        }

        [Authorize] // Ensure only logged-in users can see slots
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(string date)
        {
            // 1. Validate and parse the incoming date
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var selectedDate))
            {
                return BadRequest("Invalid date format. Please use yyyy-MM-dd.");
            }

            // 2. Define Business Logic
            var openingTime = new TimeSpan(9, 0, 0);  // 9:00 AM
            var closingTime = new TimeSpan(21, 0, 0); // 9:00 PM
            var appointmentDuration = 45; // in minutes
            var availableSlots = new List<string>();

            // 3. Get all appointments already booked for that day
            var startOfDay = selectedDate.Date;
            var endOfDay = startOfDay.AddDays(1);
            

            var existingAppointments = await _DbContext.Citas
                        .Where(c => c.FechaHora >= startOfDay && c.FechaHora < endOfDay
                                   && (c.Estado == EstadoCita.Pendiente || c.Estado == EstadoCita.Iniciada)) // <-- THIS IS THE FIX
                        .Select(c => c.FechaHora)
                        .ToListAsync();

            // 4. Generate all potential slots and filter out booked ones
            var currentTime = startOfDay.Add(openingTime);
            while (currentTime.TimeOfDay < closingTime)
            {
                // Check if this slot conflicts with an existing appointment
                bool isBooked = existingAppointments.Any(bookedTime =>
                    bookedTime.TimeOfDay == currentTime.TimeOfDay
                );

                // Add a check to see if the potential slot is in the past.
                if (!isBooked && currentTime > DateTime.Now)
                {
                    availableSlots.Add(currentTime.ToString("HH:mm"));
                }

                currentTime = currentTime.AddMinutes(appointmentDuration);
            }

            return Json(availableSlots);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BookAppointment([FromBody] BookingViewModel model)
        {
            // 1. Initial validation (remains the same)
            if (model == null || string.IsNullOrEmpty(model.Date) || string.IsNullOrEmpty(model.Time))
            {
                return BadRequest("Date and time are required.");
            }
            var dateTimeStr = $"{model.Date} {model.Time}";
            if (!DateTime.TryParseExact(dateTimeStr, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var appointmentDateTime))
            {
                return BadRequest("Invalid date or time format.");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // 2. Automatic Barber Assignment Logic (remains the same)
            var allBarbers = await _userManager.GetUsersInRoleAsync("Barbero");
            if (!allBarbers.Any())
            {
                return BadRequest("No barbers are available for booking.");
            }
            var appointmentsForDay = await _DbContext.Citas
                .Where(c => c.FechaHora.Date == appointmentDateTime.Date && (c.Estado == EstadoCita.Pendiente || c.Estado == EstadoCita.Iniciada))
                .ToListAsync();
            var leastBookedBarber = allBarbers
                .Select(barber => new {
                    Barber = barber,
                    Count = appointmentsForDay.Count(a => a.BarberoId == barber.Id)
                })
                .OrderBy(x => x.Count)
                .First();

            // 3. NEW: Determine appointment details based on user role FIRST
            string clienteId;
            string successMessage;
            string? walkInName = null;
            string? walkInPhone = null;
            string? walkInNotes = null;

            if (User.IsInRole("Admin") || User.IsInRole("Barbero"))
            {
                if (string.IsNullOrEmpty(model.WalkInCustomerName) || string.IsNullOrEmpty(model.WalkInCustomerPhone))
                {
                    return BadRequest("El nombre y teléfono del cliente son necesarios");
                }

                clienteId = user.Id;
                walkInName = model.WalkInCustomerName;
                walkInPhone = model.WalkInCustomerPhone;
                walkInNotes = model.WalkInCustomerNotes;
                successMessage = $"Cita agendada para {model.WalkInCustomerName} exitosamente!";
            }
            else
            {
                clienteId = user.Id;
                successMessage = "Tu cita ha sido agendada exitosamente!";
            }

            // 4. NOW, create the appointment object once all data is ready
            var newAppointment = new Cita
            {
                // Required properties
                ClienteId = clienteId, // This now satisfies the compiler
                BarberoId = leastBookedBarber.Barber.Id,
                FechaHora = appointmentDateTime,
                HoraInicioReal = appointmentDateTime,
                HoraFinReal = appointmentDateTime.AddMinutes(45),

                // Walk-in properties (will be null for regular clients, which is correct)
                WalkInCustomerName = walkInName,
                WalkInCustomerPhone = walkInPhone,
                WalkInCustomerNotes = walkInNotes,

                // Default properties
                Estado = EstadoCita.Pendiente,
                FechaCreacion = DateTime.UtcNow,
            };

            // 5. Save to database
            _DbContext.Citas.Add(newAppointment);
            await _DbContext.SaveChangesAsync();


            // --- START OF NEW REAL-TIME LOGIC ---

            // 6. Broadcast the new appointment card to relevant users
            // First, create the ViewModel for the card we want to send
            var newAppointmentCard = new AppointmentCardViewModel
            {
                CitaId = newAppointment.CitaId,
                FechaHora = newAppointment.FechaHora,
                ClienteName = user.Nombre, // The name of the person who booked
                ClientePhone = user.PhoneNumber,
                ClienteTelEmergencia = user.TelefonoEmergencia,
                BarberoName = leastBookedBarber.Barber.Nombre,
                Estado = newAppointment.Estado,
                WalkInCustomerName = newAppointment.WalkInCustomerName,
                WalkInCustomerPhone = newAppointment.WalkInCustomerPhone,
                WalkInCustomerNotes = newAppointment.WalkInCustomerNotes
            };

            // Next, get a list of all user IDs that need to see this new card
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var userIdsToNotify = adminUsers.Select(a => a.Id).ToList();
            userIdsToNotify.Add(newAppointment.ClienteId); // Add the client
            userIdsToNotify.Add(newAppointment.BarberoId); // Add the barber

            // Finally, send the new card data only to those specific users
            await _hubContext.Clients.Users(userIdsToNotify.Distinct()).SendAsync("AddNewAppointmentCard", newAppointmentCard);

            // --- END OF NEW REAL-TIME LOGIC ---

            // Broadcast that this specific time slot on this date is now taken.
            await _hubContext.Clients.All.SendAsync("SlotBooked", model.Date, model.Time);

            successMessage = successMessage.Replace("!", $" con {leastBookedBarber.Barber.Nombre}!");

            return Json(new { success = true, message = successMessage });
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            // 1. Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // 2. Find the requested appointment
            var appointment = await _DbContext.Citas.FindAsync(id);

            if (appointment == null)
            {
                return NotFound(new { success = false, message = "Cita no encontrada." });
            }

            // 3. UPDATED SECURITY CHECK:
            // An Admin can cancel any appointment. Other users can only cancel their own.
            if (!User.IsInRole("Admin") && appointment.ClienteId != user.Id)
            {
                // This is an unauthorized attempt for a non-admin user!
                return Forbid();
            }

            // 4. Business Logic Check: Only allow canceling pending appointments
            if (appointment.Estado != EstadoCita.Pendiente)
            {
                return BadRequest(new { success = false, message = "Solo puedes cancelar citas que están pendientes." });
            }

            // 5. Perform the "Cancel" (Soft Delete)
            appointment.Estado = EstadoCita.Cancelada;
            await _DbContext.SaveChangesAsync();

            // --- START OF NEW CODE ---
            // 6. Broadcast the status update to all connected clients
            await _hubContext.Clients.All.SendAsync("ReceiveStatusUpdate", appointment.CitaId, EstadoCita.Cancelada.ToString());
            // --- END OF NEW CODE ---



            // Customize the success message if an admin is canceling someone else's appointment
            string message = (user.Id == appointment.ClienteId)
                ? "Tu cita ha sido cancelada exitosamente."
                : "La cita ha sido cancelada exitosamente por el administrador.";

            return Json(new { success = true, message = message });
        }


    }


}
