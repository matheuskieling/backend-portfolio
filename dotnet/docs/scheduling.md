# Scheduling Module

Appointment scheduling system where users can define availability time slots and other users can book appointments within those slots.

## Features

- Scheduling profiles (Individual or Business)
- Recurring schedule patterns (e.g., Mon-Fri 9am-5pm)
- Flexible availability management
- Automatic time slot generation
- Slot blocking for exceptions
- Appointment booking with validation
- Self-booking prevention
- Booking window constraints (min advance, max future)
- Cancellation deadline enforcement

## API Endpoints

All scheduling endpoints are profile-scoped. Users can have multiple profiles (max 1 Individual, unlimited Business).

### Profiles

Base path: `/api/scheduling/profiles`

#### Create Profile

```http
POST /api/scheduling/profiles
Authorization: Bearer {token}
```

**Request:**
```json
{
  "type": "Individual",
  "displayName": "John's Consulting",
  "businessName": null
}
```

For Business profiles, `businessName` is required and must be unique per user.

**Response:** `201 Created`
```json
{
  "id": "uuid",
  "type": "Individual",
  "displayName": "John's Consulting",
  "businessName": null,
  "createdAt": "2024-01-15T10:00:00Z"
}
```

**Errors:**
- `400` - Validation error or user already has an Individual profile
- `409` - Business name already exists for this user

---

#### Get My Profiles

```http
GET /api/scheduling/profiles/me
Authorization: Bearer {token}
```

Returns all profiles owned by the authenticated user.

---

#### Get Profile by ID

```http
GET /api/scheduling/profiles/{profileId}
```

Public endpoint - no authentication required. Returns basic profile information.

---

#### Delete Profile

```http
DELETE /api/scheduling/profiles/{profileId}
Authorization: Bearer {token}
```

Only the profile owner can delete. Cannot delete if profile has existing appointments.

---

### Schedules

Schedules define recurring availability patterns that generate concrete availabilities.

Base path: `/api/scheduling/profiles/{profileId}/schedules`

#### Create Schedule

```http
POST /api/scheduling/profiles/{profileId}/schedules
Authorization: Bearer {token}
```

**Request:**
```json
{
  "name": "Morning Hours",
  "daysOfWeek": [1, 2, 3, 4, 5],
  "startTimeOfDay": "09:00",
  "endTimeOfDay": "12:00",
  "slotDurationMinutes": 60,
  "effectiveFrom": "2024-02-01",
  "effectiveUntil": null
}
```

Days of week: 0=Sunday, 1=Monday, ..., 6=Saturday

**Response:** `201 Created`
```json
{
  "id": "uuid",
  "name": "Morning Hours",
  "daysOfWeek": [1, 2, 3, 4, 5],
  "startTimeOfDay": "09:00",
  "endTimeOfDay": "12:00",
  "slotDurationMinutes": 60,
  "effectiveFrom": "2024-02-01",
  "effectiveUntil": null,
  "isActive": true
}
```

---

#### List Schedules

```http
GET /api/scheduling/profiles/{profileId}/schedules
Authorization: Bearer {token}
```

---

#### Get Schedule Details

```http
GET /api/scheduling/profiles/{profileId}/schedules/{scheduleId}
Authorization: Bearer {token}
```

---

#### Update Schedule

```http
PUT /api/scheduling/profiles/{profileId}/schedules/{scheduleId}
Authorization: Bearer {token}
```

**Request:**
```json
{
  "name": "Updated Name",
  "daysOfWeek": [1, 2, 3],
  "startTimeOfDay": "08:00",
  "endTimeOfDay": "11:00",
  "slotDurationMinutes": 30,
  "effectiveFrom": "2024-02-01",
  "effectiveUntil": "2024-12-31",
  "minAdvanceBookingMinutes": 60,
  "maxAdvanceBookingDays": 30,
  "cancellationDeadlineMinutes": 60
}
```

---

#### Delete Schedule

```http
DELETE /api/scheduling/profiles/{profileId}/schedules/{scheduleId}
Authorization: Bearer {token}
```

Deleting a schedule does NOT delete already-generated availabilities.

---

#### Pause Schedule

```http
POST /api/scheduling/profiles/{profileId}/schedules/{scheduleId}/pause
Authorization: Bearer {token}
```

Paused schedules cannot generate new availabilities.

---

#### Resume Schedule

```http
POST /api/scheduling/profiles/{profileId}/schedules/{scheduleId}/resume
Authorization: Bearer {token}
```

---

#### Generate Availabilities

```http
POST /api/scheduling/profiles/{profileId}/schedules/{scheduleId}/generate
Authorization: Bearer {token}
```

Generates concrete availabilities based on the schedule pattern for a date range.

**Request:**
```json
{
  "fromDate": "2024-02-01",
  "toDate": "2024-02-28"
}
```

**Response:** `200 OK`
```json
{
  "generatedCount": 20,
  "skippedCount": 2,
  "availabilities": [
    {
      "date": "2024-02-01",
      "slotCount": 3
    }
  ]
}
```

Skipped dates are those where an availability already exists (overlap prevention).

---

### Availabilities

