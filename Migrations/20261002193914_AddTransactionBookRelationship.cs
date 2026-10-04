using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionBookRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Transactions_TransactionId",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_TransactionId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Books");

            migrationBuilder.CreateTable(
                name: "BookTransaction",
                columns: table => new
                {
                    BooksBookId = table.Column<int>(type: "int", nullable: false),
                    TransactionsTransactionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookTransaction", x => new { x.BooksBookId, x.TransactionsTransactionId });
                    table.ForeignKey(
                        name: "FK_BookTransaction_Books_BooksBookId",
                        column: x => x.BooksBookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookTransaction_Transactions_TransactionsTransactionId",
                        column: x => x.TransactionsTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookTransaction_TransactionsTransactionId",
                table: "BookTransaction",
                column: "TransactionsTransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookTransaction");

            migrationBuilder.AddColumn<int>(
                name: "TransactionId",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_TransactionId",
                table: "Books",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Transactions_TransactionId",
                table: "Books",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "TransactionId");
        }
    }
}
