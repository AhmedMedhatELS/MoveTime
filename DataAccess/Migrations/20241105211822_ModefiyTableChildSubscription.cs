using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ModefiyTableChildSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChildSubscriptions_Subscriptions_SubscriptionId",
                table: "ChildSubscriptions");

            migrationBuilder.RenameColumn(
                name: "SubscriptionId",
                table: "ChildSubscriptions",
                newName: "SubscriptionPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_ChildSubscriptions_SubscriptionId",
                table: "ChildSubscriptions",
                newName: "IX_ChildSubscriptions_SubscriptionPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChildSubscriptions_SubscriptionPlans_SubscriptionPlanId",
                table: "ChildSubscriptions",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "SubscriptionPlanId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChildSubscriptions_SubscriptionPlans_SubscriptionPlanId",
                table: "ChildSubscriptions");

            migrationBuilder.RenameColumn(
                name: "SubscriptionPlanId",
                table: "ChildSubscriptions",
                newName: "SubscriptionId");

            migrationBuilder.RenameIndex(
                name: "IX_ChildSubscriptions_SubscriptionPlanId",
                table: "ChildSubscriptions",
                newName: "IX_ChildSubscriptions_SubscriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChildSubscriptions_Subscriptions_SubscriptionId",
                table: "ChildSubscriptions",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "SubscriptionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
