using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchedulingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dotnet_scheduling");

            migrationBuilder.CreateTable(
                name: "scheduling_profiles",
                schema: "dotnet_scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BusinessName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduling_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "schedules",
                schema: "dotnet_scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DaysOfWeek = table.Column<string>(type: "text", nullable: false),
                    StartTimeOfDay = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTimeOfDay = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    SlotDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    MinAdvanceBookingMinutes = table.Column<int>(type: "integer", nullable: false),
                    MaxAdvanceBookingDays = table.Column<int>(type: "integer", nullable: false),
                    CancellationDeadlineMinutes = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_schedules_scheduling_profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "dotnet_scheduling",
                        principalTable: "scheduling_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "availabilities",
                schema: "dotnet_scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HostProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SlotDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    MinAdvanceBookingMinutes = table.Column<int>(type: "integer", nullable: false),
                    MaxAdvanceBookingDays = table.Column<int>(type: "integer", nullable: false),
                    CancellationDeadlineMinutes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_availabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_availabilities_schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalSchema: "dotnet_scheduling",
                        principalTable: "schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_availabilities_scheduling_profiles_HostProfileId",
                        column: x => x.HostProfileId,
                        principalSchema: "dotnet_scheduling",
                        principalTable: "scheduling_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "time_slots",
                schema: "dotnet_scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AvailabilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_slots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_time_slots_availabilities_AvailabilityId",
                        column: x => x.AvailabilityId,
                        principalSchema: "dotnet_scheduling",
                        principalTable: "availabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "appointments",
                schema: "dotnet_scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeSlotId = table.Column<Guid>(type: "uuid", nullable: false),
                    HostProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuestProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CanceledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CanceledBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_appointments_scheduling_profiles_GuestProfileId",
                        column: x => x.GuestProfileId,
                        principalSchema: "dotnet_scheduling",
                        principalTable: "scheduling_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_scheduling_profiles_HostProfileId",
                        column: x => x.HostProfileId,
                        principalSchema: "dotnet_scheduling",
                        principalTable: "scheduling_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_time_slots_TimeSlotId",
                        column: x => x.TimeSlotId,
                        principalSchema: "dotnet_scheduling",
                        principalTable: "time_slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_GuestProfileId_Status",
                schema: "dotnet_scheduling",
                table: "appointments",
                columns: new[] { "GuestProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_HostProfileId_Status",
                schema: "dotnet_scheduling",
                table: "appointments",
                columns: new[] { "HostProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_TimeSlotId",
                schema: "dotnet_scheduling",
                table: "appointments",
                column: "TimeSlotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_availabilities_HostProfileId_StartTime_EndTime",
                schema: "dotnet_scheduling",
                table: "availabilities",
                columns: new[] { "HostProfileId", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_availabilities_ScheduleId",
                schema: "dotnet_scheduling",
                table: "availabilities",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_schedules_ProfileId_IsActive",
                schema: "dotnet_scheduling",
                table: "schedules",
                columns: new[] { "ProfileId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_schedules_ProfileId_Name",
                schema: "dotnet_scheduling",
                table: "schedules",
                columns: new[] { "ProfileId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_scheduling_profiles_ExternalUserId",
                schema: "dotnet_scheduling",
                table: "scheduling_profiles",
                column: "ExternalUserId");

            // Unique partial index: max 1 Individual profile per ExternalUserId
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ""IX_scheduling_profiles_ExternalUserId_Individual""
                ON dotnet_scheduling.scheduling_profiles (""ExternalUserId"")
                WHERE ""Type"" = 'Individual' AND ""IsDeleted"" = false;
            ");

            // Unique partial index: unique BusinessName per ExternalUserId for Business profiles
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ""IX_scheduling_profiles_ExternalUserId_BusinessName""
                ON dotnet_scheduling.scheduling_profiles (""ExternalUserId"", ""BusinessName"")
                WHERE ""Type"" = 'Business' AND ""IsDeleted"" = false;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_time_slots_AvailabilityId_Status_StartTime",
                schema: "dotnet_scheduling",
                table: "time_slots",
                columns: new[] { "AvailabilityId", "Status", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_time_slots_Status_StartTime",
                schema: "dotnet_scheduling",
                table: "time_slots",
                columns: new[] { "Status", "StartTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop custom indexes
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS dotnet_scheduling.""IX_scheduling_profiles_ExternalUserId_Individual"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS dotnet_scheduling.""IX_scheduling_profiles_ExternalUserId_BusinessName"";");

            migrationBuilder.DropTable(
                name: "appointments",
                schema: "dotnet_scheduling");

            migrationBuilder.DropTable(
                name: "time_slots",
                schema: "dotnet_scheduling");

            migrationBuilder.DropTable(
                name: "availabilities",
                schema: "dotnet_scheduling");

            migrationBuilder.DropTable(
                name: "schedules",
                schema: "dotnet_scheduling");

            migrationBuilder.DropTable(
                name: "scheduling_profiles",
                schema: "dotnet_scheduling");
        }
    }
}
