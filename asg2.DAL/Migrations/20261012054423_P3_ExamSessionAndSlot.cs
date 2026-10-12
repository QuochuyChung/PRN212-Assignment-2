using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asg2.DAL.Migrations
{
    /// <inheritdoc />
    public partial class P3_ExamSessionAndSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SlotMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 15),
                    MainQuestionCount = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    MaxFollowUpCount = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSessions_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSessions_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    OrderNo = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSlots_Accounts_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSlots_ExamSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ExamSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessions_RoomId",
                table: "ExamSessions",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessions_RoomId_StartTime_EndTime",
                table: "ExamSessions",
                columns: new[] { "RoomId", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessions_SubjectId",
                table: "ExamSessions",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSlots_SessionId_OrderNo",
                table: "ExamSlots",
                columns: new[] { "SessionId", "OrderNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ExamSlots_StudentId_StartTime_EndTime",
                table: "ExamSlots",
                columns: new[] { "StudentId", "StartTime", "EndTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamSlots");

            migrationBuilder.DropTable(
                name: "ExamSessions");
        }
    }
}
