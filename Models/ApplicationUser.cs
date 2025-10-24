using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace StayGo.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public Usuario? Usuario { get; set; }
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<Resena> Resenas { get; set; } = new List<Resena>();
    public ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
}