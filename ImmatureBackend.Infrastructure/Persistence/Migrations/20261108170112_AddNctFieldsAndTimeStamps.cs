using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImmatureBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNctFieldsAndTimeStamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "captured_at",
                table: "replicates",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "index_number",
                table: "replicates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "sample_no",
                table: "replicates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "season",
                table: "replicates",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "selection_name",
                table: "replicates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "submitted_at",
                table: "replicates",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "ix_replicates_season_index_number_sample_no",
                table: "replicates",
                columns: new[] { "season", "index_number", "sample_no" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_replicates_season_index_number_sample_no",
                table: "replicates");

            migrationBuilder.DropColumn(
                name: "captured_at",
                table: "replicates");

            migrationBuilder.DropColumn(
                name: "index_number",
                table: "replicates");

            migrationBuilder.DropColumn(
                name: "sample_no",
                table: "replicates");

            migrationBuilder.DropColumn(
                name: "season",
                table: "replicates");

            migrationBuilder.DropColumn(
                name: "selection_name",
                table: "replicates");

            migrationBuilder.DropColumn(
                name: "submitted_at",
                table: "replicates");
        }
    }
}
