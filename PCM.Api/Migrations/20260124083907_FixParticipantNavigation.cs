using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCM.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixParticipantNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParticipantId",
                table: "123_Participants",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_123_Participants_ParticipantId",
                table: "123_Participants",
                column: "ParticipantId");

            migrationBuilder.AddForeignKey(
                name: "FK_123_Participants_123_Participants_ParticipantId",
                table: "123_Participants",
                column: "ParticipantId",
                principalTable: "123_Participants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_123_Participants_123_Participants_ParticipantId",
                table: "123_Participants");

            migrationBuilder.DropIndex(
                name: "IX_123_Participants_ParticipantId",
                table: "123_Participants");

            migrationBuilder.DropColumn(
                name: "ParticipantId",
                table: "123_Participants");
        }
    }
}
