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
