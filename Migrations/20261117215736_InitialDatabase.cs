using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NashaMuktiDB.Migrations
{
    /// <inheritdoc />
    public partial class InitialDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    StateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.StateId);
                });

            migrationBuilder.CreateTable(
                name: "Centres",
                columns: table => new
                {
                    CentreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CentreName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StateId = table.Column<int>(type: "int", nullable: false),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Centres", x => x.CentreId);
                    table.ForeignKey(
                        name: "FK_Centres_States",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "StateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Beneficiaries",
                columns: table => new
                {
                    BeneficiaryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BeneficiaryCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CentreId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beneficiaries", x => x.BeneficiaryId);
                    table.ForeignKey(
                        name: "FK_Beneficiaries_Centres",
                        column: x => x.CentreId,
                        principalTable: "Centres",
                        principalColumn: "CentreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "States",
                columns: new[] { "StateId", "IsActive", "StateName" },
                values: new object[,]
                {
                    { 1, true, "Punjab" },
                    { 2, true, "Bihar" },
                    { 3, true, "Maharashtra" },
                    { 4, true, "Rajasthan" },
                    { 5, true, "Delhi" },
                    { 6, true, "Madhya Pradesh" },
                    { 7, true, "Uttar Pradesh" },
                    { 8, true, "Haryana" }
                });

            migrationBuilder.InsertData(
                table: "Centres",
                columns: new[] { "CentreId", "Address", "CentreName", "ContactNumber", "CreatedAt", "District", "Email", "IsActive", "StateId" },
                values: new object[,]
                {
                    { 1, "Civil Lines, Near Red Cross Bhawan, Amritsar, Punjab - 143001", "Navchetan De-Addiction & Rehabilitation Centre", "0183-2550101", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Amritsar", "navchetan.amritsar@example.gov.in", true, 1 },
                    { 2, "Model Town, Phase II, Ludhiana, Punjab - 141002", "Asha Kiran Counselling and Recovery Centre", "0161-2440202", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Ludhiana", "ashakiran.ludhiana@example.gov.in", true, 1 },
                    { 3, "Bailey Road, Near High Court, Patna, Bihar - 800001", "Naya Savera Integrated Rehabilitation Facility", "0612-2230303", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Patna", "nayasavera.patna@example.gov.in", true, 2 },
                    { 4, "Shivajinagar, J.M. Road, Pune, Maharashtra - 411005", "Prerna De-Addiction and Care Kendra", "020-25510404", new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Pune", "prerna.pune@example.gov.in", true, 3 },
                    { 5, "Tonk Road, Gandhi Nagar, Jaipur, Rajasthan - 302015", "Sanjeevani Wellness and Rehabilitation Kendra", "0141-2700505", new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Jaipur", "sanjeevani.jaipur@example.gov.in", true, 4 }
                });

            migrationBuilder.InsertData(
                table: "Beneficiaries",
                columns: new[] { "BeneficiaryId", "BeneficiaryCode", "CentreId", "ContactNumber", "CreatedAt", "DateOfBirth", "FullName", "Gender", "RegistrationDate", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "BEN-PB-2026-0001", 1, "9876543210", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1992, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gurpreet Singh (Demo)", "Male", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", null },
                    { 2, "BEN-PB-2026-0002", 1, "9876543211", new DateTime(2026, 2, 3, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1988, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jaswinder Kaur (Demo)", "Female", new DateTime(2026, 2, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", null },
                    { 3, "BEN-PB-2026-0003", 2, "9876543212", new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1995, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Harmanpreet Sharma (Demo)", "Male", new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", null },
                    { 4, "BEN-BR-2026-0001", 3, "9876543213", new DateTime(2026, 2, 12, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1985, 3, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ramesh Kumar Verma (Demo)", "Male", new DateTime(2026, 2, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", null },
                    { 5, "BEN-MH-2026-0001", 4, "9876543214", new DateTime(2026, 2, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1990, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sachin Raghunath Patil (Demo)", "Male", new DateTime(2026, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", null },
                    { 6, "BEN-RJ-2026-0001", 5, "9876543215", new DateTime(2026, 2, 18, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1993, 12, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Vikram Singh Rathore (Demo)", "Male", new DateTime(2026, 2, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_CentreId",
                table: "Beneficiaries",
                column: "CentreId");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_FullName",
                table: "Beneficiaries",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_RegistrationDate",
                table: "Beneficiaries",
                column: "RegistrationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_Status",
                table: "Beneficiaries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "UQ_Beneficiaries_BeneficiaryCode",
                table: "Beneficiaries",
                column: "BeneficiaryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Centres_CentreName",
                table: "Centres",
                column: "CentreName");

            migrationBuilder.CreateIndex(
                name: "IX_Centres_District",
                table: "Centres",
                column: "District");

            migrationBuilder.CreateIndex(
                name: "IX_Centres_IsActive",
                table: "Centres",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Centres_StateId",
                table: "Centres",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_States_StateName",
                table: "States",
                column: "StateName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Beneficiaries");

            migrationBuilder.DropTable(
                name: "Centres");

            migrationBuilder.DropTable(
                name: "States");
        }
    }
}
