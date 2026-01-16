using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentTempUploads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TicketId",
                table: "TicketAttachments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "TempKey",
                table: "TicketAttachments",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachments_TempKey",
                table: "TicketAttachments",
                column: "TempKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TicketAttachments_TempKey",
                table: "TicketAttachments");

            migrationBuilder.DropColumn(
                name: "TempKey",
                table: "TicketAttachments");

            migrationBuilder.AlterColumn<int>(
                name: "TicketId",
                table: "TicketAttachments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
