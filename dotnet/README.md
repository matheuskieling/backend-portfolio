# Backend Portfolio - .NET

.NET 8 implementation of the backend portfolio project.

## Quick Start

```bash
# Run with Docker
docker-compose up -d

# Run migrations
dotnet ef database update --project src/Modules/Identity/Identity.Infrastructure
dotnet ef database update --project src/Modules/DocumentManager/DocumentManager.Infrastructure
dotnet ef database update --project src/Modules/Scheduling/Scheduling.Infrastructure

# Run the API
dotnet run --project src/Portfolio.Api
```

## Modules

| Module | Description | Documentation |
|--------|-------------|---------------|
| **Identity** | Authentication, authorization, JWT, RBAC, Admin Panel | [View docs](docs/identity.md) |
| **DocumentManager** | Document management with approval workflows | [View docs](docs/document-manager.md) |
| **Scheduling** | Appointment scheduling with availability management | [View docs](docs/scheduling.md) |

## Technology Stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL (containerized)
- JWT Authentication
- RBAC (Roles + Permissions)
- xUnit + Testcontainers

## Project Structure

```
src/
├── Portfolio.Api/           # API layer (controllers, contracts)
├── Common/                  # Shared infrastructure and contracts
└── Modules/
    ├── Identity/            # Authentication & authorization module
    │   ├── Identity.Domain
    │   ├── Identity.Application
    │   └── Identity.Infrastructure
    ├── DocumentManager/     # Document management module
    │   ├── DocumentManager.Domain
    │   ├── DocumentManager.Application
    │   └── DocumentManager.Infrastructure
    └── Scheduling/          # Appointment scheduling module
        ├── Scheduling.Domain
        ├── Scheduling.Application
        └── Scheduling.Infrastructure
```

## API Endpoints Overview

### Identity (`/api/identity`)
- `POST /register` - Create new user account
- `POST /login` - Authenticate and get JWT token

### Admin Panel (`/api/admin`)
- `GET /me` - Get current user with roles and permissions
- `GET /permissions` - List all permissions
- `GET /permissions/{id}` - Get permission details
- `POST /permissions` - Create permission (ADMIN)
- `DELETE /permissions/{id}` - Delete permission (ADMIN)
- `GET /roles` - List all roles with permissions
- `GET /roles/{id}` - Get role details
- `POST /roles` - Create role (ADMIN)
- `DELETE /roles/{id}` - Delete role (ADMIN)
- `POST /roles/{roleId}/permissions/{permissionId}` - Assign permission to role (ADMIN)
- `DELETE /roles/{roleId}/permissions/{permissionId}` - Remove permission from role (ADMIN)
- `GET /users` - List users (self only - privacy protection)
- `GET /users/{id}` - Get user details (self only)
- `POST /users/{userId}/roles/{roleId}` - Assign role to user (self only)
- `DELETE /users/{userId}/roles/{roleId}` - Remove role from user (self only)

### Document Manager (`/api/document-manager`)
- `POST /documents` - Create document
- `GET /documents` - List documents (paginated, filtered)
- `GET /documents/{id}` - Get document details
- `PUT /documents/{id}` - Update document
- `DELETE /documents/{id}` - Delete document
- `POST /documents/{id}/versions` - Upload new version
- `POST /documents/{id}/submit` - Submit for approval
- `POST /folders` - Create folder
- `GET /folders/tree` - Get folder hierarchy
- `POST /tags` - Create tag
- `GET /tags` - List tags
- `POST /workflows` - Create approval workflow
- `GET /workflows` - List workflows
- `POST /approvals/{id}/approve` - Approve step
- `POST /approvals/{id}/reject` - Reject step

### Scheduling (`/api/scheduling`)
- `POST /profiles` - Create scheduling profile
- `GET /profiles/me` - List my profiles
- `GET /profiles/{id}` - Get profile (public)
- `DELETE /profiles/{id}` - Delete profile
- `POST /profiles/{id}/schedules` - Create recurring schedule
- `GET /profiles/{id}/schedules` - List schedules
- `POST /profiles/{id}/schedules/{id}/generate` - Generate availabilities
- `POST /profiles/{id}/schedules/{id}/pause` - Pause schedule
- `POST /profiles/{id}/schedules/{id}/resume` - Resume schedule
- `POST /profiles/{id}/availabilities` - Create single availability
- `GET /profiles/{id}/availabilities` - List availabilities
- `GET /profiles/{id}/slots` - Get available slots (public)
- `POST /profiles/{id}/slots/block` - Block slots
- `POST /profiles/{id}/slots/unblock` - Unblock slots
- `POST /profiles/{id}/appointments` - Book appointment
- `GET /profiles/{id}/appointments` - List appointments
- `POST /profiles/{id}/appointments/{id}/cancel` - Cancel appointment
- `POST /profiles/{id}/appointments/{id}/complete` - Complete appointment

See module documentation for detailed API specifications.
