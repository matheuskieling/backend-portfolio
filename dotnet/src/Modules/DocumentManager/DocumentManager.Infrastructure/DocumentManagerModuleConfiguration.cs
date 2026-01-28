using Common.Contracts;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Services;
using DocumentManager.Application.Common.Interfaces;
using DocumentManager.Application.Repositories;
using DocumentManager.Application.UseCases.Approvals;
using DocumentManager.Application.UseCases.Documents;
using DocumentManager.Application.UseCases.Folders;
using DocumentManager.Application.UseCases.Tags;
using DocumentManager.Application.UseCases.Versions;
using DocumentManager.Application.UseCases.Workflows;
using DocumentManager.Infrastructure.Persistence;
using DocumentManager.Infrastructure.Persistence.Repositories;
using DocumentManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentManager.Infrastructure;

public static class DocumentManagerModuleConfiguration
{
    public static IServiceCollection AddDocumentManagerModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string is not configured");

        var connectionString = ConnectionStringHelper.ConvertToNpgsqlConnectionString(databaseUrl);

        services.AddDbContext<DocumentManagerDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    "dotnet_document_manager")));

        // Unit of Work
        services.AddScoped<IDocumentManagerUnitOfWork>(provider =>
            provider.GetRequiredService<DocumentManagerDbContext>());

        // Repositories
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IApprovalWorkflowRepository, ApprovalWorkflowRepository>();
        services.AddScoped<IApprovalRequestRepository, ApprovalRequestRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Services
        var fileStoragePath = configuration["DocumentManager:FileStoragePath"]
            ?? Path.Combine(AppContext.BaseDirectory, "file-storage");
        services.AddSingleton<IFileStorageService>(new FileStorageService(fileStoragePath));
        services.AddScoped<IDocumentAuthorizationService, DocumentAuthorizationService>();

        // Folder use case handlers
        services.AddScoped<CreateFolderHandler>();
        services.AddScoped<GetFolderTreeHandler>();

        // Document use case handlers
        services.AddScoped<CreateDocumentHandler>();
        services.AddScoped<GetDocumentsHandler>();
        services.AddScoped<GetDocumentByIdHandler>();
        services.AddScoped<UpdateDocumentHandler>();
        services.AddScoped<DeleteDocumentHandler>();
        services.AddScoped<GetDocumentHistoryHandler>();

        // Version use case handlers
        services.AddScoped<UploadVersionHandler>();

        // Tag use case handlers
        services.AddScoped<CreateTagHandler>();
        services.AddScoped<GetTagsHandler>();
        services.AddScoped<AddTagToDocumentHandler>();
        services.AddScoped<RemoveTagFromDocumentHandler>();

        // Workflow use case handlers
        services.AddScoped<CreateWorkflowHandler>();
        services.AddScoped<GetWorkflowsHandler>();

        // Approval use case handlers
        services.AddScoped<SubmitForApprovalHandler>();
        services.AddScoped<ApproveStepHandler>();
        services.AddScoped<RejectStepHandler>();
        services.AddScoped<GetApprovalStatusHandler>();

        return services;
    }
}
