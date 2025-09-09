using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StayGo.Models;

namespace StayGo.Data;

public class StayGoContext : IdentityDbContext<IdentityUser>
{
    public StayGoContext(DbContextOptions<StayGoContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Propiedad> Propiedades => Set<Propiedad>();
    public DbSet<Habitacion> Habitaciones => Set<Habitacion>();
    public DbSet<ImagenPropiedad> ImagenesPropiedad => Set<ImagenPropiedad>();
    public DbSet<Amenidad> Amenidades => Set<Amenidad>();
    public DbSet<PropiedadAmenidad> PropiedadAmenidades => Set<PropiedadAmenidad>();
    public DbSet<Disponibilidad> Disponibilidades => Set<Disponibilidad>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Resena> Resenas => Set<Resena>();
    public DbSet<Favorito> Favoritos => Set<Favorito>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb); // necesario para Identity

        // Owned type: Direccion dentro de Propiedad (NO crea tabla aparte)
        mb.Entity<Propiedad>().OwnsOne(p => p.Direccion, d =>
        {
            d.Property(x => x.Pais).HasMaxLength(80);
            d.Property(x => x.Ciudad).HasMaxLength(120);
            d.Property(x => x.Linea1).HasMaxLength(200);
            d.Property(x => x.Linea2).HasMaxLength(200);
            d.Property(x => x.CodigoPostal).HasMaxLength(20);
        });

        // Índices útiles
        mb.Entity<Propiedad>().HasIndex(p => new { p.Tipo, p.Capacidad });
        mb.Entity<Propiedad>().HasIndex(p => p.PrecioPorNoche);

        // Favorito (PK compuesta)
        mb.Entity<Favorito>().HasKey(f => new { f.UsuarioId, f.PropiedadId });

        // PropiedadAmenidad (N–N con PK compuesta)
        mb.Entity<PropiedadAmenidad>().HasKey(pa => new { pa.PropiedadId, pa.AmenidadId });
        mb.Entity<PropiedadAmenidad>()
            .HasOne(pa => pa.Propiedad)
            .WithMany(p => p.PropiedadAmenidades)
            .HasForeignKey(pa => pa.PropiedadId);
        mb.Entity<PropiedadAmenidad>()
            .HasOne(pa => pa.Amenidad)
            .WithMany(a => a.PropiedadAmenidades)
            .HasForeignKey(pa => pa.AmenidadId);

        // ImagenPropiedad → Propiedad
        mb.Entity<ImagenPropiedad>()
            .HasOne(ip => ip.Propiedad)
            .WithMany(p => p.Imagenes)
            .HasForeignKey(ip => ip.PropiedadId);

        // Disponibilidad → Propiedad / Habitacion (opcional)
        mb.Entity<Disponibilidad>()
            .HasOne(d => d.Propiedad)
            .WithMany(p => p.Disponibilidades)
            .HasForeignKey(d => d.PropiedadId);
        mb.Entity<Disponibilidad>()
            .HasOne(d => d.Habitacion)
            .WithMany(h => h.Disponibilidades)
            .HasForeignKey(d => d.HabitacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Reserva → Propiedad / Habitacion / Usuario
        mb.Entity<Reserva>()
            .HasOne(r => r.Propiedad)
            .WithMany(p => p.Reservas)
            .HasForeignKey(r => r.PropiedadId);
        mb.Entity<Reserva>()
            .HasOne(r => r.Habitacion)
            .WithMany(h => h.Reservas)
            .HasForeignKey(r => r.HabitacionId)
            .OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Reserva>()
            .HasOne(r => r.Usuario)
            .WithMany(u => u.Reservas)
            .HasForeignKey(r => r.UsuarioId);

        // Pago → Reserva (1–N)
        mb.Entity<Pago>()
            .HasOne(p => p.Reserva)
            .WithMany(r => r.Pagos)
            .HasForeignKey(p => p.ReservaId);

        // Reseña → Propiedad / Usuario
        mb.Entity<Resena>()
            .HasOne(x => x.Propiedad)
            .WithMany(p => p.Resenas)
            .HasForeignKey(x => x.PropiedadId);
        mb.Entity<Resena>()
            .HasOne(x => x.Usuario)
            .WithMany(u => u.Resenas)
            .HasForeignKey(x => x.UsuarioId);

        // Reglas de esquema simples
        mb.Entity<Propiedad>().Property(p => p.Titulo).HasMaxLength(200).IsRequired();
        mb.Entity<Amenidad>().Property(a => a.Nombre).HasMaxLength(120).IsRequired();

        // Usuario ↔ AspNetUsers (FK por IdentityUserId)
        mb.Entity<Usuario>()
          .HasOne<IdentityUser>()          // sin navegación inversa
          .WithMany()
          .HasForeignKey(u => u.IdentityUserId)
          .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Usuario>()
          .Property(u => u.Email).HasMaxLength(256).IsRequired();

        mb.Entity<Usuario>()
          .HasIndex(u => u.Email).IsUnique();

        mb.Entity<Usuario>()
          .HasIndex(u => u.IdentityUserId).IsUnique();

        // Reglas de negocio más complejas (hotel vs no hotel) se validan en capa de aplicación.
    }
}
