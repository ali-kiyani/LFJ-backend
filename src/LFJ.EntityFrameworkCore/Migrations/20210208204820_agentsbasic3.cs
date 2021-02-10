using Microsoft.EntityFrameworkCore.Migrations;

namespace LFJ.Migrations
{
    public partial class agentsbasic3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Agents",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Agents");
        }
    }
}
