using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddInternalSystems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InternalSystemId",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InternalSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalSystems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_InternalSystemId",
                table: "Tickets",
                column: "InternalSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalSystems_Name",
                table: "InternalSystems",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_InternalSystems_InternalSystemId",
                table: "Tickets",
                column: "InternalSystemId",
                principalTable: "InternalSystems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_InternalSystems_InternalSystemId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "InternalSystems");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_InternalSystemId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "InternalSystemId",
                table: "Tickets");
        }
    }
}
