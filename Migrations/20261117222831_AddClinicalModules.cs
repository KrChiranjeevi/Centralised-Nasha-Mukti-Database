using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NashaMuktiDB.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CounsellingSessions",
                columns: table => new
                {
                    SessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BeneficiaryId = table.Column<int>(type: "int", nullable: false),
                    SessionDate = table.Column<DateTime>(type: "date", nullable: false),
                    SessionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CounsellorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SessionStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Completed"),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounsellingSessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_CounsellingSessions_Beneficiaries",
                        column: x => x.BeneficiaryId,
                        principalTable: "Beneficiaries",
                        principalColumn: "BeneficiaryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FollowUps",
                columns: table => new
                {
                    FollowUpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BeneficiaryId = table.Column<int>(type: "int", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    CompletedDate = table.Column<DateTime>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUps", x => x.FollowUpId);
                    table.ForeignKey(
                        name: "FK_FollowUps_Beneficiaries",
                        column: x => x.BeneficiaryId,
                        principalTable: "Beneficiaries",
                        principalColumn: "BeneficiaryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Interventions",
                columns: table => new
                {
                    InterventionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BeneficiaryId = table.Column<int>(type: "int", nullable: false),
                    InterventionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ServiceDate = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "In Progress"),
                    ResponsibleStaffName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interventions", x => x.InterventionId);
                    table.ForeignKey(
                        name: "FK_Interventions_Beneficiaries",
                        column: x => x.BeneficiaryId,
                        principalTable: "Beneficiaries",
                        principalColumn: "BeneficiaryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CounsellingSessions",
                columns: new[] { "SessionId", "BeneficiaryId", "CounsellorName", "CreatedAt", "Notes", "SessionDate", "SessionStatus", "SessionType", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, "Dr. Harvinder Singh (Psychologist)", new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Initial motivational interviewing completed. Beneficiary expressed readiness for rehabilitation.", new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", "Individual", null },
                    { 2, 1, "Dr. Harvinder Singh (Psychologist)", new DateTime(2026, 2, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Family session conducted with spouse. Established positive household recovery support framework.", new DateTime(2026, 2, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", "Family", null },
                    { 3, 2, "Dr. Manpreet Kaur (Counsellor)", new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Cognitive behavioral therapy intro. Identified primary emotional triggers and coping strategies.", new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", "Individual", null },
                    { 4, 4, "Sanjay Jha (Social Worker)", new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Participated in peer support group. Shared challenges and received positive group feedback.", new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", "Group", null }
                });

            migrationBuilder.InsertData(
                table: "FollowUps",
                columns: new[] { "FollowUpId", "BeneficiaryId", "CompletedDate", "CreatedAt", "Notes", "ScheduledDate", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, null, new DateTime(2026, 2, 12, 0, 0, 0, 0, DateTimeKind.Utc), "1-month post-intake recovery evaluation scheduled.", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pending", null },
                    { 2, 2, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Routine bi-weekly check-in completed. Beneficiary reports good adherence to program.", new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", null },
                    { 3, 4, null, new DateTime(2026, 2, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Beneficiary was unable to attend due to travel. Re-contact initiated by field team.", new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Missed", null }
                });

            migrationBuilder.InsertData(
                table: "Interventions",
                columns: new[] { "InterventionId", "BeneficiaryId", "CreatedAt", "InterventionType", "Notes", "ResponsibleStaffName", "ServiceDate", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 2, 2, 0, 0, 0, 0, DateTimeKind.Utc), "Medical Detoxification", "5-day inpatient clinical detox successfully administered without acute complications.", "Dr. Rajesh Sharma (Medical Officer)", new DateTime(2026, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", null },
                    { 2, 1, new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Psychosocial Rehabilitation", "Enrolled in 60-day residential psychosocial recovery module.", "Navchetan Rehabilitation Team", new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "In Progress", null },
                    { 3, 4, new DateTime(2026, 2, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Medical Detoxification", "Supervised pharmacotherapy detox provided at Patna facility.", "Dr. P.K. Sinha (Physician)", new DateTime(2026, 2, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CounsellingSessions_BeneficiaryId",
                table: "CounsellingSessions",
                column: "BeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_CounsellingSessions_SessionDate",
                table: "CounsellingSessions",
                column: "SessionDate");

            migrationBuilder.CreateIndex(
                name: "IX_CounsellingSessions_SessionStatus",
                table: "CounsellingSessions",
                column: "SessionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_CounsellingSessions_SessionType",
                table: "CounsellingSessions",
                column: "SessionType");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUps_BeneficiaryId",
                table: "FollowUps",
                column: "BeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUps_ScheduledDate",
                table: "FollowUps",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUps_Status",
                table: "FollowUps",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_BeneficiaryId",
                table: "Interventions",
                column: "BeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_InterventionType",
                table: "Interventions",
                column: "InterventionType");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_ServiceDate",
                table: "Interventions",
                column: "ServiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_Status",
                table: "Interventions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CounsellingSessions");

            migrationBuilder.DropTable(
                name: "FollowUps");

            migrationBuilder.DropTable(
                name: "Interventions");
        }
    }
}
