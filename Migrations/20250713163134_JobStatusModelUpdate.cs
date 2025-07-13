using Microsoft.EntityFrameworkCore.Migrations;
using tradetrackr.api.Models;

#nullable disable

namespace tradetrackr.api.Migrations
{
    /// <inheritdoc />
    public partial class JobStatusModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:job_status", "not_started,in_progress,completed,cancelled,on_hold");

            migrationBuilder.Sql("ALTER TABLE \"Jobs\" ALTER COLUMN \"Status\" DROP DEFAULT;");          

            migrationBuilder.AlterColumn<JobStatus>(
                name: "Status",
                table: "Jobs",
                type: "job_status USING \"Status\"::job_status",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.Sql("ALTER TABLE \"Jobs\" ALTER COLUMN \"Status\" SET DEFAULT 'not_started';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:job_status", "not_started,in_progress,completed,cancelled,on_hold");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Jobs",
                type: "text",
                nullable: false,
                oldClrType: typeof(JobStatus),
                oldType: "job_status");
        }
    }
}
