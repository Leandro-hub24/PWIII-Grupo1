using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuteoYa.Entidades.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ViajeId = table.Column<int>(type: "int", nullable: false),
                    EnvioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Viajes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    OrigenDir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrigenLat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrigenLng = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DestinoLat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DestinoLng = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HoraSalida = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraLlegada = table.Column<TimeOnly>(type: "time", nullable: false),
                    MaxPaquets = table.Column<int>(type: "int", nullable: false),
                    PesoMaxKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DesvioMaxKm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DistanciaMin = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    AsignacionId = table.Column<int>(type: "int", nullable: false),
                    ViajeDiaId = table.Column<int>(type: "int", nullable: false),
                    ParadaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Viajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Viajes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Asignaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViajeId = table.Column<int>(type: "int", nullable: false),
                    FechaViaje = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesvioKm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TiempoExtraMin = table.Column<int>(type: "int", nullable: false),
                    Ganancia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AsignadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asignaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asignaciones_Viajes_ViajeId",
                        column: x => x.ViajeId,
                        principalTable: "Viajes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paradas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViajeId = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Lng = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paradas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paradas_Viajes_ViajeId",
                        column: x => x.ViajeId,
                        principalTable: "Viajes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViajeDias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViajeId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViajeDias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViajeDias_Viajes_ViajeId",
                        column: x => x.ViajeId,
                        principalTable: "Viajes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Envios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    OrigenDir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrigenLat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrigenLng = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DestinoDir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinoLat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DestinoLng = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaLimite = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PesoKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tamano = table.Column<byte>(type: "tinyint", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte>(type: "tinyint", nullable: false),
                    AsignacionId = table.Column<int>(type: "int", nullable: false),
                    EnvioEstadoHistorialId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Envios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Envios_Asignaciones_AsignacionId",
                        column: x => x.AsignacionId,
                        principalTable: "Asignaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Envios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EnvioEstadoHistoriales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnvioId = table.Column<int>(type: "int", nullable: false),
                    EstadoAnterior = table.Column<byte>(type: "tinyint", nullable: false),
                    EstadoNuevo = table.Column<byte>(type: "tinyint", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvioEstadoHistoriales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnvioEstadoHistoriales_Envios_EnvioId",
                        column: x => x.EnvioId,
                        principalTable: "Envios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asignaciones_ViajeId",
                table: "Asignaciones",
                column: "ViajeId");

            migrationBuilder.CreateIndex(
                name: "IX_EnvioEstadoHistoriales_EnvioId",
                table: "EnvioEstadoHistoriales",
                column: "EnvioId");

            migrationBuilder.CreateIndex(
                name: "IX_Envios_AsignacionId",
                table: "Envios",
                column: "AsignacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Envios_UsuarioId",
                table: "Envios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Paradas_ViajeId",
                table: "Paradas",
                column: "ViajeId");

            migrationBuilder.CreateIndex(
                name: "IX_ViajeDias_ViajeId",
                table: "ViajeDias",
                column: "ViajeId");

            migrationBuilder.CreateIndex(
                name: "IX_Viajes_UsuarioId",
                table: "Viajes",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnvioEstadoHistoriales");

            migrationBuilder.DropTable(
                name: "Paradas");

            migrationBuilder.DropTable(
                name: "ViajeDias");

            migrationBuilder.DropTable(
                name: "Envios");

            migrationBuilder.DropTable(
                name: "Asignaciones");

            migrationBuilder.DropTable(
                name: "Viajes");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
