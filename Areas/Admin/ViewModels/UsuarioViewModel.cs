namespace StayGo.ViewModels.Admin
{
    // Esta clase contendrá la información de CADA fila de tu tabla de usuarios
    public class UsuarioAdminViewModel
    {
        public string Id { get; set; } = ""; // Este será el Id de ApplicationUser
        public string NombreCompleto { get; set; } = "";
        public string Email { get; set; } = "";
        public int TotalReservas { get; set; }
        public bool EsAdmin { get; set; }
    }
}