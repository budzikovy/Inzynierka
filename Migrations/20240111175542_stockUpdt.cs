using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inz_Fn.Migrations
{
    public partial class stockUpdt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "StocksHistory",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Stock",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "StocksHistory");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Stock");
        }
    }
}
