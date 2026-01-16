using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketTimeEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketTimeEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Minutes = table.Column<int>(type: "int", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTimeEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketTimeEntries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketTimeEntries_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketTimeEntries_TicketId",
                table: "TicketTimeEntries",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTimeEntries_UserId",
                table: "TicketTimeEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTimeEntries_WorkDate",
                table: "TicketTimeEntries",
                column: "WorkDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketTimeEntries");
        }
    }
}
