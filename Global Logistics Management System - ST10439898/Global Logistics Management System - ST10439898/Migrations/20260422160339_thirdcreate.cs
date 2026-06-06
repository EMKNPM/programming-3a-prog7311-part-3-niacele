using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Global_Logistics_Management_System___ST10439898.Migrations
{
    /// <inheritdoc />
    public partial class thirdcreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Contracts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "companyName",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "companyName",
                table: "Clients");
        }
    }
}
