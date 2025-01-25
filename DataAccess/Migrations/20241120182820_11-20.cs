using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class _1120 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChildSubscriptionId",
                table: "CheckInOut",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckInOut_ChildSubscriptionId",
                table: "CheckInOut",
                column: "ChildSubscriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckInOut_ChildSubscriptions_ChildSubscriptionId",
                table: "CheckInOut",
                column: "ChildSubscriptionId",
                principalTable: "ChildSubscriptions",
                principalColumn: "ChildSubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckInOut_ChildSubscriptions_ChildSubscriptionId",
                table: "CheckInOut");

            migrationBuilder.DropIndex(
                name: "IX_CheckInOut_ChildSubscriptionId",
                table: "CheckInOut");

            migrationBuilder.DropColumn(
                name: "ChildSubscriptionId",
                table: "CheckInOut");
        }
    }
}
