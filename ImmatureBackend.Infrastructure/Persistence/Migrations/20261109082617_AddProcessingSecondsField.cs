using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImmatureBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessingSecondsField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "processing_seconds",
                table: "replicates",
                type: "numeric(10,1)",
                precision: 10,
                scale: 1,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "processing_seconds",
                table: "replicates");
        }
    }
}