Availabilities represent concrete time windows with bookable time slots.

Base path: `/api/scheduling/profiles/{profileId}/availabilities`

#### Create Availability (Direct)

```http
POST /api/scheduling/profiles/{profileId}/availabilities
Authorization: Bearer {token}
```

Create a single-occurrence availability (not from a schedule).

**Request:**
```json
{
  "startTime": "2024-02-15T09:00:00Z",
  "endTime": "2024-02-15T12:00:00Z",
  "slotDurationMinutes": 60
}
```

**Response:** `201 Created`
```json
{
  "id": "uuid",
  "startTime": "2024-02-15T09:00:00Z",
  "endTime": "2024-02-15T12:00:00Z",
  "slotDurationMinutes": 60,
  "timeSlots": [
    {
      "id": "uuid",
      "startTime": "2024-02-15T09:00:00Z",
      "endTime": "2024-02-15T10:00:00Z",
      "status": "Available"
    },
    {
      "id": "uuid",
      "startTime": "2024-02-15T10:00:00Z",
      "endTime": "2024-02-15T11:00:00Z",
      "status": "Available"
    },
    {
      "id": "uuid",
      "startTime": "2024-02-15T11:00:00Z",
      "endTime": "2024-02-15T12:00:00Z",
      "status": "Available"
    }
  ]
}
```

Time slots are automatically generated based on the duration.

**Errors:**
- `400` - Overlaps with existing availability

---

#### List Availabilities

```http
GET /api/scheduling/profiles/{profileId}/availabilities
Authorization: Bearer {token}
```

**Query Parameters:**
- `from` - Filter by start date (ISO 8601)
- `to` - Filter by end date (ISO 8601)

---

#### Get Availability Details

```http
GET /api/scheduling/profiles/{profileId}/availabilities/{availabilityId}
Authorization: Bearer {token}
```

Returns availability with all time slots.

---

#### Delete Availability

```http
DELETE /api/scheduling/profiles/{profileId}/availabilities/{availabilityId}
Authorization: Bearer {token}
```

Cannot delete if any slots are booked.

---

### Time Slots

#### Get Available Slots (Public)

```http
GET /api/scheduling/profiles/{profileId}/slots?from={date}&to={date}
```

Public endpoint - no authentication required. Returns only available (not booked or blocked) slots.

**Query Parameters:**
- `from` - Start of date range (required, ISO 8601)
- `to` - End of date range (required, ISO 8601)

**Response:** `200 OK`
```json
[
  {
    "id": "uuid",
    "availabilityId": "uuid",
    "startTime": "2024-02-15T09:00:00Z",
    "endTime": "2024-02-15T10:00:00Z"
  }
]
```

---

#### Block Slots

```http
POST /api/scheduling/profiles/{profileId}/slots/block
Authorization: Bearer {token}
```

Block multiple slots to mark them as unavailable (e.g., for lunch breaks, personal time).

**Request:**
```json
{
  "slotIds": ["uuid1", "uuid2", "uuid3"]
}
```

**Response:** `200 OK`
```json
{
  "processedCount": 3,
  "processedSlotIds": ["uuid1", "uuid2", "uuid3"]
}
```

**Errors:**
- `400` - Slot already blocked or booked

---

#### Unblock Slots

```http
POST /api/scheduling/profiles/{profileId}/slots/unblock
Authorization: Bearer {token}
```

Returns blocked slots to available status.

**Request:**
```json
{
  "slotIds": ["uuid1", "uuid2"]
}
```

---

### Appointments

Base path: `/api/scheduling/profiles/{profileId}/appointments`

#### Book Appointment

```http
POST /api/scheduling/profiles/{hostProfileId}/appointments
Authorization: Bearer {token}
```

Book an appointment with the host profile. The authenticated user must specify their own guest profile.

**Request:**
```json
{
  "guestProfileId": "uuid",
  "timeSlotId": "uuid"
}
```

**Response:** `201 Created`
```json
{
  "id": "uuid",
  "hostProfileId": "uuid",
  "guestProfileId": "uuid",
  "timeSlotId": "uuid",
  "startTime": "2024-02-15T09:00:00Z",
  "endTime": "2024-02-15T10:00:00Z",
  "status": "Scheduled",
  "isHost": false
}
```

**Errors:**
- `400` - Self-booking not allowed (host and guest same user)
- `400` - Slot not available (already booked or blocked)
- `400` - Booking window violation (too early or too late)

---

#### List Appointments

```http
GET /api/scheduling/profiles/{profileId}/appointments
Authorization: Bearer {token}
```

Returns appointments where the profile is either host or guest.

**Query Parameters:**
- `status` - Filter by status (Scheduled, Canceled, Completed)

**Response:** `200 OK`
```json
[
  {
    "id": "uuid",
    "hostProfileId": "uuid",
    "guestProfileId": "uuid",
    "startTime": "2024-02-15T09:00:00Z",
    "endTime": "2024-02-15T10:00:00Z",
    "status": "Scheduled",
    "isHost": true
  }
]
```

---

#### Get Appointment Details

```http
GET /api/scheduling/profiles/{profileId}/appointments/{appointmentId}
Authorization: Bearer {token}
```

