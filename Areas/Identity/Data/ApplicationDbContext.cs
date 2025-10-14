using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StayGo.Models; // <-- Necesitas este 'using' para ApplicationUser

namespace StayGo.Areas.Identity.Data // <-- ¡AJUSTAR ESTE NAMESPACE!
{
    // ¡DEBE HEREDAR DE TU CLASE PERSONALIZADA ApplicationUser!
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // ... (resto del código)
        }
    }
}