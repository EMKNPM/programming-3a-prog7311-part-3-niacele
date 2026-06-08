using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GLMS.API.Migrations
{
    /// <inheritdoc />
    public partial class EditSeedDefaultUsersAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "Username",
                value: "admin@techmove.com");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "Username",
                value: "dmin@techmove.com");
        }
    }
}
