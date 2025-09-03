using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OphimIngestApi.Migrations
{
    /// <inheritdoc />
    public partial class AddListMovie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Chieurap",
                table: "Movies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCopyright",
                table: "Movies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MovieListId",
                table: "Movies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SubDocquyen",
                table: "Movies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MovieLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieLists", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movies_MovieListId",
                table: "Movies",
                column: "MovieListId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieLists_Slug",
                table: "MovieLists",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_MovieLists_MovieListId",
                table: "Movies",
                column: "MovieListId",
                principalTable: "MovieLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movies_MovieLists_MovieListId",
                table: "Movies");

            migrationBuilder.DropTable(
                name: "MovieLists");

            migrationBuilder.DropIndex(
                name: "IX_Movies_MovieListId",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "Chieurap",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "IsCopyright",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "MovieListId",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "SubDocquyen",
                table: "Movies");
        }
    }
}
