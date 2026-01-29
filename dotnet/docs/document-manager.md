# Document Manager Module

Document management system with versioning, folder organization, tagging, and multi-step approval workflows.

## Features

- Document CRUD with soft delete
- Version management (file uploads)
- Hierarchical folder organization
- Tag-based categorization
- Configurable multi-step approval workflows
- Role-based approval steps
- Complete audit trail

## API Endpoints

Base path: `/api/document-manager`

### Documents

#### Create Document

```http
POST /api/document-manager/documents
Authorization: Bearer {token}
```

**Request:**
```json
{
  "title": "Project Proposal",
  "description": "Q1 2024 project proposal",
  "folderId": "uuid (optional)"
}
```

**Response:** `201 Created`
```json
{
  "id": "uuid",
  "title": "Project Proposal"
}
```

---

#### List Documents

```http
GET /api/document-manager/documents
Authorization: Bearer {token}
```

**Query Parameters:**
- `title` - Filter by title (contains)
- `folderId` - Filter by folder
- `ownerId` - Filter by owner
- `status` - Filter by status (Draft, PendingApproval, Approved, Rejected)
- `tagIds` - Filter by tags
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 10)

**Response:** `200 OK`
```json
{
  "items": [
    {
      "id": "uuid",
      "title": "Project Proposal",
      "description": "Q1 2024 project proposal",
      "status": "Draft",
      "currentVersionNumber": 1,
      "ownerId": "uuid",
      "folderId": "uuid",
      "createdAt": "2024-01-15T10:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10
}
```

---

#### Get Document Details

```http
GET /api/document-manager/documents/{id}
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "id": "uuid",
  "title": "Project Proposal",
  "description": "Q1 2024 project proposal",
  "status": "Draft",
  "currentVersionNumber": 2,
  "ownerId": "uuid",
  "folderId": "uuid",
  "createdAt": "2024-01-15T10:00:00Z",
  "versions": [
    {
      "versionNumber": 1,
      "fileName": "proposal_v1.pdf",
      "mimeType": "application/pdf",
      "fileSize": 102400,
      "uploadedBy": "uuid",
      "uploadedAt": "2024-01-15T10:00:00Z"
    }
  ],
  "tags": [
    { "id": "uuid", "name": "proposal" }
  ]
}
```

---

#### Update Document

```http
PUT /api/document-manager/documents/{id}
Authorization: Bearer {token}
```

Only draft documents can be updated. Only the owner can update.

**Request:**
```json
{
  "title": "Updated Title",
  "description": "Updated description"
}
```

---

#### Delete Document

```http
DELETE /api/document-manager/documents/{id}
Authorization: Bearer {token}
```

Soft delete. Only draft documents can be deleted. Only the owner can delete.

---

#### Upload Version

