using Microsoft.EntityFrameworkCore;

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
    }
}