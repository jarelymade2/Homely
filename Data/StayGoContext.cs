using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StayGo.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace StayGo.Data;

public class StayGoContext : IdentityDbContext<ApplicationUser>
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
    public DbSet<ContactoMensaje> Contactos => Set<ContactoMensaje>();
    


    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<Propiedad>().OwnsOne(p => p.Direccion, d =>
        {
            d.Property(x => x.Pais).HasMaxLength(80);
            d.Property(x => x.Ciudad).HasMaxLength(120);
            d.Property(x => x.Linea1).HasMaxLength(200);
            d.Property(x => x.Linea2).HasMaxLength(200);
            d.Property(x => x.CodigoPostal).HasMaxLength(20);
        });

        mb.Entity<Propiedad>().HasIndex(p => new { p.Tipo, p.Capacidad });
        mb.Entity<Propiedad>().HasIndex(p => p.PrecioPorNoche);

        mb.Entity<Favorito>().HasKey(f => new { f.UsuarioId, f.PropiedadId });
        mb.Entity<Favorito>()
            .HasOne(f => f.Usuario)
            .WithMany(u => u.Favoritos)
            .HasForeignKey(f => f.UsuarioId);

        mb.Entity<PropiedadAmenidad>().HasKey(pa => new { pa.PropiedadId, pa.AmenidadId });
        mb.Entity<PropiedadAmenidad>()
            .HasOne(pa => pa.Propiedad)
            .WithMany(p => p.PropiedadAmenidades)
            .HasForeignKey(pa => pa.PropiedadId);
        mb.Entity<PropiedadAmenidad>()
            .HasOne(pa => pa.Amenidad)
            .WithMany(a => a.PropiedadAmenidades)
            .HasForeignKey(pa => pa.AmenidadId);

        mb.Entity<ImagenPropiedad>()
            .HasOne(ip => ip.Propiedad)
            .WithMany(p => p.Imagenes)
            .HasForeignKey(ip => ip.PropiedadId);

        mb.Entity<Disponibilidad>()
            .HasOne(d => d.Propiedad)
            .WithMany(p => p.Disponibilidades)
            .HasForeignKey(d => d.PropiedadId);
        mb.Entity<Disponibilidad>()
            .HasOne(d => d.Habitacion)
            .WithMany(h => h.Disponibilidades)
            .HasForeignKey(d => d.HabitacionId)
            .OnDelete(DeleteBehavior.Restrict);

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

        mb.Entity<Pago>()
            .HasOne(p => p.Reserva)
            .WithMany(r => r.Pagos)
            .HasForeignKey(p => p.ReservaId);

        mb.Entity<Resena>()
            .HasOne(x => x.Propiedad)
            .WithMany(p => p.Resenas)
            .HasForeignKey(x => x.PropiedadId);
        mb.Entity<Resena>()
            .HasOne(x => x.Usuario)
            .WithMany(u => u.Resenas)
            .HasForeignKey(x => x.UsuarioId);
        
        mb.Entity<Propiedad>().Property(p => p.Titulo).HasMaxLength(200).IsRequired();
        mb.Entity<Amenidad>().Property(a => a.Nombre).HasMaxLength(120).IsRequired();
        // -------------------------
        // ValueConverter: DateOnly
        // -------------------------
        // Guardamos DateOnly como DateTime (fecha a medianoche) para compatibilidad con SQLite/otros.
        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
            d => d.ToDateTime(TimeOnly.MinValue),
            dt => DateOnly.FromDateTime(dt));

        // Si hace falta DateOnly?nullable (DateOnly?)
        var nullableDateOnlyConverter = new ValueConverter<DateOnly?, DateTime?>(
            d => d.HasValue ? d.Value.ToDateTime(TimeOnly.MinValue) : null,
            dt => dt.HasValue ? DateOnly.FromDateTime(dt.Value) : null);

        // -------------------------
        // Propiedad -> Direccion (Owned)
        // -------------------------
        mb.Entity<Propiedad>().OwnsOne(p => p.Direccion, d =>
        {
            d.Property(x => x.Pais).HasMaxLength(80);
            d.Property(x => x.Ciudad).HasMaxLength(120);
            d.Property(x => x.Linea1).HasMaxLength(200);
            d.Property(x => x.Linea2).HasMaxLength(200);
            d.Property(x => x.CodigoPostal).HasMaxLength(20);
        });

        // -------------------------
        // Índices
        // -------------------------
        mb.Entity<Propiedad>().HasIndex(p => new { p.Tipo, p.Capacidad });
        mb.Entity<Propiedad>().HasIndex(p => p.PrecioPorNoche);

        // -------------------------
        // Keys y relaciones
        // -------------------------
        mb.Entity<Favorito>().HasKey(f => new { f.UsuarioId, f.PropiedadId });
        mb.Entity<Favorito>()
            .HasOne(f => f.Usuario)
            .WithMany(u => u.Favoritos)
            .HasForeignKey(f => f.UsuarioId);

        mb.Entity<PropiedadAmenidad>().HasKey(pa => new { pa.PropiedadId, pa.AmenidadId });
        mb.Entity<PropiedadAmenidad>()
            .HasOne(pa => pa.Propiedad)
            .WithMany(p => p.PropiedadAmenidades)
            .HasForeignKey(pa => pa.PropiedadId);
        mb.Entity<PropiedadAmenidad>()
            .HasOne(pa => pa.Amenidad)
            .WithMany(a => a.PropiedadAmenidades)
            .HasForeignKey(pa => pa.AmenidadId);

        mb.Entity<ImagenPropiedad>()
            .HasOne(ip => ip.Propiedad)
            .WithMany(p => p.Imagenes)
            .HasForeignKey(ip => ip.PropiedadId);

        mb.Entity<Disponibilidad>()
            .HasOne(d => d.Propiedad)
            .WithMany(p => p.Disponibilidades)
            .HasForeignKey(d => d.PropiedadId);

        mb.Entity<Disponibilidad>()
            .HasOne(d => d.Habitacion)
            .WithMany(h => h.Disponibilidades)
            .HasForeignKey(d => d.HabitacionId)
            .OnDelete(DeleteBehavior.Restrict);

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

        mb.Entity<Pago>()
            .HasOne(p => p.Reserva)
            .WithMany(r => r.Pagos)
            .HasForeignKey(p => p.ReservaId);

        mb.Entity<Resena>()
            .HasOne(x => x.Propiedad)
            .WithMany(p => p.Resenas)
            .HasForeignKey(x => x.PropiedadId);

        mb.Entity<Resena>()
            .HasOne(x => x.Usuario)
            .WithMany(u => u.Resenas)
            .HasForeignKey(x => x.UsuarioId);

        // -------------------------
        // Propiedades adicionales: longitudes y decimales
        // -------------------------
        // Si tienes columnas decimales como PrecioPorNoche / PrecioTotal, define precisión:
        // (ajusta la precisión/escala según tus necesidades)
        mb.Entity<Propiedad>().Property(p => p.PrecioPorNoche).HasPrecision(12, 2);
        mb.Entity<Reserva>().Property(r => r.PrecioTotal).HasPrecision(12, 2);
        mb.Entity<Pago>().Property(p => p.Monto).HasPrecision(12, 2);

        // -------------------------
        // Bind DateOnly (aplicar converter donde corresponda)
        // -------------------------
        // Reserva usa DateOnly (CheckIn / CheckOut)
        mb.Entity<Reserva>().Property(r => r.CheckIn).HasConversion(dateOnlyConverter);
        mb.Entity<Reserva>().Property(r => r.CheckOut).HasConversion(dateOnlyConverter);

        // Si hay otras entidades con DateOnly, aplica el converter similarmente.
        // -------------------------
        // Restricciones / Longitud de strings
        // -------------------------
        mb.Entity<Propiedad>().Property(p => p.Titulo).HasMaxLength(200).IsRequired();
        mb.Entity<Amenidad>().Property(a => a.Nombre).HasMaxLength(120).IsRequired();

        // -------------------------
        // (Opcional) Valores por defecto o configuraciones extra
        // -------------------------
        // Ejemplo: si quieres valor por defecto para Estado en la BD, podrías:
        // mb.Entity<Reserva>().Property(r => r.Estado).HasDefaultValue(EstadoReserva.Pendiente);

        // Fin OnModelCreating
        mb.Entity<ApplicationUser>()
        .HasOne(au => au.Usuario) // <-- CORRECCIÓN: Especifica la propiedad
        .WithOne(u => u.IdentityUser) 
        .HasForeignKey<Usuario>(u => u.IdentityUserId);
    }
}