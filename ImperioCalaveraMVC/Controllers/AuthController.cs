using ImperioCalaveraMVC.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static ImperioCalaveraMVC.Models.Enums.Enums;

namespace ImperioCalaveraMVC.Controllers
{
    public class AuthController : Controller
    {

        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated) // Safe null check
            {
                return RedirectToAction("Home", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return LocalRedirect(returnUrl ?? "/Home/Home");
                }
                
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Cuenta bloqueada. Inténtalo de nuevo más tarde.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Intento de inicio de sesión no válido.");
                }
            }

            return RedirectToAction("Index");
        }

        // GET: /Auth/Register
        // Muestra el formulario de registro
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        // Procesa el envío del formulario de registro
        [HttpPost]
        [ValidateAntiForgeryToken] // ¡CRÍTICO para la seguridad!
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Crea una nueva instancia de tu entidad Usuario
                var user = new Usuario
                {
                    UserName = model.Email, // Identity usa UserName para el login, a menudo es el Email
                    Email = model.Email,
                    Nombre = model.Nombre,
                    PhoneNumber = model.Telefono,
                    TelefonoEmergencia = model.TelefonoEmergencia,                    
                };

                // Intenta crear el usuario en la base de datos con la contraseña proporcionada
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Define el nombre del rol por defecto
                    const string defaultRole = "Admin";

                    // Verifica si el rol "Cliente" existe. Si no existe, lo crea.
                    if (!await _roleManager.RoleExistsAsync(defaultRole))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(defaultRole));
                    }

                    // Asigna el rol "Cliente" al usuario recién registrado
                    var roleAssignmentResult = await _userManager.AddToRoleAsync(user, defaultRole);

                    if (!roleAssignmentResult.Succeeded)
                    {
                        // Si la asignación del rol falla, puedes loggear el error o añadirlo al ModelState
                        foreach (var error in roleAssignmentResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, $"Error al asignar rol: {error.Description}");
                        }
                        
                    }

                    // Redirige al usuario a una página de confirmación o al login
                    TempData["SuccessMessage"] = "¡Registro exitoso! Ahora puedes iniciar sesión.";
                    return RedirectToAction("Index");
                }

                // Si hay errores (ej. email ya registrado, contraseña no cumple requisitos)
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Si el ModelState no es válido o hubo errores de Identity,
            // vuelve a mostrar la vista de registro con los errores.
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken] // ¡CRÍTICO! Protege contra ataques CSRF.
        public async Task<IActionResult> Logout()
        {
            // Cierra la sesión del usuario actual.
            // Esto elimina la cookie de autenticación del navegador.
            await _signInManager.SignOutAsync();

            // Redirige al usuario a la página de inicio (o a la de login si lo prefieres).
            // "Index" es la acción, "Home" es el controlador.
            return RedirectToAction("Index", "Auth");
            // O si quieres redirigir al login:
            // return RedirectToAction("Login", "Auth");
        }

    }
}