```http
POST /api/document-manager/documents/{id}/versions
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

Maximum file size: 100MB. Only draft documents can have versions added.

**Form Data:**
- `file` - The file to upload

**Response:** `201 Created`
```json
{
  "versionId": "uuid",
  "versionNumber": 2,
  "fileName": "proposal_v2.pdf"
}
```

---

#### Get Document History

```http
GET /api/document-manager/documents/{id}/history
Authorization: Bearer {token}
```

Returns audit trail for the document.

---

### Folders

#### Create Folder

```http
POST /api/document-manager/folders
Authorization: Bearer {token}
```

**Request:**
```json
{
  "name": "Projects",
  "parentFolderId": "uuid (optional)"
}
```

---

#### Get Folder Tree

```http
GET /api/document-manager/folders/tree
Authorization: Bearer {token}
```

Returns complete folder hierarchy.

**Response:** `200 OK`
```json
[
  {
    "id": "uuid",
    "name": "Projects",
    "parentFolderId": null,
    "children": [
      {
        "id": "uuid",
        "name": "2024",
        "parentFolderId": "uuid",
        "children": []
      }
    ]
  }
]
```

---

### Tags

#### Create Tag

```http
POST /api/document-manager/tags
Authorization: Bearer {token}
```

**Request:**
```json
{
  "name": "proposal"
}
```

Tag names are normalized to lowercase, max 50 characters.

---

#### List Tags

```http
GET /api/document-manager/tags
Authorization: Bearer {token}
```

---

#### Add Tag to Document

```http
POST /api/document-manager/documents/{documentId}/tags
Authorization: Bearer {token}
```

**Request:**
```json
{
  "tagId": "uuid"
}
```

---

#### Remove Tag from Document

```http
DELETE /api/document-manager/documents/{documentId}/tags/{tagId}
Authorization: Bearer {token}
```

---

### Approval Workflows

#### Create Workflow

```http
POST /api/document-manager/workflows
Authorization: Bearer {token}
```

**Request:**
```json
{
  "name": "Standard Approval",
  "description": "Two-step approval process",
  "steps": [
    {
      "stepOrder": 1,
      "name": "Manager Review",
      "description": "Initial review by manager",
      "requiredRole": "Manager"
    },
    {
      "stepOrder": 2,
      "name": "Director Approval",
      "description": "Final approval by director",
      "requiredRole": "Director"
    }
  ]
}
```

---

#### List Workflows

```http
GET /api/document-manager/workflows
Authorization: Bearer {token}
```

**Query Parameters:**
- `activeOnly` - Filter to active workflows only (default: true)

---

### Approval Process

#### Submit for Approval

```http
POST /api/document-manager/documents/{documentId}/submit
Authorization: Bearer {token}
```

Document must be in Draft status and have at least one version.

**Request:**
```json
{
  "workflowId": "uuid"
}
```

**Response:** `200 OK`
```json
{
  "approvalRequestId": "uuid",
  "documentId": "uuid",
  "documentTitle": "Project Proposal"
}
```

---

#### Approve Step

```http
POST /api/document-manager/approvals/{approvalRequestId}/approve
Authorization: Bearer {token}
```

User must have the role required for the current step.

**Request:**
```json
{
  "comment": "Looks good, approved."
}
```

**Response:** `200 OK`
```json
{
  "approvalRequestId": "uuid",
  "stepApproved": "Manager Review",
  "newStatus": "InProgress"
}
```

If this was the final step, `newStatus` will be `Approved`.

---

#### Reject Step

```http
POST /api/document-manager/approvals/{approvalRequestId}/reject
Authorization: Bearer {token}
```

Rejection immediately ends the workflow.

**Request:**
```json
{
  "comment": "Needs revision, see comments."
}
```

**Response:** `200 OK`
```json
{
  "approvalRequestId": "uuid",
  "stepRejected": "Manager Review",
  "newStatus": "Rejected"
}
```

---

#### Get Approval Status

```http
GET /api/document-manager/approvals/{approvalRequestId}
Authorization: Bearer {token}
```

Returns approval request with all decisions made.

## Domain Model

### Document (Aggregate Root)

**Status Lifecycle:**
```
Draft → PendingApproval → Approved
                       → Rejected
```

**Business Rules:**
- Only Draft documents can be modified or deleted
- Only the owner can modify/delete their documents
- Documents must have at least one version before submission
- Once submitted, document enters PendingApproval and cannot be modified

### Folder

Hierarchical folder structure for organizing documents.

**Business Rules:**
- Folders can have optional parent folder
- A folder cannot be its own parent

### ApprovalWorkflow

Defines a reusable approval process with sequential steps.

**Business Rules:**
- Must have at least one step
- Steps must have unique order numbers
- Only active workflows can be used for submissions

### ApprovalRequest

Tracks a document's journey through an approval workflow.

**Business Rules:**
- Starts at step 1
- Approval advances to next step (or completes if last step)
- Rejection immediately ends the workflow
- Only users with the required role can approve/reject a step

## Database Schema

Schema name: `dotnet_document_manager`

**Tables:**
- `documents` - Document metadata
- `document_versions` - Version history with file references
- `document_tags` - Document-Tag assignments
- `folders` - Folder hierarchy
- `tags` - Tag definitions
- `approval_workflows` - Workflow definitions
- `approval_steps` - Workflow step definitions
- `approval_requests` - Active approval processes
- `approval_decisions` - Approval/rejection history
- `audit_logs` - Document change history
