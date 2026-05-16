using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nimble.Modulith.Products.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                schema: "Products",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                schema: "Products",
                table: "Products");
        }
    }
}
