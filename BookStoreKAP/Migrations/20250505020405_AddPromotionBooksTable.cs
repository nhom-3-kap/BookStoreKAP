using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStoreKAP.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionBooksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Series_SeriesID",
                table: "Books");

            migrationBuilder.AlterColumn<Guid>(
                name: "SeriesID",
                table: "Books",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Series_SeriesID",
                table: "Books",
                column: "SeriesID",
                principalTable: "Series",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Series_SeriesID",
                table: "Books");

            migrationBuilder.AlterColumn<Guid>(
                name: "SeriesID",
                table: "Books",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Series_SeriesID",
                table: "Books",
                column: "SeriesID",
                principalTable: "Series",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
