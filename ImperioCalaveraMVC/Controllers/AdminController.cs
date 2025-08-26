using System.Linq;
using System.Security.Claims; // Para usar ClaimTypes
using System.Threading.Tasks;
using ImperioCalaveraMVC.Models;
using ImperioCalaveraMVC.Models.Entities; // Asegúrate de que tu clase Usuario esté aquí
using ImperioCalaveraMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Necesario para SelectListItem
using Microsoft.EntityFrameworkCore;
using UserCardViewModel = ImperioCalaveraMVC.Models.UserCardViewModel;
using UserEditViewModel = ImperioCalaveraMVC.Models.UserEditViewModel;

namespace ImperioCalaveraMVC.Controllers
{
    [Authorize(Roles = "Admin")] // Solo los usuarios con el rol "Admin" pueden acceder a este controlador
    public class AdminController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Index (EXISTENTE)
        // Muestra la lista de usuarios (Barberos y Admins) en tarjetas.
        public async Task<IActionResult> Admin()
        {
            var userCards = new List<UserCardViewModel>();

            // Obtener todos los usuarios
            var users = await _userManager.Users.ToListAsync();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Filtrar para mostrar solo usuarios que son "Barbero" o "Admin"
                // Si quieres mostrar TODOS los usuarios, elimina esta condición 'if'
                if (roles.Contains("Barbero") || roles.Contains("Admin"))
                {
                    userCards.Add(new UserCardViewModel
                    {
                        Id = user.Id,
                        Nombre = user.Nombre,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        IsLockedOut = await _userManager.IsLockedOutAsync(user),
                        LockoutEnd = user.LockoutEnd, // La fecha de fin de bloqueo de Identity
                        Roles = roles.ToList()
                    });
                }
            }

