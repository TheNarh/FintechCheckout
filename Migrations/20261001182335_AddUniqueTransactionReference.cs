using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FintechCheckout.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueTransactionReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Reference",
                table: "Transactions",
                column: "Reference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Reference",
                table: "Transactions");
        }
    }
}
