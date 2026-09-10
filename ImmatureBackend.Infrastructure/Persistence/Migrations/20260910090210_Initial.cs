using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImmatureBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReplicateEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TechnicianName = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    SampleId = table.Column<string>(type: "text", nullable: false),
                    AiPredictedGrains = table.Column<string>(type: "text", nullable: false),
                    ConfirmedGrains = table.Column<string>(type: "text", nullable: false),
                    ImmatureWeight = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Grade = table.Column<string>(type: "text", nullable: false),
                    OriginalImage = table.Column<byte[]>(type: "bytea", nullable: true),
                    ReviewStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplicateEntities", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReplicateEntities");
        }
    }
}
