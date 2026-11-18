# RESTful Web API Specification and Developer Guide

**Base URL:** `https://localhost:5001/api/v1` or `http://localhost:5000/api/v1`  
**API Version:** `v1`  
**Authentication Scheme:** `Bearer <JWT_TOKEN>`  
**Interactive Swagger UI:** `http://localhost:5000/swagger`  
**Postman Collection:** `NashaMukti_API_Postman_Collection.json`  

---

## 1. Standard Response Format

All API responses are wrapped in a standard JSON envelope:

### Successful Response Format
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { ... }
}
```

### Paginated List Format
```json
{
  "success": true,
  "message": null,
  "page": 1,
  "pageSize": 10,
  "totalCount": 24,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true,
  "items": [ ... ]
}
```

### Error Response Format (400, 401, 403, 404, 500)
```json
{
  "success": false,
  "message": "Detailed error message",
  "errors": [
    "The Email field is required.",
    "Invalid contact telephone format."
  ]
}
```

---

## 2. API Endpoints Directory

### A. Authentication & Identity (`/api/v1/auth`)

| Method | Endpoint | Access Level | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/login` | **Public** | Authenticate credentials and receive a JWT Bearer token. |
| `GET` | `/api/v1/auth/me` | **Authenticated** | Get profile and assigned facility of current user. |

#### Example: `POST /api/v1/auth/login`
**Request Body:**
```json
{
  "email": "admin@nashamukti.gov.in",
  "password": "Admin@12345"
}
```
**Response (200 OK):**
```json
{
  "success": true,
  "message": "Authentication successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresInSeconds": 28800,
  "user": {
    "id": "admin-guid-001",
    "fullName": "National Administrator",
    "email": "admin@nashamukti.gov.in",
    "role": "Admin",
    "centreId": null,
    "centreName": null
  }
}
```

---

### B. Centre Management (`/api/v1/centres`)

