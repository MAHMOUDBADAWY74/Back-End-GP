using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPostCountAndCommentCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PostCount",
                table: "CommunityPosts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PostCount",
                table: "Communities",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostCount",
                table: "CommunityPosts");

            migrationBuilder.DropColumn(
                name: "PostCount",
                table: "Communities");
        }
    }
}
