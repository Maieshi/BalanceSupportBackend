using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Balance_Support.Migrations
{
    /// <inheritdoc />
    public partial class AddSelectedGroupsJsonColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedGroup",
                table: "UserSettings");

            migrationBuilder.AddColumn<string>(
                name: "SelectedGroups",
                table: "UserSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedGroups",
                table: "UserSettings");

            migrationBuilder.AddColumn<int>(
                name: "SelectedGroup",
                table: "UserSettings",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }
    }
}