---

#### Cancel Appointment

```http
POST /api/scheduling/profiles/{profileId}/appointments/{appointmentId}/cancel
Authorization: Bearer {token}
```

Either host or guest can cancel. Must be before the cancellation deadline.

**Response:** `204 No Content`

Canceling releases the time slot back to Available status.

**Errors:**
- `400` - Already canceled
- `400` - Cancellation deadline passed

---

#### Complete Appointment

```http
POST /api/scheduling/profiles/{profileId}/appointments/{appointmentId}/complete
Authorization: Bearer {token}
```

Only the host can mark an appointment as completed.

---

## Domain Model

### SchedulingProfile (Aggregate Root)

**Properties:**
- `ExternalUserId` - Reference to Identity user (no FK)
- `Type` - Individual or Business (immutable)
- `DisplayName` - Optional display name
- `BusinessName` - Required for Business type, unique per user

**Business Rules:**
- Max 1 Individual profile per user
- Business profiles must have unique BusinessName per user
- Profile type cannot be changed after creation

### Schedule

Defines a recurring availability pattern.

**Properties:**
- `Name` - Unique per profile
- `DaysOfWeek` - Array of days (0-6)
- `StartTimeOfDay` / `EndTimeOfDay` - Time range
- `SlotDurationMinutes` - Duration of each slot
- `EffectiveFrom` / `EffectiveUntil` - When schedule is valid
- `IsActive` - Can be paused

**Business Rules:**
- Name must be unique per profile
- End time must be after start time
- Generates availabilities only for matching days
- Paused schedules cannot generate

### Availability

A concrete date/time window with bookable slots.

**Properties:**
- `ScheduleId` - Set if generated from schedule (null if direct)
- `StartTime` / `EndTime` - UTC timestamps
- `SlotDurationMinutes` - Duration per slot
- `MinAdvanceBookingMinutes` - Minimum advance notice (default: 60)
- `MaxAdvanceBookingDays` - Maximum days ahead (default: 30)
- `CancellationDeadlineMinutes` - Deadline before slot start (default: 60)
- `TimeSlots` - Generated on creation

**Business Rules:**
- Cannot overlap with other availabilities for same profile
- Slots auto-generated based on duration

### TimeSlot

**Status Lifecycle:**
```
Available → Booked (appointment created)
Available → Blocked (owner blocks)
Booked → Available (appointment canceled)
Blocked → Available (owner unblocks)
```

**Business Rules:**
- Only Available slots can be booked
- Only Available slots can be blocked
- Booked slots cannot be blocked
- Blocking/unblocking only by profile owner

### Appointment

**Status Lifecycle:**
```
Scheduled → Canceled (by host or guest)
Scheduled → Completed (by host or auto)
```

**Business Rules:**
- Cannot book own profile (same ExternalUserId)
- Must respect MinAdvanceBookingMinutes
- Must respect MaxAdvanceBookingDays
- Cancellation must be before deadline
- Only host can mark as completed

## Database Schema

Schema name: `dotnet_scheduling`

**Tables:**
- `scheduling_profiles` - User profiles
- `schedules` - Recurring patterns
- `availabilities` - Concrete time windows
- `time_slots` - Individual bookable slots
- `appointments` - Booked appointments

**Key Indexes:**
- Unique partial index on `(ExternalUserId)` where `Type = 'Individual'`
- Unique index on `(ExternalUserId, BusinessName)` for Business profiles
- Unique index on `(ProfileId, Name)` for schedules
- Overlap detection index on `(HostProfileId, StartTime, EndTime)` for availabilities
- Status + time index on time_slots for available slots query

## Usage Examples

### Setting Up Availability

1. **Create a profile:**
```bash
POST /api/scheduling/profiles
{ "type": "Individual", "displayName": "Dr. Smith" }
```

2. **Create a recurring schedule:**
```bash
POST /api/scheduling/profiles/{id}/schedules
{
  "name": "Weekday Hours",
  "daysOfWeek": [1, 2, 3, 4, 5],
  "startTimeOfDay": "09:00",
  "endTimeOfDay": "17:00",
  "slotDurationMinutes": 30,
  "effectiveFrom": "2024-02-01"
}
```

3. **Generate availabilities:**
```bash
POST /api/scheduling/profiles/{id}/schedules/{scheduleId}/generate
{ "fromDate": "2024-02-01", "toDate": "2024-02-28" }
```

4. **Block lunch hours:**
```bash
POST /api/scheduling/profiles/{id}/slots/block
{ "slotIds": ["12pm-slot-id", "12:30pm-slot-id"] }
```

### Booking an Appointment

1. **Guest views available slots:**
```bash
GET /api/scheduling/profiles/{hostId}/slots?from=2024-02-15&to=2024-02-16
```

2. **Guest books a slot:**
```bash
POST /api/scheduling/profiles/{hostId}/appointments
{ "guestProfileId": "{guestProfileId}", "timeSlotId": "{slotId}" }
```

3. **Guest can cancel if needed:**
```bash
POST /api/scheduling/profiles/{guestProfileId}/appointments/{id}/cancel
```
