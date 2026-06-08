using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GLMS.API.Migrations
{
    /// <inheritdoc />
    public partial class EditSeedDefaultUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "Password", "Username" },
                values: new object[] { "Admin123!", "dmin@techmove.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "Password", "Username" },
                values: new object[] { "Password123", "admin" });
        }
    }
}
