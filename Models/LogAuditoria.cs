namespace StayGo.Models;
public class LogAuditoria
{
    public Guid Id { get; set; }
    public string Entidad { get; set; } = "";
    public string EntidadId { get; set; } = "";
    public string Accion { get; set; } = ""; // Create/Update/Delete/StatusChange
    public Guid? UsuarioId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? DatosAntes { get; set; } // JSON
    public string? DatosDespues { get; set; } // JSON
}
