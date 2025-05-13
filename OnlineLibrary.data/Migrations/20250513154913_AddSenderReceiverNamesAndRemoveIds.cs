using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSenderReceiverNamesAndRemoveIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "exchangeBooksRequests",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverName",
                table: "exchangeBooksRequests",
                type: "nvarchar(100)",
                nullable: true);

            
            migrationBuilder.Sql(@"
        UPDATE e
        SET e.SenderName = (SELECT CONCAT(u.firstName, ' ', u.LastName) 
                           FROM AspNetUsers u 
                           WHERE u.Id = e.SenderUserId),
            e.ReceiverName = (SELECT CONCAT(u.firstName, ' ', u.LastName) 
                             FROM AspNetUsers u 
                             WHERE u.Id = e.ReceiverUserId)
        FROM exchangeBooksRequests e
        WHERE e.SenderUserId IS NOT NULL OR e.ReceiverUserId IS NOT NULL;
    ");

            
            migrationBuilder.DropForeignKey(
                name: "FK_exchangeBooksRequests_AspNetUsers_SenderId",
                table: "exchangeBooksRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_exchangeBooksRequests_AspNetUsers_ReceiverId",
                table: "exchangeBooksRequests");

            
            migrationBuilder.DropIndex(
                name: "IX_exchangeBooksRequests_SenderId",
                table: "exchangeBooksRequests");

            migrationBuilder.DropIndex(
                name: "IX_exchangeBooksRequests_ReceiverId",
                table: "exchangeBooksRequests");

           
            migrationBuilder.DropColumn(
                name: "SenderId",
                table: "exchangeBooksRequests");

            migrationBuilder.DropColumn(
                name: "ReceiverId",
                table: "exchangeBooksRequests");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.AddColumn<string>(
                name: "SenderId",
                table: "exchangeBooksRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverId",
                table: "exchangeBooksRequests",
                type: "nvarchar(max)",
                nullable: true);

            
            migrationBuilder.CreateIndex(
                name: "IX_exchangeBooksRequests_SenderId",
                table: "exchangeBooksRequests",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_exchangeBooksRequests_ReceiverId",
                table: "exchangeBooksRequests",
                column: "ReceiverId");

            migrationBuilder.AddForeignKey(
                name: "FK_exchangeBooksRequests_AspNetUsers_SenderId",
                table: "exchangeBooksRequests",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_exchangeBooksRequests_AspNetUsers_ReceiverId",
                table: "exchangeBooksRequests",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

           migrationBuilder.DropColumn(
                name: "SenderName",
                table: "exchangeBooksRequests");

            migrationBuilder.DropColumn(
                name: "ReceiverName",
                table: "exchangeBooksRequests");
        }
    }
}
