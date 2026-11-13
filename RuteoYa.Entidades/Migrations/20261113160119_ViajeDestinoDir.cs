using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuteoYa.Entidades.Migrations
{
    /// <inheritdoc />
    public partial class ViajeDestinoDir : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DestinoDir",
                table: "Viajes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinoDir",
                table: "Viajes");
        }
    }
}
