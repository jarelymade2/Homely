using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayGo.Migrations
{
    /// <inheritdoc />
    public partial class Sync_ContactoMensaje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Baseline: la tabla Contactos ya existe, no hacemos nada.
            // (Opcional) Si quisieras validar su existencia, podrías hacerlo con SQL crudo.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Baseline inverso: no dropear nada.
        }
    }
}
