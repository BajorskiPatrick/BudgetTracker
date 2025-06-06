using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetTracker.Migrations
{
    /// <inheritdoc />
    public partial class SecondDatabaseMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expense_PaymentMethod_PaymentMethodId",
                table: "Expense");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Category");

            migrationBuilder.RenameColumn(
                name: "BudgetID",
                table: "Amount",
                newName: "BudgetId");

            migrationBuilder.AlterColumn<string>(
                name: "ApiToken",
                table: "User",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<long>(
                name: "PaymentMethodId",
                table: "Expense",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "UserId",
                keyValue: 1L,
                columns: new[] { "ApiToken", "RegistrationDate" },
                values: new object[] { "79b7d2871a464dc0a9a9328c5248f7e8", new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Local) });

            migrationBuilder.AddForeignKey(
                name: "FK_Expense_PaymentMethod_PaymentMethodId",
                table: "Expense",
                column: "PaymentMethodId",
                principalTable: "PaymentMethod",
                principalColumn: "PaymentMethodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expense_PaymentMethod_PaymentMethodId",
                table: "Expense");

            migrationBuilder.RenameColumn(
                name: "BudgetId",
                table: "Amount",
                newName: "BudgetID");

            migrationBuilder.AlterColumn<string>(
                name: "ApiToken",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "PaymentMethodId",
                table: "Expense",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Category",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "UserId",
                keyValue: 1L,
                columns: new[] { "ApiToken", "RegistrationDate" },
                values: new object[] { "", new DateTime(2025, 5, 29, 0, 0, 0, 0, DateTimeKind.Local) });

            migrationBuilder.AddForeignKey(
                name: "FK_Expense_PaymentMethod_PaymentMethodId",
                table: "Expense",
                column: "PaymentMethodId",
                principalTable: "PaymentMethod",
                principalColumn: "PaymentMethodId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
