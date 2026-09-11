using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImmatureBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameReplicateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ReplicateEntities",
                table: "ReplicateEntities");

            migrationBuilder.RenameTable(
                name: "ReplicateEntities",
                newName: "replicates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_replicates",
                table: "replicates",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_replicates",
                table: "replicates");

            migrationBuilder.RenameTable(
                name: "replicates",
                newName: "ReplicateEntities");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReplicateEntities",
                table: "ReplicateEntities",
                column: "Id");
        }
    }
}
