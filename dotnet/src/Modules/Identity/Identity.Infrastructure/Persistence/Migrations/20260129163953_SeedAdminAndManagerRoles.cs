using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminAndManagerRoles : Migration
    {
        // Role GUIDs
        private static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid ManagerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Permission GUIDs
        private static readonly Guid AdminAccessPanelId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private static readonly Guid AdminManagePermissionsId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        private static readonly Guid AdminManageRolesId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        private static readonly Guid AdminManageUsersId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        private static readonly Guid AdminViewUsersId = Guid.Parse("77777777-7777-7777-7777-777777777777");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = DateTime.UtcNow;

            // Insert admin permissions
            migrationBuilder.Sql($@"
                INSERT INTO dotnet_identity.permissions (""Id"", ""Name"", ""Description"", ""CreatedAt"", ""IsDeleted"")
                VALUES
                    ('{AdminAccessPanelId}', 'admin:access_panel', 'Access admin panel', '{now:O}', false),
                    ('{AdminManagePermissionsId}', 'admin:manage_permissions', 'Create, update and delete permissions', '{now:O}', false),
                    ('{AdminManageRolesId}', 'admin:manage_roles', 'Create, update and delete roles', '{now:O}', false),
                    ('{AdminManageUsersId}', 'admin:manage_users', 'Manage all users', '{now:O}', false),
                    ('{AdminViewUsersId}', 'admin:view_users', 'View user information', '{now:O}', false);
            ");

            // Insert ADMIN and MANAGER roles
            migrationBuilder.Sql($@"
                INSERT INTO dotnet_identity.roles (""Id"", ""Name"", ""Description"", ""CreatedAt"", ""IsDeleted"")
                VALUES
                    ('{AdminRoleId}', 'ADMIN', 'Full system access - bypasses all permission checks', '{now:O}', false),
                    ('{ManagerRoleId}', 'MANAGER', 'Self-management access and user viewing', '{now:O}', false);
            ");

            // Assign all admin permissions to ADMIN role
            migrationBuilder.Sql($@"
                INSERT INTO dotnet_identity.role_permissions (""Id"", ""RoleId"", ""PermissionId"", ""AssignedAt"")
                VALUES
                    ('{Guid.NewGuid()}', '{AdminRoleId}', '{AdminAccessPanelId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{AdminRoleId}', '{AdminManagePermissionsId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{AdminRoleId}', '{AdminManageRolesId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{AdminRoleId}', '{AdminManageUsersId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{AdminRoleId}', '{AdminViewUsersId}', '{now:O}');
            ");

            // Assign limited permissions to MANAGER role
            migrationBuilder.Sql($@"
                INSERT INTO dotnet_identity.role_permissions (""Id"", ""RoleId"", ""PermissionId"", ""AssignedAt"")
                VALUES
                    ('{Guid.NewGuid()}', '{ManagerRoleId}', '{AdminAccessPanelId}', '{now:O}'),
                    ('{Guid.NewGuid()}', '{ManagerRoleId}', '{AdminViewUsersId}', '{now:O}');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove role_permissions first (FK constraint)
            migrationBuilder.Sql($@"
                DELETE FROM dotnet_identity.role_permissions
                WHERE ""RoleId"" IN ('{AdminRoleId}', '{ManagerRoleId}');
            ");

            // Remove roles
            migrationBuilder.Sql($@"
                DELETE FROM dotnet_identity.roles
                WHERE ""Id"" IN ('{AdminRoleId}', '{ManagerRoleId}');
            ");

            // Remove permissions
            migrationBuilder.Sql($@"
                DELETE FROM dotnet_identity.permissions
                WHERE ""Id"" IN (
                    '{AdminAccessPanelId}',
                    '{AdminManagePermissionsId}',
                    '{AdminManageRolesId}',
                    '{AdminManageUsersId}',
                    '{AdminViewUsersId}'
                );
            ");
        }
    }
}
