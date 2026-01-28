using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedDocumentManagerRoles : Migration
    {
        // Predefined GUIDs for consistency and idempotency
        private static readonly Guid DocumentReviewerRoleId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        private static readonly Guid DocumentAdminRoleId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");

        // Permission GUIDs
        private static readonly Guid DocumentCreateId = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012");
        private static readonly Guid DocumentReadId = Guid.Parse("d4e5f6a7-b8c9-0123-def1-234567890123");
        private static readonly Guid DocumentUpdateId = Guid.Parse("e5f6a7b8-c9d0-1234-ef12-345678901234");
        private static readonly Guid DocumentDeleteId = Guid.Parse("f6a7b8c9-d0e1-2345-f123-456789012345");
        private static readonly Guid DocumentReadAllId = Guid.Parse("a7b8c9d0-e1f2-3456-0123-567890123456");
        private static readonly Guid DocumentManageAllId = Guid.Parse("b8c9d0e1-f2a3-4567-1234-678901234567");
        private static readonly Guid FolderCreateId = Guid.Parse("c9d0e1f2-a3b4-5678-2345-789012345678");
        private static readonly Guid FolderManageId = Guid.Parse("d0e1f2a3-b4c5-6789-3456-890123456789");
        private static readonly Guid WorkflowCreateId = Guid.Parse("e1f2a3b4-c5d6-7890-4567-901234567890");
        private static readonly Guid WorkflowManageId = Guid.Parse("f2a3b4c5-d6e7-8901-5678-012345678901");
        private static readonly Guid ApprovalReviewId = Guid.Parse("a3b4c5d6-e7f8-9012-6789-123456789012");
        private static readonly Guid TagCreateId = Guid.Parse("b4c5d6e7-f8a9-0123-7890-234567890123");
        private static readonly Guid TagManageId = Guid.Parse("c5d6e7f8-a9b0-1234-8901-345678901234");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = DateTime.UtcNow;

            // Insert permissions
            migrationBuilder.Sql($@"
                INSERT INTO dotnet_identity.permissions (""Id"", ""Name"", ""Description"", ""CreatedAt"", ""IsDeleted"")
                VALUES
                    ('{DocumentCreateId}', 'document:create', 'Create documents', '{now:O}', false),
                    ('{DocumentReadId}', 'document:read', 'Read own documents', '{now:O}', false),
                    ('{DocumentUpdateId}', 'document:update', 'Update own documents', '{now:O}', false),
                    ('{DocumentDeleteId}', 'document:delete', 'Delete own documents', '{now:O}', false),
                    ('{DocumentReadAllId}', 'document:read_all', 'Read all documents', '{now:O}', false),
                    ('{DocumentManageAllId}', 'document:manage_all', 'Manage all documents', '{now:O}', false),
                    ('{FolderCreateId}', 'folder:create', 'Create folders', '{now:O}', false),
                    ('{FolderManageId}', 'folder:manage', 'Manage all folders', '{now:O}', false),
                    ('{WorkflowCreateId}', 'workflow:create', 'Create approval workflows', '{now:O}', false),
                    ('{WorkflowManageId}', 'workflow:manage', 'Manage approval workflows', '{now:O}', false),
                    ('{ApprovalReviewId}', 'approval:review', 'Review and approve documents', '{now:O}', false),
                    ('{TagCreateId}', 'tag:create', 'Create tags', '{now:O}', false),
                    ('{TagManageId}', 'tag:manage', 'Manage all tags', '{now:O}', false);
            ");

            // Insert roles
            migrationBuilder.Sql($@"
                INSERT INTO dotnet_identity.roles (""Id"", ""Name"", ""Description"", ""CreatedAt"", ""IsDeleted"")
                VALUES
                    ('{DocumentReviewerRoleId}', 'DOCUMENT_REVIEWER', 'Can approve documents in workflows', '{now:O}', false),
                    ('{DocumentAdminRoleId}', 'DOCUMENT_ADMIN', 'Full document management access', '{now:O}', false);
            ");

            // Assign permissions to DOCUMENT_REVIEWER role
            // Reviewers can: read documents, review approvals
            migrationBuilder.Sql($@"
                INSERT INTO dotnet_identity.role_permissions (""Id"", ""RoleId"", ""PermissionId"", ""AssignedAt"")
                VALUES
                    ('{Guid.NewGuid()}', '{DocumentReviewerRoleId}', '{DocumentReadId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{DocumentReviewerRoleId}', '{DocumentReadAllId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{DocumentReviewerRoleId}', '{ApprovalReviewId}', '{now:O}');
            ");

            // Assign all permissions to DOCUMENT_ADMIN role
            migrationBuilder.Sql($@"
                INSERT INTO dotnet_identity.role_permissions (""Id"", ""RoleId"", ""PermissionId"", ""AssignedAt"")
                VALUES
                    ('{Guid.NewGuid()}', '{DocumentAdminRoleId}', '{DocumentCreateId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{DocumentAdminRoleId}', '{DocumentReadId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{DocumentAdminRoleId}', '{DocumentUpdateId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{DocumentAdminRoleId}', '{DocumentDeleteId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{DocumentAdminRoleId}', '{DocumentReadAllId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{DocumentAdminRoleId}', '{DocumentManageAllId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{DocumentAdminRoleId}', '{FolderCreateId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{DocumentAdminRoleId}', '{FolderManageId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{DocumentAdminRoleId}', '{WorkflowCreateId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{DocumentAdminRoleId}', '{WorkflowManageId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{DocumentAdminRoleId}', '{ApprovalReviewId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{DocumentAdminRoleId}', '{TagCreateId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{DocumentAdminRoleId}', '{TagManageId}', '{now:O}');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove role_permissions first (FK constraint)
            migrationBuilder.Sql($@"
                DELETE FROM dotnet_identity.role_permissions
                WHERE ""RoleId"" IN ('{DocumentReviewerRoleId}', '{DocumentAdminRoleId}');
            ");

            // Remove roles
            migrationBuilder.Sql($@"
                DELETE FROM dotnet_identity.roles
                WHERE ""Id"" IN ('{DocumentReviewerRoleId}', '{DocumentAdminRoleId}');
            ");

            // Remove permissions
            migrationBuilder.Sql($@"
                DELETE FROM dotnet_identity.permissions
                WHERE ""Id"" IN (
                    '{DocumentCreateId}',
                    '{DocumentReadId}',
                    '{DocumentUpdateId}',
                    '{DocumentDeleteId}',
                    '{DocumentReadAllId}',
                    '{DocumentManageAllId}',
                    '{FolderCreateId}',
                    '{FolderManageId}',
                    '{WorkflowCreateId}',
                    '{WorkflowManageId}',
                    '{ApprovalReviewId}',
                    '{TagCreateId}',
                    '{TagManageId}'
                );
            ");
        }
    }
}
