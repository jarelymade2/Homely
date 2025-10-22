using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayGo.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration

    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Amenidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenidades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogsAuditoria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Entidad = table.Column<string>(type: "TEXT", nullable: false),
                    EntidadId = table.Column<string>(type: "TEXT", nullable: false),
                    Accion = table.Column<string>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DatosAntes = table.Column<string>(type: "TEXT", nullable: true),
                    DatosDespues = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsAuditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Propiedades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioPorNoche = table.Column<decimal>(type: "TEXT", nullable: true),
                    Capacidad = table.Column<int>(type: "INTEGER", nullable: false),
                    Lat = table.Column<double>(type: "REAL", nullable: false),
                    Lng = table.Column<double>(type: "REAL", nullable: false),
                    Direccion_Pais = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Direccion_Ciudad = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Direccion_Linea1 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Direccion_Linea2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Direccion_CodigoPostal = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Propiedades", x => x.Id);
                });

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
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Habitaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropiedadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Capacidad = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioPorNoche = table.Column<decimal>(type: "TEXT", nullable: false),
                    Piso = table.Column<int>(type: "INTEGER", nullable: true),
                    Numero = table.Column<string>(type: "TEXT", nullable: true),
                    TipoCama = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Habitaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Habitaciones_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImagenesPropiedad",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropiedadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    EsPrincipal = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagenesPropiedad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImagenesPropiedad_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropiedadAmenidades",
                columns: table => new
                {
                    PropiedadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AmenidadId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropiedadAmenidades", x => new { x.PropiedadId, x.AmenidadId });
                    table.ForeignKey(
                        name: "FK_PropiedadAmenidades_Amenidades_AmenidadId",
                        column: x => x.AmenidadId,
                        principalTable: "Amenidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropiedadAmenidades_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "Favoritos",
                columns: table => new
                {
                    UsuarioId = table.Column<string>(type: "TEXT", nullable: false),
                    PropiedadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioId1 = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoritos", x => new { x.UsuarioId, x.PropiedadId });
                    table.ForeignKey(
                        name: "FK_Favoritos_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favoritos_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favoritos_Usuario_UsuarioId1",
                        column: x => x.UsuarioId1,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Resenas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropiedadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<string>(type: "TEXT", nullable: false),
                    Puntuacion = table.Column<int>(type: "INTEGER", nullable: false),
                    Comentario = table.Column<string>(type: "TEXT", nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioId1 = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resenas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resenas_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Resenas_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Resenas_Usuario_UsuarioId1",
                        column: x => x.UsuarioId1,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Disponibilidades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropiedadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HabitacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Desde = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Hasta = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disponibilidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Disponibilidades_Habitaciones_HabitacionId",
                        column: x => x.HabitacionId,
                        principalTable: "Habitaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Disponibilidades_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropiedadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HabitacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UsuarioId = table.Column<string>(type: "TEXT", nullable: false),
                    CheckIn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    CheckOut = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Huespedes = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId1 = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservas_Habitaciones_HabitacionId",
                        column: x => x.HabitacionId,
                        principalTable: "Habitaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservas_Usuario_UsuarioId1",
                        column: x => x.UsuarioId1,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReservaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Monto = table.Column<decimal>(type: "TEXT", nullable: false),
                    Moneda = table.Column<string>(type: "TEXT", nullable: false),
                    Metodo = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    TransaccionRef = table.Column<string>(type: "TEXT", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualizadoEn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "Reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contactos_UsuarioId",
                table: "Contactos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Disponibilidades_HabitacionId",
                table: "Disponibilidades",
                column: "HabitacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Disponibilidades_PropiedadId",
                table: "Disponibilidades",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_PropiedadId",
                table: "Favoritos",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_UsuarioId1",
                table: "Favoritos",
                column: "UsuarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_Habitaciones_PropiedadId",
                table: "Habitaciones",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_ImagenesPropiedad_PropiedadId",
                table: "ImagenesPropiedad",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ReservaId",
                table: "Pagos",
                column: "ReservaId");

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadAmenidades_AmenidadId",
                table: "PropiedadAmenidades",
                column: "AmenidadId");

            migrationBuilder.CreateIndex(
                name: "IX_Propiedades_PrecioPorNoche",
                table: "Propiedades",
                column: "PrecioPorNoche");

            migrationBuilder.CreateIndex(
                name: "IX_Propiedades_Tipo_Capacidad",
                table: "Propiedades",
                columns: new[] { "Tipo", "Capacidad" });

            migrationBuilder.CreateIndex(
                name: "IX_Resenas_PropiedadId",
                table: "Resenas",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_Resenas_UsuarioId",
                table: "Resenas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Resenas_UsuarioId1",
                table: "Resenas",
                column: "UsuarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_HabitacionId",
                table: "Reservas",
                column: "HabitacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_PropiedadId",
                table: "Reservas",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_UsuarioId",
                table: "Reservas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_UsuarioId1",
                table: "Reservas",
                column: "UsuarioId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Contactos");

            migrationBuilder.DropTable(
                name: "Disponibilidades");

            migrationBuilder.DropTable(
                name: "Favoritos");

            migrationBuilder.DropTable(
                name: "ImagenesPropiedad");

            migrationBuilder.DropTable(
                name: "LogsAuditoria");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "PropiedadAmenidades");

            migrationBuilder.DropTable(
                name: "Resenas");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Amenidades");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Habitaciones");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Propiedades");
        }
    }
}
