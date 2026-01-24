using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCM.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxParticipantsToChallenge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_123_Challenges_123_Members_CreatedById",
                table: "123_Challenges");

            migrationBuilder.DropIndex(
                name: "IX_123_Challenges_CreatedById",
                table: "123_Challenges");

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
                name: "ModifiedDate",
                table: "123_Challenges");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "123_Challenges",
                newName: "MaxParticipants");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "123_Challenges",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaxParticipants",
                table: "123_Challenges",
                newName: "Type");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "123_Challenges",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "Config_TargetWins",
                table: "123_Challenges",
                type: "int",
                nullable: true);

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

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "123_Challenges",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_123_Challenges_CreatedById",
                table: "123_Challenges",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_123_Challenges_123_Members_CreatedById",
                table: "123_Challenges",
                column: "CreatedById",
                principalTable: "123_Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
