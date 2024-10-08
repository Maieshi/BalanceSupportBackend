using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Balance_Support.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RowCount",
                table: "UserSettings",
                newName: "RowsCount");

            migrationBuilder.AddColumn<string>(
                name: "About",
                table: "UserSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "UserSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "AnswersOnForm",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BlogDigest",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CommentsOnArticle",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "UserSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "NewsAnnouncements",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "UserSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "OnFollower",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "UserSettings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "100");

            migrationBuilder.AddColumn<bool>(
                name: "ProductUpdates",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "UserSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "About",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "AnswersOnForm",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "BlogDigest",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "CommentsOnArticle",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "NewsAnnouncements",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "OnFollower",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ProductUpdates",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "UserSettings");

            migrationBuilder.RenameColumn(
                name: "RowsCount",
                table: "UserSettings",
                newName: "RowCount");
        }
    }
}
