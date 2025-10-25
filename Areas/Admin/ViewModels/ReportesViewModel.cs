// ViewModels/ReportesViewModel.cs
namespace StayGo.ViewModels.Admin
{
    // DTO para el gráfico de barras
    public class ReservasPorMesDto
    {
        public string MesAno { get; set; } = "";
        public int Cantidad { get; set; }
    }

    // DTO para la tabla de propiedades top
    public class PropiedadMasReservadaDto
    {
        public Guid PropiedadId { get; set; }
        public string NombrePropiedad { get; set; } = "";
        public int CantidadReservas { get; set; }
    }

    // DTO para la tabla de usuarios top
    public class UsuarioMasActivoDto
    {
        public string UsuarioId { get; set; } = "";
        public string EmailUsuario { get; set; } = "";
        public int CantidadReservas { get; set; }
    }

    // El ViewModel principal para la vista de Reportes
    public class ReportesViewModel
    {
        public List<ReservasPorMesDto> ReservasPorMes { get; set; } = new();
        public List<PropiedadMasReservadaDto> TopPropiedades { get; set; } = new();
        public List<UsuarioMasActivoDto> TopUsuarios { get; set; } = new();
    }
}