using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStoreKAP.Migrations
{
    /// <inheritdoc />
    public partial class AddFKForTableBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BookID",
                table: "Tags",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TagID",
                table: "Books",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Tags_BookID",
                table: "Tags",
                column: "BookID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Books_BookID",
                table: "Tags",
                column: "BookID",
                principalTable: "Books",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Books_BookID",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_BookID",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "BookID",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "TagID",
                table: "Books");
        }
    }
}
