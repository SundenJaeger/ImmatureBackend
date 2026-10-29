using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImmatureBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceOriginalImageWithImagePublicIdAndImageSecureUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "original_image",
                table: "replicates");

            migrationBuilder.AddColumn<string>(
                name: "image_public_id",
                table: "replicates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "image_secure_url",
                table: "replicates",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_public_id",
                table: "replicates");

            migrationBuilder.DropColumn(
                name: "image_secure_url",
                table: "replicates");

            migrationBuilder.AddColumn<byte[]>(
                name: "original_image",
                table: "replicates",
                type: "bytea",
                nullable: true);
        }
    }
}
