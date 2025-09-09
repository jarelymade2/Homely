using Microsoft.EntityFrameworkCore;
using StayGo.Models;

namespace StayGo.Data
{
    public class StayGoContext : DbContext
    {
        public StayGoContext(DbContextOptions<StayGoContext> options)
            : base(options)
        {
        }

        // Agrega el DbSet para la entidad Propiedades
        public DbSet<Propiedad> Propiedades { get; set; }
         public DbSet<Reserva> Reservas { get; set; } 
    }
}