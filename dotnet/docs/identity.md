# Identity Module

Authentication and authorization module implementing JWT-based authentication with Role-Based Access Control (RBAC).

## Features

- User registration and authentication
- JWT token generation with embedded roles and permissions
- Password hashing with BCrypt (cost factor 12)
- Account lockout after failed login attempts
- Role and permission management
- Cross-module user query service

## API Endpoints

Base path: `/api/identity`

### Register User

```http
POST /api/identity/register
```

Creates a new user account.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response:** `201 Created`
```json
{
  "userId": "uuid",
  "email": "user@example.com",
  "fullName": "John Doe"
}
```

**Note:** New users are automatically assigned the MANAGER role upon registration.

**Errors:**
- `400` - Invalid request (validation errors)
- `409` - Email already exists
- `429` - Rate limited

---

### Login

```http
POST /api/identity/login
```

Authenticates user and returns JWT token.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response:** `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "userId": "uuid",
  "email": "user@example.com",
  "fullName": "John Doe",
  "roles": ["User"]
}
```

**Errors:**
- `400` - Invalid credentials or account locked/deactivated
- `429` - Rate limited

---

## Admin Panel

Base path: `/api/admin`

The Admin Panel provides RBAC (Role-Based Access Control) management capabilities. All endpoints require authentication.

**Role Hierarchy:**
- **ADMIN**: Full system access - bypasses all permission checks
- **MANAGER**: Can view roles/permissions and manage their own role assignments

**Demo Note:** New users automatically receive the MANAGER role upon registration, allowing them to self-assign additional roles including ADMIN to explore all features.

> **Why Privacy Restrictions Exist**
>
> In a production system, administrators would have full access to view and manage all users.
> However, this is a **portfolio/demo application** where anyone can register and self-assign
> the ADMIN role to explore the system's capabilities.
>
> To protect user privacy in this open environment, **all users (including admins) can only
> view and modify their own information**. This prevents one user from accessing another
> user's personal data, even if they have elevated themselves to ADMIN.
>
> This is an intentional design decision specific to this demo application.

### Current User

#### Get Current User

```http
GET /api/admin/me
Authorization: Bearer {token}
```

Returns the authenticated user's details including roles and permissions.

**Response:** `200 OK`
```json
{
  "id": "uuid",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "fullName": "John Doe",
  "status": "Active",
  "createdAt": "2024-01-15T10:00:00Z",
  "lastLoginAt": "2024-01-20T15:30:00Z",
  "roles": ["MANAGER"],
  "permissions": ["document:create", "document:read"]
}
```

---

### Permissions

#### List Permissions

```http
GET /api/admin/permissions
Authorization: Bearer {token}
```

Returns all available permissions in the system.

**Response:** `200 OK`
```json
{
  "permissions": [
    {
      "id": "uuid",
      "name": "DOCUMENT:CREATE",
      "description": "Allows creating documents",
      "createdAt": "2024-01-15T10:00:00Z"
    }
  ]
}
```

---

#### Get Permission by ID

```http
GET /api/admin/permissions/{id}
Authorization: Bearer {token}
```

---

#### Create Permission

```http
POST /api/admin/permissions
Authorization: Bearer {token}
```

**Requires ADMIN role.**

**Request:**
```json
{
  "name": "report:generate",
  "description": "Allows generating reports"
}
```

**Response:** `201 Created`
```json
{
  "id": "uuid",
  "name": "REPORT:GENERATE",
  "description": "Allows generating reports"
}
```

**Errors:**
- `409` - Permission already exists

---

#### Delete Permission

```http
DELETE /api/admin/permissions/{id}
Authorization: Bearer {token}
```

**Requires ADMIN role.** Removes the permission from all roles.

---

### Roles

#### List Roles

```http
GET /api/admin/roles
Authorization: Bearer {token}
```

Returns all roles with their assigned permissions.

**Response:** `200 OK`
```json
{
  "roles": [
    {
      "id": "uuid",
      "name": "MANAGER",
      "description": "Can manage documents and self-assign roles",
      "createdAt": "2024-01-15T10:00:00Z",
      "permissions": ["document:create", "document:read"]
    }
  ]
}
```

---

#### Get Role by ID

```http
GET /api/admin/roles/{id}
Authorization: Bearer {token}
```

---

#### Create Role

```http
POST /api/admin/roles
Authorization: Bearer {token}
```

**Requires ADMIN role.**

**Request:**
```json
{
  "name": "editor",
  "description": "Can edit and publish content"
}
```

**Response:** `201 Created`
```json
{
  "id": "uuid",
  "name": "EDITOR",
  "description": "Can edit and publish content"
}
```

**Errors:**
- `409` - Role already exists

---

#### Delete Role

