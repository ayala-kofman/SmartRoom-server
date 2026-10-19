using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartRoom.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Grade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LessonPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LessonNumber = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Floor = table.Column<int>(type: "integer", nullable: false),
                    Quality = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RoomType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ComputerCount = table.Column<int>(type: "integer", nullable: true),
                    SeatCount = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IdentityNumber = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClassGroupId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_ClassGroups_ClassGroupId",
                        column: x => x.ClassGroupId,
                        principalTable: "ClassGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherClassGroups",
                columns: table => new
                {
                    TeacherId = table.Column<int>(type: "integer", nullable: false),
                    ClassGroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherClassGroups", x => new { x.TeacherId, x.ClassGroupId });
                    table.ForeignKey(
                        name: "FK_TeacherClassGroups_ClassGroups_ClassGroupId",
                        column: x => x.ClassGroupId,
                        principalTable: "ClassGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherClassGroups_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ClassGroups",
                columns: new[] { "Id", "Grade", "Name" },
                values: new object[,]
                {
                    { 1, 9, "ט'1" },
                    { 2, 9, "ט'2" },
                    { 3, 10, "י'1" },
                    { 4, 10, "י'2" },
                    { 5, 11, "י\"א1" },
                    { 6, 12, "י\"ב1" }
                });

            migrationBuilder.InsertData(
                table: "LessonPeriods",
                columns: new[] { "Id", "EndTime", "LessonNumber", "StartTime" },
                values: new object[,]
                {
                    { 1, new TimeOnly(8, 45, 0), 1, new TimeOnly(8, 0, 0) },
                    { 2, new TimeOnly(9, 30, 0), 2, new TimeOnly(8, 45, 0) },
                    { 3, new TimeOnly(10, 30, 0), 3, new TimeOnly(9, 45, 0) },
                    { 4, new TimeOnly(11, 15, 0), 4, new TimeOnly(10, 30, 0) },
                    { 5, new TimeOnly(12, 15, 0), 5, new TimeOnly(11, 30, 0) },
                    { 6, new TimeOnly(13, 0, 0), 6, new TimeOnly(12, 15, 0) },
                    { 7, new TimeOnly(14, 0, 0), 7, new TimeOnly(13, 15, 0) },
                    { 8, new TimeOnly(14, 45, 0), 8, new TimeOnly(14, 0, 0) },
                    { 9, new TimeOnly(15, 30, 0), 9, new TimeOnly(14, 45, 0) },
                    { 10, new TimeOnly(16, 30, 0), 10, new TimeOnly(15, 45, 0) },
                    { 11, new TimeOnly(17, 15, 0), 11, new TimeOnly(16, 30, 0) },
                    { 12, new TimeOnly(18, 0, 0), 12, new TimeOnly(17, 15, 0) }
                });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "Floor", "Name", "Quality", "RoomNumber", "RoomType", "SeatCount", "Status" },
                values: new object[,]
                {
                    { 1, 1, "כיתה 101", 1, "101", "Regular", 30, "Active" },
                    { 2, 1, "כיתה 102", 2, "102", "Regular", 35, "Active" },
                    { 3, 2, "כיתה 201", 3, "201", "Regular", 40, "Active" },
                    { 4, 2, "כיתה 202", 2, "202", "Regular", 32, "UnderMaintenance" },
                    { 5, 3, "אולם 301", 3, "301", "Regular", 60, "Active" }
                });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "ComputerCount", "Floor", "Name", "Quality", "RoomNumber", "RoomType", "Status" },
                values: new object[,]
                {
                    { 6, 25, 1, "מעבדת מחשבים א", 2, "103", "Computer", "Active" },
                    { 7, 30, 2, "מעבדת מחשבים ב", 3, "203", "Computer", "Active" },
                    { 8, 20, 3, "מעבדת מחשבים ג", 1, "302", "Computer", "Active" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassGroups_Name",
                table: "ClassGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonPeriods_LessonNumber",
                table: "LessonPeriods",
                column: "LessonNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_RoomNumber",
                table: "Rooms",
                column: "RoomNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherClassGroups_ClassGroupId",
                table: "TeacherClassGroups",
                column: "ClassGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClassGroupId",
                table: "Users",
                column: "ClassGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdentityNumber",
                table: "Users",
                column: "IdentityNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonPeriods");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "TeacherClassGroups");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ClassGroups");
        }
    }
}