            return View(userCards);
        }

        // GET: Admin/GetUserForEdit (EXISTENTE)
        [HttpGet]
        public async Task<IActionResult> GetUserForEdit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new UserEditViewModel
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                TelefonoEmergencia = user.TelefonoEmergencia,
                LockoutStartDate = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow ? DateTime.UtcNow : (DateTime?)null,
                LockoutEndDate = user.LockoutEnd?.LocalDateTime.Date,
                Roles = userRoles.ToList(),
            };

            return Json(model);
        }

        // POST: Admin/EditUser (EXISTENTE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser([FromBody] UserEditViewModel model)
        {
            // ... (código existente para EditUser) ...
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado." });
            }

            user.Nombre = model.Nombre;
            user.PhoneNumber = model.PhoneNumber;
            user.TelefonoEmergencia = model.TelefonoEmergencia;

            if (user.Email != model.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    var errors = setEmailResult.Errors.Select(e => e.Description).ToList();
                    return Json(new { success = false, errors = errors });
                }
                if (user.UserName == user.Email)
                {
                    var setUserNameResult = await _userManager.SetUserNameAsync(user, model.Email);
                    if (!setUserNameResult.Succeeded)
                    {
                        var errors = setUserNameResult.Errors.Select(e => e.Description).ToList();
                        return Json(new { success = false, errors = errors });
                    }
                }
            }

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (!TryValidateModel(model))
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, errors = errors });
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetPasswordResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (!resetPasswordResult.Succeeded)
                {
                    var errors = resetPasswordResult.Errors.Select(e => e.Description).ToList();
                    return Json(new { success = false, errors = errors });
                }
            }

            if (model.LockoutStartDate.HasValue && model.LockoutEndDate.HasValue && model.LockoutEndDate.Value > model.LockoutStartDate.Value)
            {
                var lockoutEndDateUtc = new DateTimeOffset(model.LockoutEndDate.Value.AddDays(1), TimeSpan.Zero);
                await _userManager.SetLockoutEndDateAsync(user, lockoutEndDateUtc);
                await _userManager.SetLockoutEnabledAsync(user, true);
            }
            else if (model.LockoutStartDate.HasValue || model.LockoutEndDate.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Ambas fechas de bloqueo (inicio y fin) deben ser válidas y la fecha de fin debe ser posterior a la de inicio.");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, errors = errors });
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.SetLockoutEnabledAsync(user, true);
            }

            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                return Json(new { success = true, message = "Perfil actualizado exitosamente." });
            }
            else
            {
                var errors = updateResult.Errors.Select(e => e.Description).ToList();
                return Json(new { success = false, errors = errors });
            }
        }

        // POST: Admin/ToggleLockout (EXISTENTE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLockout(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado." });
            }

            if (User.FindFirstValue(ClaimTypes.NameIdentifier) == user.Id)
            {
                return Json(new { success = false, message = "No puedes bloquear tu propia cuenta de administrador desde esta acción." });
            }

            var isLockedOut = await _userManager.IsLockedOutAsync(user);

            if (isLockedOut)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                return Json(new { success = true, message = "Usuario desbloqueado exitosamente." });
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                await _userManager.SetLockoutEnabledAsync(user, true);
                return Json(new { success = true, message = "Usuario bloqueado exitosamente." });
            }
        }

        // POST: Admin/DeleteUser (EXISTENTE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado." });
            }

            if (User.FindFirstValue(ClaimTypes.NameIdentifier) == user.Id)
            {
                return Json(new { success = false, message = "No puedes eliminar tu propia cuenta de administrador." });
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Usuario eliminado exitosamente." });
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Json(new { success = false, errors = errors });
            }
        }

        // NUEVO: GET: Admin/CreateUser
        // Muestra el formulario para crear un nuevo usuario
        public async Task<IActionResult> CreateUser()
        {
            var model = new CreateUserViewModel();
            // Obtener solo los roles que queremos permitir asignar desde esta interfaz
            var rolesToAssign = new List<string> { "Admin", "Barbero" };

            // Asegurarse de que estos roles existan antes de intentar obtenerlos
            foreach (var roleName in rolesToAssign)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            model.AvailableRoles = await _roleManager.Roles
                                                    .Where(r => rolesToAssign.Contains(r.Name))
                                                    .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                                                    .ToListAsync();
            // return View(model);
            return RedirectToAction("Admin", "Admin");
        }

        // NUEVO: POST: Admin/CreateUser
        // Procesa el formulario para crear un nuevo usuario y asignarle un rol
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            // Volver a poblar los roles disponibles si el ModelState no es válido
            // para que el dropdown no esté vacío al regresar a la vista
            var rolesToAssign = new List<string> { "Admin", "Barbero" };
            model.AvailableRoles = await _roleManager.Roles
                                                    .Where(r => rolesToAssign.Contains(r.Name))
                                                    .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                                                    .ToListAsync();

            if (ModelState.IsValid)
            {
                var user = new Usuario
                {
                    UserName = model.Email, // Usar Email como UserName para Identity
                    Email = model.Email,
                    Nombre = model.Nombre,
                    PhoneNumber = model.PhoneNumber,
                    TelefonoEmergencia = model.TelefonoEmergencia,
                    LockoutEnabled = false // Asume que esta propiedad existe y es false por defecto
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Asignar el rol seleccionado al usuario
                    var roleResult = await _userManager.AddToRoleAsync(user, model.SelectedRole);

                    if (roleResult.Succeeded)
                    {
                        TempData["SuccessMessage"] = $"Usuario '{user.Email}' creado y asignado al rol '{model.SelectedRole}' exitosamente.";
                        return RedirectToAction("Admin"); // Redirigir a la lista de usuarios
                    }
                    else
                    {
                        // Si falla la asignación de rol, eliminar el usuario recién creado para evitar huérfanos
                        await _userManager.DeleteAsync(user);
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, $"Error al asignar rol: {error.Description}");
                        }
                    }
                }
                else
                {
                    // Si falla la creación del usuario
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // Si llegamos a este punto, algo falló, volver a mostrar el formulario con errores
            return View(model);
        }


    }
}
