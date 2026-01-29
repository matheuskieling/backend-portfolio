# Backend Portfolio - .NET

.NET 8 implementation of the backend portfolio project.

## Quick Start

```bash
# Run with Docker
docker-compose up -d

# Run migrations
dotnet ef database update --project src/Modules/Identity/Identity.Infrastructure
dotnet ef database update --project src/Modules/DocumentManager/DocumentManager.Infrastructure

# Run the API
dotnet run --project src/Portfolio.Api
```

## Modules

| Module | Description | Documentation |
|--------|-------------|---------------|
| **Identity** | Authentication, authorization, JWT, RBAC | [View docs](docs/identity.md) |
| **DocumentManager** | Document management with approval workflows | [View docs](docs/document-manager.md) |

## Technology Stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL (containerized)
- JWT Authentication
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
    └── DocumentManager/     # Document management module
        ├── DocumentManager.Domain
        ├── DocumentManager.Application
        └── DocumentManager.Infrastructure
```

## API Endpoints Overview

### Identity (`/api/identity`)
- `POST /register` - Create new user account
- `POST /login` - Authenticate and get JWT token

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

See module documentation for detailed API specifications.
