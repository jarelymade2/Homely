using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayGo.Migrations
{
    /// <inheritdoc />
    public partial class LinkUsuarioToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQLite doesn't support some operations in transactions
            migrationBuilder.Sql("PRAGMA foreign_keys = OFF;", suppressTransaction: true);

            migrationBuilder.DropForeignKey(
                name: "FK_Contactos_Usuario_UsuarioId",
                table: "Contactos");

            migrationBuilder.DropForeignKey(
                name: "FK_Favoritos_Usuario_UsuarioId1",
                table: "Favoritos");

            migrationBuilder.DropForeignKey(
                name: "FK_Resenas_Usuario_UsuarioId1",
                table: "Resenas");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Usuario_UsuarioId1",
                table: "Reservas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuario",
                table: "Usuario");

            migrationBuilder.RenameTable(
                name: "Usuario",
                newName: "Usuarios");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_IdentityUserId",
                table: "Usuarios",
                column: "IdentityUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contactos_Usuarios_UsuarioId",
                table: "Contactos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Favoritos_Usuarios_UsuarioId1",
                table: "Favoritos",
                column: "UsuarioId1",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Resenas_Usuarios_UsuarioId1",
                table: "Resenas",
                column: "UsuarioId1",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Usuarios_UsuarioId1",
                table: "Reservas",
                column: "UsuarioId1",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_AspNetUsers_IdentityUserId",
                table: "Usuarios",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("PRAGMA foreign_keys = ON;", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contactos_Usuarios_UsuarioId",
                table: "Contactos");

            migrationBuilder.DropForeignKey(
                name: "FK_Favoritos_Usuarios_UsuarioId1",
                table: "Favoritos");

            migrationBuilder.DropForeignKey(
                name: "FK_Resenas_Usuarios_UsuarioId1",
                table: "Resenas");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Usuarios_UsuarioId1",
                table: "Reservas");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_AspNetUsers_IdentityUserId",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_IdentityUserId",
                table: "Usuarios");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "Usuario");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuario",
                table: "Usuario",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contactos_Usuario_UsuarioId",
                table: "Contactos",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Favoritos_Usuario_UsuarioId1",
                table: "Favoritos",
                column: "UsuarioId1",
                principalTable: "Usuario",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Resenas_Usuario_UsuarioId1",
                table: "Resenas",
                column: "UsuarioId1",
                principalTable: "Usuario",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Usuario_UsuarioId1",
                table: "Reservas",
                column: "UsuarioId1",
                principalTable: "Usuario",
                principalColumn: "Id");
        }
    }
}
