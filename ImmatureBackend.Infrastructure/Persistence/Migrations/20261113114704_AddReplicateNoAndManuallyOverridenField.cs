using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImmatureBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReplicateNoAndManuallyOverridenField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_replicates_season_index_number_sample_no",
                table: "replicates");

            migrationBuilder.AddColumn<bool>(
                name: "manually_overridden",
                table: "replicates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "replicate_no",
                table: "replicates",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "ix_replicates_sample_replicate_no",
                table: "replicates",
                columns: new[] { "season", "index_number", "sample_no", "replicate_no" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_replicates_sample_replicate_no",
                table: "replicates");

            migrationBuilder.DropColumn(
                name: "manually_overridden",
                table: "replicates");

            migrationBuilder.DropColumn(
                name: "replicate_no",
                table: "replicates");

            migrationBuilder.CreateIndex(
                name: "ix_replicates_season_index_number_sample_no",
                table: "replicates",
                columns: new[] { "season", "index_number", "sample_no" },
                unique: true);
        }
    }
}
