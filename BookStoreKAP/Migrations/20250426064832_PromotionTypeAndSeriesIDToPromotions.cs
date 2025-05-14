using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStoreKAP.Migrations
{
    /// <inheritdoc />
    public partial class PromotionTypeAndSeriesIDToPromotions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Promotions_Tags_TagID",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "ApplyToBestSeller",
                table: "Promotions");

            migrationBuilder.AlterColumn<Guid>(
                name: "TagID",
                table: "Promotions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "PromotionType",
                table: "Promotions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SeriesID",
                table: "Promotions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PromotionBooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromotionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionBooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionBooks_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromotionBooks_Promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "Promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_SeriesID",
                table: "Promotions",
                column: "SeriesID");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBooks_BookId",
                table: "PromotionBooks",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBooks_PromotionId",
                table: "PromotionBooks",
                column: "PromotionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Promotions_Series_SeriesID",
                table: "Promotions",
                column: "SeriesID",
                principalTable: "Series",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Promotions_Tags_TagID",
                table: "Promotions",
                column: "TagID",
                principalTable: "Tags",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Promotions_Series_SeriesID",
                table: "Promotions");

            migrationBuilder.DropForeignKey(
                name: "FK_Promotions_Tags_TagID",
                table: "Promotions");

            migrationBuilder.DropTable(
                name: "PromotionBooks");

            migrationBuilder.DropIndex(
                name: "IX_Promotions_SeriesID",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "PromotionType",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "SeriesID",
                table: "Promotions");

            migrationBuilder.AlterColumn<Guid>(
                name: "TagID",
                table: "Promotions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyToBestSeller",
                table: "Promotions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Promotions_Tags_TagID",
                table: "Promotions",
                column: "TagID",
                principalTable: "Tags",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
