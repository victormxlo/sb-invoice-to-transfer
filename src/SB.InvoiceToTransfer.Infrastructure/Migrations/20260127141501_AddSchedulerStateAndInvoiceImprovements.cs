using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SB.InvoiceToTransfer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulerStateAndInvoiceImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Invoices",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "TransferId",
                table: "Invoices",
                type: "TEXT",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "InvoiceSchedulerStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceSchedulerStates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSchedulerStates_IsActive",
                table: "InvoiceSchedulerStates",
                column: "IsActive",
                unique: true,
                filter: "IsActive = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceSchedulerStates");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TransferId",
                table: "Invoices");
        }
    }
}
