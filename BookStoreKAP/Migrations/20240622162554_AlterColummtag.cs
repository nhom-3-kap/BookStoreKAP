using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStoreKAP.Migrations
{
    /// <inheritdoc />
    public partial class AlterColummtag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "BookTag",
                columns: table => new
                {
                    BooksID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagsID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookTag", x => new { x.BooksID, x.TagsID });
                    table.ForeignKey(
                        name: "FK_BookTag_Books_BooksID",
                        column: x => x.BooksID,
                        principalTable: "Books",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookTag_Tags_TagsID",
                        column: x => x.TagsID,
                        principalTable: "Tags",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookTag_TagsID",
                table: "BookTag",
                column: "TagsID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookTag");

            migrationBuilder.AddColumn<Guid>(
                name: "BookID",
                table: "Tags",
                type: "uniqueidentifier",
                nullable: true);

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
    }
}
