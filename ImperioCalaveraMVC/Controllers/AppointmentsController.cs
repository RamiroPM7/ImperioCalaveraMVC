using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ImperioCalaveraMVC.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AppointmentsController(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Appointment()
        {
            return View();
        }
    }
}
