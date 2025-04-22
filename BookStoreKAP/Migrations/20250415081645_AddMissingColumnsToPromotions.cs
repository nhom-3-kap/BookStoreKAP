using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStoreKAP.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumnsToPromotions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DiscountPercentage",
                table: "Promotions",
                newName: "DiscountPercent");

            migrationBuilder.AddColumn<Guid>(
                name: "TagID",
                table: "Promotions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_TagID",
                table: "Promotions",
                column: "TagID");

            migrationBuilder.AddForeignKey(
                name: "FK_Promotions_Tags_TagID",
                table: "Promotions",
                column: "TagID",
                principalTable: "Tags",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Promotions_Tags_TagID",
                table: "Promotions");

            migrationBuilder.DropIndex(
                name: "IX_Promotions_TagID",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "TagID",
                table: "Promotions");

            migrationBuilder.RenameColumn(
                name: "DiscountPercent",
                table: "Promotions",
                newName: "DiscountPercentage");
        }
    }
}
