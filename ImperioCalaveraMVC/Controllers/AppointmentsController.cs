using ImperioCalaveraMVC.Data;
using ImperioCalaveraMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImperioCalaveraMVC.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _DbContext;

        public AppointmentsController(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _DbContext = context;
        }

        public async Task<IActionResult> Appointment()
        {
            // Obtener el usuario actual
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Si el usuario no se encuentra (raro si está autorizado), redirigir al login
                return RedirectToAction("Index", "Auth");
            }

            var cita = await _DbContext.Citas
                .Where(c =>c.ClienteId == user.Id)
                .OrderByDescending(c => c.FechaHora)
                .Select(c => new
                {
                    c.CitaId,
                    c.FechaHora,
                    ClienteNombre = c.Cliente.Nombre,
                    BarberoNombre = c.Barbero.Nombre,
                    EstadoCita = c.Estado
                })
                .FirstOrDefaultAsync();


            if (cita == null) {

                ViewBag.Mensaje = "No tienes citas";
                return View();

            }

            var model = new AppointmentCardViewModel
            { 
               CitaId = cita.CitaId,
               ClienteName = cita.ClienteNombre,
               BarberoName = cita.BarberoNombre,
               FechaHora = cita.FechaHora,
               Estado = cita.EstadoCita

            };


            return View(model);
        }
    
    
    
    }


}
