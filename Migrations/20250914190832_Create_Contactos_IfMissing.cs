using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayGo.Migrations
{
    /// <inheritdoc />
    public partial class Create_Contactos_IfMissing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQLite: crea la tabla e índice SOLO si no existen
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""Contactos"" (
                  ""Id"" TEXT NOT NULL CONSTRAINT ""PK_Contactos"" PRIMARY KEY,
                  ""Nombre"" TEXT NOT NULL,
                  ""Email"" TEXT NOT NULL,
                  ""Mensaje"" TEXT NOT NULL,
                  ""IdentityUserId"" TEXT NULL,
                  ""UsuarioId"" TEXT NULL,
                  ""FechaUtc"" TEXT NOT NULL,
                  CONSTRAINT ""FK_Contactos_Usuarios_UsuarioId""
                    FOREIGN KEY (""UsuarioId"") REFERENCES ""Usuarios"" (""Id"")
                    ON DELETE SET NULL
                );
                CREATE INDEX IF NOT EXISTS ""IX_Contactos_UsuarioId""
                  ON ""Contactos"" (""UsuarioId"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // no-op: como la creación fue condicional, no dropeamos nada
        }
    }
}
