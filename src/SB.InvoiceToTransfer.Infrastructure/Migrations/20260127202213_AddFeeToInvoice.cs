using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SB.InvoiceToTransfer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFeeToInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Fee",
                table: "Invoices",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fee",
                table: "Invoices");
        }
    }
}
