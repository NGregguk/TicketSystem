using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketSubscribers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    ActorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketEvents_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketEvents_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketSubscribers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketSubscribers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketSubscribers_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketSubscribers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketSubscribers_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketEvents_ActorUserId",
                table: "TicketEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketEvents_TicketId",
                table: "TicketEvents",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSubscribers_AddedByUserId",
                table: "TicketSubscribers",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSubscribers_TicketId_UserId",
                table: "TicketSubscribers",
                columns: new[] { "TicketId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketSubscribers_UserId",
                table: "TicketSubscribers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketEvents");

            migrationBuilder.DropTable(
                name: "TicketSubscribers");
        }
    }
}
