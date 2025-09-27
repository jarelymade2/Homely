using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayGo.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioId1",
                table: "Reservas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioId1",
                table: "Resenas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioId1",
                table: "Favoritos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    IdentityUserId = table.Column<string>(type: "TEXT", nullable: false),
                    EsAdmin = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contactos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Mensaje = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    IdentityUserId = table.Column<string>(type: "TEXT", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contactos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contactos_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_UsuarioId1",
                table: "Reservas",
                column: "UsuarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_Resenas_UsuarioId1",
                table: "Resenas",
                column: "UsuarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_UsuarioId1",
                table: "Favoritos",
                column: "UsuarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_Contactos_UsuarioId",
                table: "Contactos",
                column: "UsuarioId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favoritos_Usuario_UsuarioId1",
                table: "Favoritos");

            migrationBuilder.DropForeignKey(
                name: "FK_Resenas_Usuario_UsuarioId1",
                table: "Resenas");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Usuario_UsuarioId1",
                table: "Reservas");

            migrationBuilder.DropTable(
                name: "Contactos");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_UsuarioId1",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Resenas_UsuarioId1",
                table: "Resenas");

            migrationBuilder.DropIndex(
                name: "IX_Favoritos_UsuarioId1",
                table: "Favoritos");

            migrationBuilder.DropColumn(
                name: "UsuarioId1",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "UsuarioId1",
                table: "Resenas");

            migrationBuilder.DropColumn(
                name: "UsuarioId1",
                table: "Favoritos");
        }
    }
}
