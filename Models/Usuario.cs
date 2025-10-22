// Models/Usuario.cs

using System.ComponentModel.DataAnnotations;

namespace StayGo.Models;
public class Usuario
{
    public Guid Id { get; set; }
    [EmailAddress]
    public string Email { get; set; } = "";

    // Vinculación con ASP.NET Identity (tabla AspNetUsers)
    public string IdentityUserId { get; set; } = "";
    public ApplicationUser IdentityUser { get; set; } = null!;
    public bool EsAdmin { get; set; }

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<Resena> Resenas { get; set; } = new List<Resena>();
    public ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
}