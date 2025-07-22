using ImperioCalaveraMVC.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ¡CAMBIO CRÍTICO AQUÍ!
// Cambiado de AddDefaultIdentity<IdentityUser> a AddIdentity<Usuario, IdentityRole>
builder.Services.AddIdentity<Usuario, IdentityRole>(options => // Especifica tu clase Usuario y IdentityRole
{
    options.SignIn.RequireConfirmedAccount = false; // Mantén esta opción si quieres confirmación de cuenta
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>() // Conecta Identity con tu DbContext
.AddDefaultTokenProviders(); // ¡Importante! Añade los proveedores de tokens por defecto

// Configuración explícita de la cookie de autenticación (mantener)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.LoginPath = "/Auth/Index"; // Ruta a tu controlador/acción de Login
    options.LogoutPath = "/Auth/Logout"; // Ruta para tu controlador/acción de Logout
    options.AccessDeniedPath = "/Auth/AccessDenied"; // Ruta para acceso denegado
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Para HTTPS. En HTTP dev, usa .SameAsRequest.
});

builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // ¡CRÍTICO! Debe ir antes de UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Home}/{id?}"); // Tu ruta predeterminada, ahora apunta a Auth/Login
// app.MapRazorPages(); // Elimina o comenta esta línea si no usas las Razor Pages de Identity UI

// ¡NUEVO! Lógica para inicializar roles (mantener aquí)
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    // Si también necesitas el UserManager para crear un usuario admin inicial, inyéctalo aquí:
    // var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

    // Crear roles si no existen
    string[] roleNames = { "Admin", "Barbero", "Cliente" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
    // Opcional: Crear un usuario administrador inicial y asignarle un rol
    // var adminUser = await userManager.FindByEmailAsync("admin@tuapp.com");
    // if (adminUser == null)
    // {
    //     adminUser = new Usuario { UserName = "admin@tuapp.com", Email = "admin@tuapp.com", Nombre = "Administrador" /*, otras propiedades */ };
    //     var createAdminResult = await userManager.CreateAsync(adminUser, "TuContrasenaSegura123!"); // ¡CAMBIA ESTO!
    //     if (createAdminResult.Succeeded)
    //     {
    //         await userManager.AddToRoleAsync(adminUser, "Admin");
    //     }
    // }
}

app.Run();
