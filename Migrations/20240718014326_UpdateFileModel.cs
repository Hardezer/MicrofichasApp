using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microfichas_App.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFileModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AzureDocumentId",
                table: "Files",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AzureToken",
                table: "Files",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AzureDocumentId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AzureToken",
                table: "Files");
        }
    }
}
