using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImmatureBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMeetsRecommendedValueField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "meets_recommended_value",
                table: "replicates",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "meets_recommended_value",
                table: "replicates");
        }
    }
}
