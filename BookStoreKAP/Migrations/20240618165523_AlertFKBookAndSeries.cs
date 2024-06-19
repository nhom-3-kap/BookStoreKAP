using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStoreKAP.Migrations
{
    /// <inheritdoc />
    public partial class AlertFKBookAndSeries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Series_Books_BookID",
                table: "Series");

            migrationBuilder.DropIndex(
                name: "IX_Series_BookID",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "BookID",
                table: "Series");

            migrationBuilder.CreateIndex(
                name: "IX_Books_SeriesID",
                table: "Books",
                column: "SeriesID");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Series_SeriesID",
                table: "Books",
                column: "SeriesID",
                principalTable: "Series",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Series_SeriesID",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_SeriesID",
                table: "Books");

            migrationBuilder.AddColumn<Guid>(
                name: "BookID",
                table: "Series",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Series_BookID",
                table: "Series",
                column: "BookID");

            migrationBuilder.AddForeignKey(
                name: "FK_Series_Books_BookID",
                table: "Series",
                column: "BookID",
                principalTable: "Books",
                principalColumn: "ID");
        }
    }
}
