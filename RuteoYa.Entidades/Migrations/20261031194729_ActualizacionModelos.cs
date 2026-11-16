using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuteoYa.Entidades.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionModelos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Apellido", "CreadoEn", "Email", "EnvioId", "Nombre", "PasswordHash", "ViajeId" },
                values: new object[] { 1, "Sistema", new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@ruteoya.com", 0, "Admin", "$2a$11$xKvLhR4QZJvKZN8Y5Y8wWeF.0KK/jZqR6HjF4V5hFhqE3YHxMYGhK", 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
