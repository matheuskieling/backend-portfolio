using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dotnet_document_manager");

            migrationBuilder.CreateTable(
                name: "approval_workflows",
                schema: "dotnet_document_manager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_approval_workflows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "dotnet_document_manager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PerformedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "folders",
                schema: "dotnet_document_manager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ParentFolderId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_folders_folders_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalSchema: "dotnet_document_manager",
                        principalTable: "folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "dotnet_document_manager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "approval_steps",
                schema: "dotnet_document_manager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequiredRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_approval_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_approval_steps_approval_workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalSchema: "dotnet_document_manager",
                        principalTable: "approval_workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                schema: "dotnet_document_manager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CurrentVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    FolderId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_documents_folders_FolderId",
                        column: x => x.FolderId,
                        principalSchema: "dotnet_document_manager",
                        principalTable: "folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "approval_requests",
                schema: "dotnet_document_manager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStepOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_approval_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_approval_requests_approval_workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalSchema: "dotnet_document_manager",
                        principalTable: "approval_workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_approval_requests_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "dotnet_document_manager",
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_tags",
                schema: "dotnet_document_manager",
                columns: table => new
                {
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_tags", x => new { x.DocumentId, x.TagId });
                    table.ForeignKey(
                        name: "FK_document_tags_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "dotnet_document_manager",
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_tags_tags_TagId",
                        column: x => x.TagId,
                        principalSchema: "dotnet_document_manager",
                        principalTable: "tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_versions",
                schema: "dotnet_document_manager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_versions_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "dotnet_document_manager",
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "approval_decisions",
                schema: "dotnet_document_manager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovalRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Decision = table.Column<string>(type: "text", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_approval_decisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_approval_decisions_approval_requests_ApprovalRequestId",
                        column: x => x.ApprovalRequestId,
                        principalSchema: "dotnet_document_manager",
                        principalTable: "approval_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_approval_decisions_approval_steps_StepId",
                        column: x => x.StepId,
                        principalSchema: "dotnet_document_manager",
                        principalTable: "approval_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_approval_decisions_ApprovalRequestId",
                schema: "dotnet_document_manager",
                table: "approval_decisions",
                column: "ApprovalRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_approval_decisions_DecidedBy",
                schema: "dotnet_document_manager",
                table: "approval_decisions",
                column: "DecidedBy");

            migrationBuilder.CreateIndex(
                name: "IX_approval_decisions_StepId",
                schema: "dotnet_document_manager",
                table: "approval_decisions",
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "IX_approval_requests_DocumentId",
                schema: "dotnet_document_manager",
                table: "approval_requests",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_approval_requests_RequestedBy",
                schema: "dotnet_document_manager",
                table: "approval_requests",
                column: "RequestedBy");

            migrationBuilder.CreateIndex(
                name: "IX_approval_requests_Status",
                schema: "dotnet_document_manager",
                table: "approval_requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_approval_requests_WorkflowId",
                schema: "dotnet_document_manager",
                table: "approval_requests",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_approval_steps_WorkflowId_StepOrder",
                schema: "dotnet_document_manager",
                table: "approval_steps",
                columns: new[] { "WorkflowId", "StepOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityType_EntityId",
                schema: "dotnet_document_manager",
                table: "audit_logs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_PerformedAt",
                schema: "dotnet_document_manager",
                table: "audit_logs",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_PerformedBy",
                schema: "dotnet_document_manager",
                table: "audit_logs",
                column: "PerformedBy");

            migrationBuilder.CreateIndex(
                name: "IX_document_tags_TagId",
                schema: "dotnet_document_manager",
                table: "document_tags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_DocumentId_VersionNumber",
                schema: "dotnet_document_manager",
                table: "document_versions",
                columns: new[] { "DocumentId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_UploadedBy",
                schema: "dotnet_document_manager",
                table: "document_versions",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_documents_FolderId",
                schema: "dotnet_document_manager",
                table: "documents",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_documents_OwnerId",
                schema: "dotnet_document_manager",
                table: "documents",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_documents_Status",
                schema: "dotnet_document_manager",
                table: "documents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_folders_ParentFolderId",
                schema: "dotnet_document_manager",
                table: "folders",
                column: "ParentFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_tags_Name",
                schema: "dotnet_document_manager",
                table: "tags",
                column: "Name",
                unique: true);

            // Cross-schema foreign keys to dotnet_identity.users
            // Document.OwnerId -> CASCADE (documents deleted when user is deleted)
            migrationBuilder.Sql(@"
                ALTER TABLE dotnet_document_manager.documents
                ADD CONSTRAINT FK_documents_users_OwnerId
                FOREIGN KEY (""OwnerId"")
                REFERENCES dotnet_identity.users (""Id"")
                ON DELETE CASCADE;
            ");

            // DocumentVersion.UploadedBy -> SET NULL (preserve version history)
            migrationBuilder.Sql(@"
                ALTER TABLE dotnet_document_manager.document_versions
                ADD CONSTRAINT FK_document_versions_users_UploadedBy
                FOREIGN KEY (""UploadedBy"")
                REFERENCES dotnet_identity.users (""Id"")
                ON DELETE SET NULL;
            ");

            // ApprovalRequest.RequestedBy -> SET NULL (preserve approval history)
            migrationBuilder.Sql(@"
                ALTER TABLE dotnet_document_manager.approval_requests
                ADD CONSTRAINT FK_approval_requests_users_RequestedBy
                FOREIGN KEY (""RequestedBy"")
                REFERENCES dotnet_identity.users (""Id"")
                ON DELETE SET NULL;
            ");

            // ApprovalDecision.DecidedBy -> SET NULL (preserve approval decisions)
            migrationBuilder.Sql(@"
                ALTER TABLE dotnet_document_manager.approval_decisions
                ADD CONSTRAINT FK_approval_decisions_users_DecidedBy
                FOREIGN KEY (""DecidedBy"")
                REFERENCES dotnet_identity.users (""Id"")
                ON DELETE SET NULL;
            ");

            // AuditLog.PerformedBy -> SET NULL (preserve audit trail)
            migrationBuilder.Sql(@"
                ALTER TABLE dotnet_document_manager.audit_logs
                ADD CONSTRAINT FK_audit_logs_users_PerformedBy
                FOREIGN KEY (""PerformedBy"")
                REFERENCES dotnet_identity.users (""Id"")
                ON DELETE SET NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop cross-schema foreign keys first
            migrationBuilder.Sql(@"
                ALTER TABLE dotnet_document_manager.audit_logs
                DROP CONSTRAINT IF EXISTS FK_audit_logs_users_PerformedBy;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE dotnet_document_manager.approval_decisions
                DROP CONSTRAINT IF EXISTS FK_approval_decisions_users_DecidedBy;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE dotnet_document_manager.approval_requests
                DROP CONSTRAINT IF EXISTS FK_approval_requests_users_RequestedBy;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE dotnet_document_manager.document_versions
                DROP CONSTRAINT IF EXISTS FK_document_versions_users_UploadedBy;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE dotnet_document_manager.documents
                DROP CONSTRAINT IF EXISTS FK_documents_users_OwnerId;
            ");

            migrationBuilder.DropTable(
                name: "approval_decisions",
                schema: "dotnet_document_manager");

            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "dotnet_document_manager");

            migrationBuilder.DropTable(
                name: "document_tags",
                schema: "dotnet_document_manager");

            migrationBuilder.DropTable(
                name: "document_versions",
                schema: "dotnet_document_manager");

            migrationBuilder.DropTable(
                name: "approval_requests",
                schema: "dotnet_document_manager");

            migrationBuilder.DropTable(
                name: "approval_steps",
                schema: "dotnet_document_manager");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "dotnet_document_manager");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "dotnet_document_manager");

            migrationBuilder.DropTable(
                name: "approval_workflows",
                schema: "dotnet_document_manager");

            migrationBuilder.DropTable(
                name: "folders",
                schema: "dotnet_document_manager");
        }
    }
}
