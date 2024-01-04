using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inz_Fn.Migrations
{
    public partial class cd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type_of_action",
                table: "StocksHistory",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type_of_action",
                table: "StocksHistory");
        }
    }
}
