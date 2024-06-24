using Microsoft.EntityFrameworkCore.Migrations;

namespace Microfichas_App.Migrations
{
    public partial class AddFileUrlParts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Solo añade nuevas columnas a la tabla 'Files'
            migrationBuilder.AddColumn<string>(
                name: "Server",
                table: "Files",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContainerPath",
                table: "Files",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullFileName",
                table: "Files",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar las columnas en el método Down para revertir la migración si es necesario
            migrationBuilder.DropColumn(
                name: "Server",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "ContainerPath",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "FullFileName",
                table: "Files");
        }
    }
}