```http
DELETE /api/admin/roles/{id}
Authorization: Bearer {token}
```

**Requires ADMIN role.** Removes the role from all users.

---

#### Assign Permission to Role

```http
POST /api/admin/roles/{roleId}/permissions/{permissionId}
Authorization: Bearer {token}
```

**Requires ADMIN role.**

**Response:** `200 OK` - Returns the updated role with permissions.

---

#### Remove Permission from Role

```http
DELETE /api/admin/roles/{roleId}/permissions/{permissionId}
Authorization: Bearer {token}
```

**Requires ADMIN role.**

---

### Users

#### List Users

```http
GET /api/admin/users
Authorization: Bearer {token}
```

**Portfolio Restriction:** Returns only the current authenticated user. In a production system, admins would see all users.

---

#### Get User by ID

```http
GET /api/admin/users/{id}
Authorization: Bearer {token}
```

**Portfolio Restriction:** Users can only view their own details. In a production system, admins would be able to view any user.

**Response:** `200 OK`
```json
{
  "id": "uuid",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "fullName": "John Doe",
  "status": "Active",
  "createdAt": "2024-01-15T10:00:00Z",
  "lastLoginAt": "2024-01-20T15:30:00Z",
  "roles": ["MANAGER"],
  "permissions": ["document:create", "document:read"]
}
```

---

#### Assign Role to User

```http
POST /api/admin/users/{userId}/roles/{roleId}
Authorization: Bearer {token}
```

**Portfolio Restriction:** Users can only assign roles to themselves. In a production system, admins would be able to manage any user's roles.

**Response:** `204 No Content`

**Tip:** After assigning a role, re-login to get an updated token with the new permissions.

---

#### Remove Role from User

```http
DELETE /api/admin/users/{userId}/roles/{roleId}
Authorization: Bearer {token}
```

**Portfolio Restriction:** Users can only remove roles from themselves. In a production system, admins would be able to manage any user's roles.

**Response:** `204 No Content`

---

## Pre-configured Roles and Permissions

The system is seeded with the following roles and permissions:

### Roles

| Role | Description |
|------|-------------|
| **ADMIN** | Full system access - bypasses all permission checks |
| **MANAGER** | Self-management access and user viewing (auto-assigned on registration) |

### Permissions

| Permission | Description |
|------------|-------------|
| `admin:access_panel` | Access admin panel |
| `admin:manage_permissions` | Create, update and delete permissions |
| `admin:manage_roles` | Create, update and delete roles |
| `admin:manage_users` | Manage all users |
| `admin:view_users` | View user information |

### Role-Permission Assignments

**ADMIN role has:**
- All permissions (full access)

**MANAGER role has:**
- `admin:access_panel`
- `admin:view_users`

---

## Domain Model

### User

The core entity representing a system user.

**Properties:**
- `Email` - Unique email address (value object with validation)
- `PasswordHash` - BCrypt hashed password
- `FirstName`, `LastName` - User's name
- `Status` - Account status (PendingVerification, Active, Deactivated, Locked)
- `LastLoginAt` - Last successful login timestamp
- `FailedLoginAttempts` - Counter for lockout logic
- `LockoutEndAt` - When lockout expires

**Business Rules:**
- Email must be valid format, max 254 characters, stored lowercase
- Account locks after 5 failed login attempts (15-minute lockout)
- Only Active accounts can authenticate
- Lockout automatically expires after duration

### Role

Defines a set of permissions that can be assigned to users.

**Properties:**
- `Name` - Unique role name (normalized to UPPERCASE)
- `Description` - Human-readable description
- `Permissions` - Collection of assigned permissions

### Permission

Granular access control unit.

**Properties:**
- `Name` - Unique permission name (normalized to UPPERCASE)
- `Description` - Human-readable description

## JWT Token Claims

The generated JWT includes:

| Claim | Description |
|-------|-------------|
| `sub` | User ID |
| `email` | User email |
| `firstName` | User's first name |
| `lastName` | User's last name |
| `jti` | Unique token identifier |
| `roles` | Array of role names |
| `permissions` | Array of permission names |

Default expiration: 60 minutes

## Cross-Module Integration

Other modules can query user information via `IUserQueryService`:

```csharp
public interface IUserQueryService
{
    Task<UserBasicInfoDto?> GetByIdAsync(Guid userId);
    Task<Dictionary<Guid, UserBasicInfoDto>> GetByIdsAsync(IEnumerable<Guid> userIds);
}
```

This allows modules to resolve user information without direct database access to the Identity schema.

## Database Schema

Schema name: `dotnet_identity`

**Tables:**
- `users` - User accounts
- `roles` - Role definitions
- `permissions` - Permission definitions
- `user_roles` - User-Role assignments (M2M)
- `role_permissions` - Role-Permission assignments (M2M)
