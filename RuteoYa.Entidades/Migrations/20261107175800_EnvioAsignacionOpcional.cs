using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuteoYa.Entidades.Migrations
{
    /// <inheritdoc />
    public partial class EnvioAsignacionOpcional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Envios_Asignaciones_AsignacionId",
                table: "Envios");

            migrationBuilder.DropColumn(
                name: "EnvioEstadoHistorialId",
                table: "Envios");

            migrationBuilder.AlterColumn<int>(
                name: "AsignacionId",
                table: "Envios",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Envios_Asignaciones_AsignacionId",
                table: "Envios",
                column: "AsignacionId",
                principalTable: "Asignaciones",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Envios_Asignaciones_AsignacionId",
                table: "Envios");

            migrationBuilder.AlterColumn<int>(
                name: "AsignacionId",
                table: "Envios",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EnvioEstadoHistorialId",
                table: "Envios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Envios_Asignaciones_AsignacionId",
                table: "Envios",
                column: "AsignacionId",
                principalTable: "Asignaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
