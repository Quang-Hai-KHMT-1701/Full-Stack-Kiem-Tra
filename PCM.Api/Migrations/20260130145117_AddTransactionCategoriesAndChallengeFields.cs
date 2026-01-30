using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCM.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionCategoriesAndChallengeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Config_TargetWins",
                table: "123_Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "123_Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "123_Challenges",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CurrentScore_TeamA",
                table: "123_Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentScore_TeamB",
                table: "123_Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "123_Challenges",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GameMode",
                table: "123_Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "123_Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "123_TransactionCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_123_TransactionCategories", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "123_TransactionCategories");

            migrationBuilder.DropColumn(
                name: "Config_TargetWins",
                table: "123_Challenges");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "123_Challenges");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "123_Challenges");

            migrationBuilder.DropColumn(
                name: "CurrentScore_TeamA",
                table: "123_Challenges");

            migrationBuilder.DropColumn(
                name: "CurrentScore_TeamB",
                table: "123_Challenges");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "123_Challenges");

            migrationBuilder.DropColumn(
                name: "GameMode",
                table: "123_Challenges");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "123_Challenges");
        }
    }
}
