using ImperioCalaveraMVC.Models;
using ImperioCalaveraMVC.Models.Entities; // Asegúrate de que tu clase Usuario esté aquí
using ImperioCalaveraMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims; // Para usar FindFirstValue

namespace ImperioCalaveraMVC.Controllers
{
    [Authorize] // Solo usuarios autenticados pueden acceder a este controlador
    public class ProfileController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager; // Necesario para re-autenticación

        public ProfileController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Profile/Index
        // Muestra los datos del perfil del usuario actual
        public async Task<IActionResult> Profile()
        {
            // Obtener el usuario actual
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Si el usuario no se encuentra (raro si está autorizado), redirigir al login
                return RedirectToAction("Index", "Auth");
            }

            // Mapear los datos del usuario al ViewModel para mostrarlos en la vista
            var model = new ProfileViewModel
            {
                Nombre = user.Nombre,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                TelefonoEmergencia = user.TelefonoEmergencia
            };

            return View(model);
        }

        // POST: /Profile/VerifyPassword
        // Verifica la contraseña actual del usuario antes de permitir la edición
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Password))
            {
                return Json(new { success = false, message = "La contraseña es obligatoria." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado." });
            }

            // CheckPasswordAsync verifica la contraseña sin iniciar sesión
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (isPasswordValid)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Contraseña incorrecta." });
            }
        }

        // POST: /Profile/Edit
        // Actualiza los datos del perfil del usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            // No validamos ModelState.IsValid aquí porque los campos pueden estar vacíos
            // si el usuario no los editó todos. La validación se hará al actualizar el usuario.

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Auth"); // O manejar error
            }

            // Actualizar solo las propiedades que se permiten editar
            user.Nombre = model.Nombre;
            user.Email = model.Email; // UserManager se encarga de la unicidad del email
            user.PhoneNumber = model.PhoneNumber;
            user.TelefonoEmergencia = model.TelefonoEmergencia;

            // Intentar actualizar el usuario
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                // Si hay errores de validación de Identity (ej. email duplicado),
                // volvemos a la vista con el modelo y los errores.
                return View("Profile", model); // Volver a la vista Index con el modelo y errores
            }

            // Si el email fue cambiado, es posible que necesites re-generar la cookie de autenticación
            // para que los claims (como el email en el UserNameClaimType) se actualicen.
            await _signInManager.RefreshSignInAsync(user);

            // Si también se va a cambiar la contraseña
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                // En un escenario real, aquí deberías verificar la contraseña actual de nuevo
                // antes de permitir el cambio de contraseña, o tener un flujo separado para ello.
                // Por simplicidad, este ejemplo asume que la contraseña actual ya fue verificada
                // al entrar en modo edición, o que este cambio de contraseña es opcional.

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, $"Error al cambiar contraseña: {error.Description}");
                    }
                    return View("Profile", model);
                }
            }

            TempData["SuccessMessage"] = "¡Perfil actualizado exitosamente!";
            return RedirectToAction("Profile");
        }

        
    }
}
