using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetTracker.Migrations
{
    /// <inheritdoc />
    public partial class ThirdDatabaseMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "UserId",
                keyValue: 1L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "UserId", "ApiToken", "Email", "IsAdmin", "Name", "PasswordHash", "RegistrationDate", "Surname", "Username" },
                values: new object[] { 1L, "79b7d2871a464dc0a9a9328c5248f7e8", "pbajorski@student.agh.edu.pl", true, "Patrick", "21232F297A57A5A743894A0E4A801FC3", new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Local), "Bajorski", "admin" });
        }
    }
}
