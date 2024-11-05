using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sub_App_1.Migrations
{
    /// <inheritdoc />
    public partial class RecreateAllergensColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Allergens",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Allergens",
                table: "Products");
        }
    }
}