| Method | Endpoint | Access Level | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/centres` | **Authenticated** | List centres with pagination, search, state and status filters. |
| `GET` | `/api/v1/centres/{id}` | **Authenticated** | Retrieve specific centre details. |
| `POST` | `/api/v1/centres` | **Admin Only** | Register a new de-addiction facility. |
| `PUT` | `/api/v1/centres/{id}` | **Admin / Assigned** | Update centre information. |
| `DELETE`| `/api/v1/centres/{id}` | **Admin Only** | Controlled soft deactivation (`IsActive = false`). |

#### Example: `POST /api/v1/centres`
**Headers:** `Authorization: Bearer <JWT_TOKEN>`  
**Request Body:**
```json
{
  "centreName": "Amritsar District Rehabilitation Kendra",
  "stateId": 1,
  "district": "Amritsar",
  "address": "Civil Hospital Complex, Majitha Road",
  "contactNumber": "0183-2223344",
  "email": "amritsar.rehab@nashamukti.gov.in"
}
```
**Response (201 Created):**
```json
{
  "success": true,
  "message": "Centre registered successfully",
  "data": {
    "centreId": 12,
    "centreName": "Amritsar District Rehabilitation Kendra",
    "stateId": 1,
    "stateName": "Punjab",
    "district": "Amritsar",
    "address": "Civil Hospital Complex, Majitha Road",
    "contactNumber": "0183-2223344",
    "email": "amritsar.rehab@nashamukti.gov.in",
    "isActive": true,
    "createdAt": "2026-11-18T04:20:00Z",
    "beneficiariesCount": 0
  }
}
```

---

### C. Beneficiary Management (`/api/v1/beneficiaries`)

| Method | Endpoint | Access Level | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/beneficiaries` | **Scoped** | List patients (filtered to user's centre for `CentreStaff`). |
| `GET` | `/api/v1/beneficiaries/{id}` | **Scoped** | Retrieve complete patient dossier with clinical counts. |
| `POST` | `/api/v1/beneficiaries` | **Scoped** | Register intake (generates `BEN-XX-YYYY-ZZZZ`). |
| `PUT` | `/api/v1/beneficiaries/{id}` | **Scoped** | Update profile details. |
| `DELETE`| `/api/v1/beneficiaries/{id}` | **Scoped** | Soft deactivation (`Status = 'Inactive'`). |

#### Example: `POST /api/v1/beneficiaries`
**Request Body:**
```json
{
  "fullName": "Harpreet Singh",
  "centreId": 1,
  "registrationDate": "2026-11-18",
  "dateOfBirth": "1994-06-15",
  "gender": "Male",
  "contactNumber": "9812345678"
}
```
**Response (201 Created):**
```json
{
  "success": true,
  "message": "Beneficiary registered successfully",
  "data": {
    "beneficiaryId": 105,
    "beneficiaryCode": "BEN-PB-2026-0105",
    "fullName": "Harpreet Singh",
    "centreId": 1,
    "centreName": "Navchetan De-Addiction & Rehabilitation Centre",
    "district": "Amritsar",
    "stateName": "Punjab",
    "registrationDate": "2026-11-18",
    "dateOfBirth": "1994-06-15",
    "gender": "Male",
    "contactNumber": "9812345678",
    "status": "Active",
    "createdAt": "2026-11-18T04:25:00Z"
  }
}
```

---

### D. Counselling Sessions (`/api/v1/counselling`)

| Method | Endpoint | Access Level | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/counselling` | **Scoped** | List sessions (clinical notes omitted for privacy). |
| `GET` | `/api/v1/counselling/{id}` | **Scoped** | Retrieve session record including confidential therapy notes. |
| `POST` | `/api/v1/counselling` | **Scoped** | Record a structured counselling session. |
| `PUT` | `/api/v1/counselling/{id}` | **Scoped** | Update session details and notes. |
| `DELETE`| `/api/v1/counselling/{id}` | **Scoped** | Mark session as Cancelled. |

#### Privacy by Design Rule:
`GET /api/v1/counselling` returns `CounsellingListItemDto` which has **no `notes` property**. Confidential psychological notes are strictly protected and returned only via `GET /api/v1/counselling/{id}` to authorized facility counsellors.

---

### E. Treatment Interventions (`/api/v1/interventions`)

| Method | Endpoint | Access Level | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/interventions` | **Scoped** | List interventions with category/status filters. |
| `GET` | `/api/v1/interventions/{id}` | **Scoped** | Retrieve intervention details. |
| `POST` | `/api/v1/interventions` | **Scoped** | Record a new medical/psychosocial intervention. |
| `PUT` | `/api/v1/interventions/{id}` | **Scoped** | Update intervention status (`Completed`, `Discontinued`). |
| `DELETE`| `/api/v1/interventions/{id}` | **Scoped** | Mark treatment as Discontinued. |

---

### F. Follow-Up Appointments (`/api/v1/followups`)

| Method | Endpoint | Access Level | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/followups` | **Scoped** | List follow-up appointments. |
| `GET` | `/api/v1/followups/{id}` | **Scoped** | Retrieve follow-up details. |
| `POST` | `/api/v1/followups` | **Scoped** | Schedule a future rehabilitation follow-up. |
| `PUT` | `/api/v1/followups/{id}` | **Scoped** | Record completion date, outcome status, and recovery notes. |
| `DELETE`| `/api/v1/followups/{id}` | **Scoped** | Mark appointment as Cancelled. |

---

### G. Analytics & Reports (`/api/v1/reports` and `/api/v1/dashboard`)

| Method | Endpoint | Access Level | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/reports/summary` | **Authenticated** | Returns all 9 high-level KPIs calculated via SQL views/CTEs. |
| `GET` | `/api/v1/reports/centres` | **Authenticated** | Centre-wise summary table matching SQL View `vw_CentreServiceSummary`. |
| `GET` | `/api/v1/dashboard/metrics` | **Authenticated** | Real-time metrics dataset for Chart.js dashboards. |

---

## 3. Postman Collection Guide

The file `NashaMukti_API_Postman_Collection.json` includes pre-scripted authorization handling:

1. **Auto-Token Capture:** The request `1. Authenticate (Login)` runs a Post-response script:
   ```javascript
   if (pm.response.code === 200) {
       var jsonData = pm.response.json();
       pm.collectionVariables.set("jwt_token", jsonData.token);
   }
   ```
2. **Inherited Bearer Authorization:** All other requests in the collection automatically pass `{{jwt_token}}` in their `Authorization` header.
3. **Automated Status Code Assertions:** Requests include automated assertion checks (e.g. `pm.response.to.have.status(200)` and `pm.expect(jsonData.success).to.be.true`).

