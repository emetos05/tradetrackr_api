using Microsoft.EntityFrameworkCore.Migrations;
using System.Net.NetworkInformation;
using tradetrackr.api.Models;

#nullable disable

namespace tradetrackr.api.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceStatusModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:invoice_status", "draft,sent,paid,overdue,cancelled")
                .Annotation("Npgsql:Enum:job_status", "not_started,in_progress,completed,cancelled,on_hold")
                .OldAnnotation("Npgsql:Enum:job_status", "not_started,in_progress,completed,cancelled,on_hold");

            migrationBuilder.Sql("ALTER TABLE \"Invoices\" ALTER COLUMN \"Status\" DROP DEFAULT;");

            migrationBuilder.AlterColumn<InvoiceStatus>(
                name: "Status",
                table: "Invoices",
                type: "invoice_status USING \"Status\"::invoice_status",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.Sql("ALTER TABLE \"Invoices\" ALTER COLUMN \"Status\" SET DEFAULT 'draft';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:job_status", "not_started,in_progress,completed,cancelled,on_hold")
                .OldAnnotation("Npgsql:Enum:invoice_status", "draft,sent,paid,overdue,cancelled")
                .OldAnnotation("Npgsql:Enum:job_status", "not_started,in_progress,completed,cancelled,on_hold");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Invoices",
                type: "text",
                nullable: false,
                oldClrType: typeof(InvoiceStatus),
                oldType: "invoice_status");
        }
    }
}
