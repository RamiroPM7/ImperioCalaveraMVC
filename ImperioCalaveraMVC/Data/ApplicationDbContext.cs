using ImperioCalaveraMVC.Models.Entities;
using Microsoft.AspNetCore.Identity; // Necesario para IdentityUserLogin, IdentityUserRole, IdentityUserToken
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ImperioCalaveraMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario>
    {
        //Constructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        //DbSets (¡QUITADO DbSet<Usuario>!)
        // public DbSet<Usuario> Usuarios { get; set; } // <-- ¡ELIMINA ESTA LÍNEA! IdentityDbContext ya lo tiene
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Reporte> Reportes { get; set; }
        public DbSet<Promocion> Promociones { get; set; }
        public DbSet<Disponibilidad> Disponibilidades { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<CitaServicio> CitasServicios { get; set; }
        public DbSet<CitaPromocion> CitasPromociones { get; set; }
        public DbSet<ServicioPromocion> ServiciosPromociones { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ¡CRÍTICO! Llama al método OnModelCreating de la clase base (IdentityDbContext)
            // Esto configura todas las tablas de Identity (AspNetUsers, AspNetRoles, etc.)
            base.OnModelCreating(modelBuilder);

            // Renombrar tablas de Identity si lo deseas (opcional)
            // modelBuilder.Entity<Usuario>().ToTable("Users");
            // modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            // etc.

            // Relación Cita - Usuario (Cliente)
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Cliente)
                .WithMany(u => u.CitasComoCliente)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Barbero)
                .WithMany(u => u.CitasComoBarbero)
                .HasForeignKey(c => c.BarberoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Cita - Servicio (CitaServicio)
            modelBuilder.Entity<CitaServicio>()
                .HasKey(cs => new { cs.CitaId, cs.ServicioId }); // Clave compuesta

            modelBuilder.Entity<CitaServicio>()//Servicio
                .HasOne(cs => cs.Servicio)
                .WithMany(s => s.CitaServicios)
                .HasForeignKey(cs => cs.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CitaServicio>()//Cita
                .HasOne(cs => cs.Cita)
                .WithMany(c => c.CitaServicios)
                .HasForeignKey(cs => cs.CitaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Cita - Promocion (CitaPromocion)
            modelBuilder.Entity<CitaPromocion>()
                .HasKey(cp => new { cp.CitaId, cp.PromocionId }); // Clave compuesta

            modelBuilder.Entity<CitaPromocion>()//Promocion
                .HasOne(cp => cp.Promocion)
                .WithMany(cp => cp.CitaPromociones)
                .HasForeignKey(cp => cp.PromocionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CitaPromocion>()//Cita
                .HasOne(cp => cp.Cita)
                .WithMany(c => c.CitaPromociones)
                .HasForeignKey(cp => cp.CitaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Disponibilidad - Usuario
            modelBuilder.Entity<Disponibilidad>() //Disponibilidad
                .HasOne(d => d.Barbero)
                .WithMany(b => b.Disponibilidades)
                .HasForeignKey(d => d.BarberoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Servicio - Promocion
            modelBuilder.Entity<ServicioPromocion>()
                .HasKey(sp => new { sp.ServicioId, sp.PromocionId }); // Clave compuesta

            modelBuilder.Entity<ServicioPromocion>()
                .HasOne(sp => sp.Servicio)
                .WithMany(s => s.ServicioPromociones)
                .HasForeignKey(sp => sp.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServicioPromocion>()//Promocion
                .HasOne(sp => sp.Promocion)
                .WithMany(p => p.ServicioPromociones)
                .HasForeignKey(sp => sp.PromocionId)
                .OnDelete(DeleteBehavior.Restrict);


            // Estas líneas suelen ser manejadas por base.OnModelCreating(modelBuilder);
            // Puedes comentarlas o eliminarlas y verificar si se generan advertencias/errores en las migraciones.
            // Si no hay problemas, es mejor quitarlas para mantener el código más limpio.
            /*
            modelBuilder.Entity<IdentityUserLogin<string>>()
                .HasKey(x => new { x.LoginProvider, x.ProviderKey });

            modelBuilder.Entity<IdentityUserRole<string>>()
                .HasKey(x => new { x.UserId, x.RoleId });

            modelBuilder.Entity<IdentityUserToken<string>>()
                .HasKey(x => new { x.UserId, x.LoginProvider, x.Name });
            */
        }
    }
}
