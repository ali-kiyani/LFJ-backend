using Microsoft.EntityFrameworkCore.Migrations;

namespace LFJ.Migrations
{
    public partial class agentsbasic2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Agents");

            migrationBuilder.AddColumn<int>(
                name: "PMCode",
                table: "Agents",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PMCode",
                table: "Agents");

            migrationBuilder.AddColumn<int>(
                name: "Code",
                table: "Agents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
