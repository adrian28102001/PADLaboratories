﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApplicationManagementService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Candidates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CandidateId = table.Column<int>(type: "integer", nullable: false),
                    JobOfferId = table.Column<int>(type: "integer", nullable: false),
                    CVPath = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AppliedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    InterviewDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Feedback = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Candidates",
                columns: new[] { "Id", "Email", "FullName", "PhoneNumber" },
                values: new object[,]
                {
                    { 1, "john.doe@example.com", "John Doe", "123-456-7890" },
                    { 2, "jane.smith@example.com", "Jane Smith", "234-567-8901" },
                    { 3, "robert.brown@example.com", "Robert Brown", "345-678-9012" }
                });

            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "Id", "AppliedDate", "CVPath", "CandidateId", "Feedback", "InterviewDate", "JobOfferId", "LastUpdated", "Status" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 10, 7, 21, 51, 18, 211, DateTimeKind.Local).AddTicks(4523), "/path/to/cv1.pdf", 1, "Great resume! Looking forward to the interview.", null, 1, new DateTime(2023, 10, 8, 21, 51, 18, 211, DateTimeKind.Local).AddTicks(4567), 0 },
                    { 2, new DateTime(2023, 10, 12, 21, 51, 18, 211, DateTimeKind.Local).AddTicks(4691), "/path/to/cv2.pdf", 2, "Impressive background!", null, 2, new DateTime(2023, 10, 13, 21, 51, 18, 211, DateTimeKind.Local).AddTicks(4695), 1 },
                    { 3, new DateTime(2023, 10, 15, 21, 51, 18, 211, DateTimeKind.Local).AddTicks(4698), "/path/to/cv3.pdf", 3, "Schedule for an interview next week.", new DateTime(2023, 10, 19, 21, 51, 18, 211, DateTimeKind.Local).AddTicks(4700), 3, new DateTime(2023, 10, 16, 21, 51, 18, 211, DateTimeKind.Local).AddTicks(4703), 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CandidateId",
                table: "Applications",
                column: "CandidateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Candidates");
        }
    }
}
