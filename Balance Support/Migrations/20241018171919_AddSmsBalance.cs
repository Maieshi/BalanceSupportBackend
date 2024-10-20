using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Balance_Support.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SmsBalance",
                table: "Accounts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmsBalance",
                table: "Accounts");
        }
    }
}
