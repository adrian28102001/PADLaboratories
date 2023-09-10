using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JobManagementService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Salary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobOffers_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "Id", "City", "Country", "State" },
                values: new object[,]
                {
                    { 1, "New York", "USA", "NY" },
                    { 2, "London", "UK", "LDN" },
                    { 3, "Paris", "France", "IDF" }
                });

            migrationBuilder.InsertData(
                table: "JobOffers",
                columns: new[] { "Id", "ClosingDate", "Description", "LocationId", "PostedDate", "Salary", "Title", "Type" },
                values: new object[,]
                {
                    { 1, null, "Develop cutting-edge applications", 1, new DateTime(2023, 9, 10, 16, 1, 39, 548, DateTimeKind.Utc).AddTicks(6685), 60000m, "Software Developer", 0 },
                    { 2, null, "Manage and maintain IT infrastructure", 2, new DateTime(2023, 9, 10, 16, 1, 39, 548, DateTimeKind.Utc).AddTicks(6687), 50000m, "System Administrator", 0 },
                    { 3, null, "Manage and maintain database systems", 3, new DateTime(2023, 9, 10, 16, 1, 39, 548, DateTimeKind.Utc).AddTicks(6688), 55000m, "Database Administrator", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobOffers_LocationId",
                table: "JobOffers",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobOffers");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
