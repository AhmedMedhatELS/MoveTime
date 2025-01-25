using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTablesModefiyOthers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Children",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CheckInOut",
                columns: table => new
                {
                    CheckInOutId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    HaveDebt = table.Column<bool>(type: "bit", nullable: false),
                    IsEscort = table.Column<bool>(type: "bit", nullable: false),
                    CheckIn = table.Column<TimeSpan>(type: "time", nullable: false),
                    ExpectedCheckout = table.Column<TimeSpan>(type: "time", nullable: true),
                    ActualCheckout = table.Column<TimeSpan>(type: "time", nullable: true),
                    InPayment = table.Column<int>(type: "int", nullable: true),
                    OutPayment = table.Column<int>(type: "int", nullable: true),
                    InTotal = table.Column<int>(type: "int", nullable: false),
                    OutTotal = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckInOut", x => x.CheckInOutId);
                    table.ForeignKey(
                        name: "FK_CheckInOut_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId");
                });

            migrationBuilder.CreateTable(
                name: "CheckChildren",
                columns: table => new
                {
                    CheckChildId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChildId = table.Column<int>(type: "int", nullable: false),
                    CheckInOutId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckChildren", x => x.CheckChildId);
                    table.ForeignKey(
                        name: "FK_CheckChildren_CheckInOut_CheckInOutId",
                        column: x => x.CheckInOutId,
                        principalTable: "CheckInOut",
                        principalColumn: "CheckInOutId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CheckChildren_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "ChildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CheckProducts",
                columns: table => new
                {
                    CheckProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CheckInOutId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckProducts", x => x.CheckProductId);
                    table.ForeignKey(
                        name: "FK_CheckProducts_CheckInOut_CheckInOutId",
                        column: x => x.CheckInOutId,
                        principalTable: "CheckInOut",
                        principalColumn: "CheckInOutId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CheckProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckChildren_CheckInOutId",
                table: "CheckChildren",
                column: "CheckInOutId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckChildren_ChildId",
                table: "CheckChildren",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInOut_EventId",
                table: "CheckInOut",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckProducts_CheckInOutId",
                table: "CheckProducts",
                column: "CheckInOutId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckProducts_ProductId",
                table: "CheckProducts",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckChildren");

            migrationBuilder.DropTable(
                name: "CheckProducts");

            migrationBuilder.DropTable(
                name: "CheckInOut");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Children");
        }
    }
}
