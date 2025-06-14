using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityBan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunityModerator_AspNetUsers_ApplicationUserId",
                table: "CommunityModerator");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunityModerator_Communities_CommunityId",
                table: "CommunityModerator");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommunityModerator",
                table: "CommunityModerator");

            migrationBuilder.RenameTable(
                name: "CommunityModerator",
                newName: "CommunityModerators");

            migrationBuilder.RenameIndex(
                name: "IX_CommunityModerator_CommunityId",
                table: "CommunityModerators",
                newName: "IX_CommunityModerators_CommunityId");

            migrationBuilder.RenameIndex(
                name: "IX_CommunityModerator_ApplicationUserId",
                table: "CommunityModerators",
                newName: "IX_CommunityModerators_ApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommunityModerators",
                table: "CommunityModerators",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CommunityBan",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BannedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityBan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityBan_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunityBan_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityBan_CommunityId",
                table: "CommunityBan",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityBan_UserId",
                table: "CommunityBan",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityModerators_AspNetUsers_ApplicationUserId",
                table: "CommunityModerators",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityModerators_Communities_CommunityId",
                table: "CommunityModerators",
                column: "CommunityId",
                principalTable: "Communities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunityModerators_AspNetUsers_ApplicationUserId",
                table: "CommunityModerators");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunityModerators_Communities_CommunityId",
                table: "CommunityModerators");

            migrationBuilder.DropTable(
                name: "CommunityBan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommunityModerators",
                table: "CommunityModerators");

            migrationBuilder.RenameTable(
                name: "CommunityModerators",
                newName: "CommunityModerator");

            migrationBuilder.RenameIndex(
                name: "IX_CommunityModerators_CommunityId",
                table: "CommunityModerator",
                newName: "IX_CommunityModerator_CommunityId");

            migrationBuilder.RenameIndex(
                name: "IX_CommunityModerators_ApplicationUserId",
                table: "CommunityModerator",
                newName: "IX_CommunityModerator_ApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommunityModerator",
                table: "CommunityModerator",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityModerator_AspNetUsers_ApplicationUserId",
                table: "CommunityModerator",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityModerator_Communities_CommunityId",
                table: "CommunityModerator",
                column: "CommunityId",
                principalTable: "Communities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
