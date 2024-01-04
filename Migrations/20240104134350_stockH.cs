using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inz_Fn.Migrations
{
    public partial class stockH : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price_per_stock",
                table: "StocksHistory");

            migrationBuilder.AlterColumn<string>(
                name: "Stock_CIK",
                table: "StocksHistory",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<double>(
                name: "Income",
                table: "StocksHistory",
                type: "double",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<double>(
                name: "Price_per_stock_b",
                table: "StocksHistory",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Price_per_stock_s",
                table: "StocksHistory",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price_per_stock_b",
                table: "StocksHistory");

            migrationBuilder.DropColumn(
                name: "Price_per_stock_s",
                table: "StocksHistory");

            migrationBuilder.AlterColumn<int>(
                name: "Stock_CIK",
                table: "StocksHistory",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Income",
                table: "StocksHistory",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AddColumn<int>(
                name: "Price_per_stock",
                table: "StocksHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
