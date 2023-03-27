using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eShop.Migrations
{
    /// <inheritdoc />
    public partial class cartItemMigrations2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BasketId",
                table: "cartItems",
                newName: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_cartItems_CartId",
                table: "cartItems",
                column: "CartId");

            migrationBuilder.AddForeignKey(
                name: "FK_cartItems_carts_CartId",
                table: "cartItems",
                column: "CartId",
                principalTable: "carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cartItems_carts_CartId",
                table: "cartItems");

            migrationBuilder.DropIndex(
                name: "IX_cartItems_CartId",
                table: "cartItems");

            migrationBuilder.RenameColumn(
                name: "CartId",
                table: "cartItems",
                newName: "BasketId");
        }
    }
}
