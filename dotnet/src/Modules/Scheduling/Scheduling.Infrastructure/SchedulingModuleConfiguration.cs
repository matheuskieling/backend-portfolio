using Common.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scheduling.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Application.Services;
using Scheduling.Application.UseCases.Appointments;
using Scheduling.Application.UseCases.Availabilities;
using Scheduling.Application.UseCases.Profiles;
using Scheduling.Application.UseCases.Schedules;
using Scheduling.Application.UseCases.TimeSlots;
using Scheduling.Infrastructure.Persistence;
using Scheduling.Infrastructure.Persistence.Repositories;
using Scheduling.Infrastructure.Services;

namespace Scheduling.Infrastructure;

public static class SchedulingModuleConfiguration
{
    public static IServiceCollection AddSchedulingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string is not configured");

        var connectionString = ConnectionStringHelper.ConvertToNpgsqlConnectionString(databaseUrl);

        services.AddDbContext<SchedulingDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    "dotnet_scheduling")));

        // Unit of Work
        services.AddScoped<ISchedulingUnitOfWork>(provider =>
            provider.GetRequiredService<SchedulingDbContext>());

        // Repositories
        services.AddScoped<ISchedulingProfileRepository, SchedulingProfileRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
        services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        // Services
        services.AddScoped<IAvailabilityGeneratorService, AvailabilityGeneratorService>();

        // Profile handlers
        services.AddScoped<CreateProfileHandler>();
        services.AddScoped<GetMyProfilesHandler>();
        services.AddScoped<GetProfileByIdHandler>();
        services.AddScoped<DeleteProfileHandler>();

        // Schedule handlers
        services.AddScoped<CreateScheduleHandler>();
        services.AddScoped<GetSchedulesHandler>();
        services.AddScoped<GetScheduleByIdHandler>();
        services.AddScoped<UpdateScheduleHandler>();
        services.AddScoped<DeleteScheduleHandler>();
        services.AddScoped<PauseScheduleHandler>();
        services.AddScoped<ResumeScheduleHandler>();
        services.AddScoped<GenerateAvailabilitiesHandler>();

        // Availability handlers
        services.AddScoped<CreateAvailabilityHandler>();
        services.AddScoped<GetAvailabilitiesHandler>();
        services.AddScoped<GetAvailabilityByIdHandler>();
        services.AddScoped<DeleteAvailabilityHandler>();

        // TimeSlot handlers
        services.AddScoped<GetAvailableSlotsHandler>();
        services.AddScoped<BlockSlotsHandler>();
        services.AddScoped<UnblockSlotsHandler>();

        // Appointment handlers
        services.AddScoped<BookAppointmentHandler>();
        services.AddScoped<GetAppointmentsHandler>();
        services.AddScoped<GetAppointmentByIdHandler>();
        services.AddScoped<CancelAppointmentHandler>();
        services.AddScoped<CompleteAppointmentHandler>();

        return services;
    }
}
