using System.ComponentModel.DataAnnotations;

namespace StayGo.Models;

public class ContactoMensaje
{
    public Guid Id { get; set; }

    [Required, StringLength(100)]
    public string Nombre { get; set; } = "";

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = "";

    [Required, StringLength(1000)]
    public string Mensaje { get; set; } = "";

    // ⭐ Enlace con Identity / dominio (opcionales si no hay login)
    public string? IdentityUserId { get; set; }
    public Guid? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;
}
