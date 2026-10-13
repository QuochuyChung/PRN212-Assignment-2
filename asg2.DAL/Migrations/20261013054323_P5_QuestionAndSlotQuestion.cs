using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asg2.DAL.Migrations
{
    /// <inheritdoc />
    public partial class P5_QuestionAndSlotQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    BloomLevel = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SlotQuestions",
                columns: table => new
                {
                    SlotId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    OrderNo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlotQuestions", x => new { x.SlotId, x.QuestionId });
                    table.ForeignKey(
                        name: "FK_SlotQuestions_ExamSlots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "ExamSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SlotQuestions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SubjectId",
                table: "Questions",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SubjectId_BloomLevel",
                table: "Questions",
                columns: new[] { "SubjectId", "BloomLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_SlotQuestions_QuestionId",
                table: "SlotQuestions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_SlotQuestions_SlotId_OrderNo",
                table: "SlotQuestions",
                columns: new[] { "SlotId", "OrderNo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SlotQuestions");

            migrationBuilder.DropTable(
                name: "Questions");
        }
    }
}
