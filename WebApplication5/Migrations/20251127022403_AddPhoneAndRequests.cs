using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication5.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneAndRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequesterName",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequesterPhone",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumberPublic",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ServiceId",
                table: "ServiceRequests",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Services_ServiceId",
                table: "ServiceRequests",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Services_ServiceId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_ServiceId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "RequesterName",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "RequesterPhone",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "PhoneNumberPublic",
                table: "AspNetUsers");
        }
    }
}
