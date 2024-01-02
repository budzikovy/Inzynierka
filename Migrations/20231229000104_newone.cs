using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inz_Fn.Migrations
{
    public partial class newone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stock_Id",
                table: "Stock");

            migrationBuilder.RenameColumn(
                name: "Stock_Id",
                table: "StocksHistory",
                newName: "Stock_CIK");

            migrationBuilder.AddColumn<string>(
                name: "Stock_CIK",
                table: "Stock",
                type: "varchar(256)",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stock_CIK",
                table: "Stock");

            migrationBuilder.RenameColumn(
                name: "Stock_CIK",
                table: "StocksHistory",
                newName: "Stock_Id");

            migrationBuilder.AddColumn<int>(
                name: "Stock_Id",
                table: "Stock",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
