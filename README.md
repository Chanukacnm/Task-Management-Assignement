# Task Management Application

A full-stack task management application built with **.NET 9 Web API** (Clean Architecture + CQRS) on the backend, **Angular 19** on the frontend, and **Microsoft SQL Server** for storage.

It supports the full task lifecycle — create, read, update, delete, and mark complete — with **HTTP Basic authentication**, request validation, robust error handling, and a responsive UI featuring a **side-by-side task list and add/edit form** with **sorting and filtering**.

---

## Table of contents

- [Features](#features)
- [Tech stack](#tech-stack)
- [Architecture](#architecture)
- [Repository structure](#repository-structure)
- [Prerequisites](#prerequisites)
- [Getting started](#getting-started)
  - [1. Database](#1-database)
  - [2. Backend API](#2-backend-api)
  - [3. Frontend](#3-frontend)
- [Default credentials](#default-credentials)
- [API reference](#api-reference)
- [Authentication](#authentication)
- [Configuration](#configuration)
- [How the requirements are met](#how-the-requirements-are-met)
- [Notes & assumptions](#notes--assumptions)

---

## Features

**Backend**
- **Clean Architecture** (Domain / Application / Infrastructure / Api) with a **CQRS** application layer (MediatR).
- **Repository + Unit of Work** pattern keeping the Application layer persistence-ignorant.
- RESTful CRUD API for tasks (`GET`/`POST`/`PUT`/`PATCH`/`DELETE`).
- HTTP **Basic authentication** on all task endpoints (no JWT, per the requirements).
- Passwords stored as **salted PBKDF2-SHA256** hashes (600k iterations, per OWASP).
- Request **validation** (FluentValidation) plus **logging**, **performance** and **exception** MediatR pipeline behaviours.
- Centralised **error handling** returning consistent `ProblemDetails` (RFC 7807) responses.
- **Server-side sorting and filtering** (by status, priority, free-text search) with deterministic ordering.
- **Rate limiting** on the login endpoint to throttle brute-force attempts.
- Interactive **Swagger / OpenAPI** documentation with a built-in "Authorize" button.

**Frontend**
- Username/password **login** screen.
- **List and add/edit form shown side by side** on one screen.
- Add, edit, delete, and toggle-complete tasks.
- **Sorting** (created, title, priority, due date, status) and **filtering** (status, priority, search).
- Toast **notifications** for every action and friendly empty/error states.
- Responsive layout and clean styling.

---

## Tech stack

| Layer      | Technology                                                            |
| ---------- | --------------------------------------------------------------------- |
| Backend    | .NET 9, ASP.NET Core Web API, MediatR (CQRS), FluentValidation        |
| Data       | Entity Framework Core 9, Microsoft SQL Server (2017+ / Express)       |
| Auth       | HTTP Basic authentication, PBKDF2-SHA256 password hashing             |
| Docs       | Swagger / Swashbuckle                                                 |
| Frontend   | Angular 19 (standalone components, signals), TypeScript, RxJS, SCSS   |

---

## Architecture

The backend follows **Clean Architecture** with a **CQRS** application layer. Dependencies point **inwards** only:

```
┌──────────────────────────────────────────────────────────────┐
│  TaskManagement.Api            (Controllers, Basic auth,       │
│                                 middleware, Swagger, DI host)  │
│      │ depends on                                              │
│      ▼                                                         │
│  TaskManagement.Infrastructure (EF Core DbContext, repository  │
│                                 + unit-of-work implementations,│
│                                 PBKDF2 hasher, seeding)        │
│      │ depends on                                              │
│      ▼                                                         │
│  TaskManagement.Application    (CQRS: Commands/Queries +       │
│                                 Handlers, Validators, DTOs,    │
│                                 pipeline behaviours, abstract  │
│                                 repository interfaces)         │
│      │ depends on                                              │
│      ▼                                                         │
│  TaskManagement.Domain         (Entities, Enums — no deps)     │
└──────────────────────────────────────────────────────────────┘
```

- **CQRS** — every operation is a `Command` (writes) or `Query` (reads) dispatched through MediatR to a dedicated handler, e.g. `CreateTaskCommand`, `UpdateTaskCommand`, `DeleteTaskCommand`, `SetTaskCompletionCommand`, `GetTasksQuery`, `GetTaskByIdQuery`, `ValidateCredentialsQuery`.
- **Repository + Unit of Work** — handlers depend only on `ITaskRepository`, `IUserRepository` and `IUnitOfWork` (defined in Application). The EF Core implementations live in Infrastructure, so the **Application layer is fully persistence-ignorant and has no reference to EF Core**.
- **MediatR pipeline behaviours** wrap every request as cross-cutting concerns, in order: `UnhandledExceptionBehaviour` → `LoggingBehaviour` → `PerformanceBehaviour` → `ValidationBehaviour` (FluentValidation).
- Dependencies point inwards only (Dependency Inversion): Domain knows nothing of the outer layers, and Application knows nothing of the database technology.

---

## Repository structure

```
Assignment/
├─ backend/                              # .NET 9 solution (Clean Architecture + CQRS)
│  ├─ TaskManagement.sln
│  └─ src/
│     ├─ TaskManagement.Domain/          # Entities, enums
│     ├─ TaskManagement.Application/     # CQRS commands/queries, validators, pipeline behaviours,
│     │                                  #   DTOs, repository/unit-of-work interfaces
│     ├─ TaskManagement.Infrastructure/  # EF Core DbContext, repositories, unit of work,
│     │                                  #   migrations, PBKDF2 hasher, seeding
│     └─ TaskManagement.Api/             # Controllers, Basic auth, middleware, rate limiting, Program.cs
├─ frontend/                             # Angular 19 application
│  └─ src/app/
│     ├─ core/        # models, services, HTTP interceptor, route guard
│     ├─ features/    # login, tasks (tasks-page + task-list + task-form)
│     └─ shared/      # toast container
├─ database/
│  ├─ schema.sql                         # Creates the database, tables and indexes (idempotent)
│  └─ seed.sql                           # Inserts the default user and sample tasks
├─ .gitignore
└─ README.md
```

---

## Prerequisites

- [.NET SDK 9](https://dotnet.microsoft.com/download) (the backend targets `net9.0`)
- [Node.js 20+ / 22](https://nodejs.org/) and npm (Angular 19)
- **Microsoft SQL Server** 2017+ or **SQL Server Express** (a local default instance, `localhost`)

> The connection string uses Windows Authentication against `localhost`. See [Configuration](#configuration) to change the server, instance, or to use SQL authentication / LocalDB.

---

## Getting started

Clone the repository, then follow the three steps below.

### 1. Database

You have **two options**:

**Option A — automatic (recommended).** The API applies EF Core migrations and seeds the default user + sample tasks **automatically on first startup**. You don't need to run any SQL by hand — just make sure SQL Server is running and start the backend (step 2).

**Option B — run the scripts manually.** Use the provided scripts in `database/`:

```bash
# from the repository root
sqlcmd -S localhost -E -i database/schema.sql
sqlcmd -S localhost -E -i database/seed.sql
```

`schema.sql` creates the `TaskManagementDb` database, tables and indexes (idempotent). `seed.sql` inserts the default `admin` user and five sample tasks.

### 2. Backend API

```bash
cd backend
dotnet restore
dotnet run --project src/TaskManagement.Api --launch-profile http
```

The API starts on **http://localhost:5142**.

- Swagger UI: **http://localhost:5142/swagger**
- All `/api/tasks` endpoints require Basic authentication (use the "Authorize" button in Swagger, or send an `Authorization: Basic …` header).

### 3. Frontend

In a separate terminal:

```bash
cd frontend
npm install
npm start          # or: ng serve
```

The app starts on **http://localhost:4200**. Open it in a browser and sign in.

> The frontend calls the API at `http://localhost:5142/api`. If you change the API port, update `frontend/src/environments/environment.ts`.

---

## Default credentials

| Username | Password    |
| -------- | ----------- |
| `admin`  | `Passw0rd!` |

---

## API reference

All task endpoints require an `Authorization: Basic <base64(username:password)>` header.

| Method   | Endpoint                      | Description                              | Success |
| -------- | ----------------------------- | ---------------------------------------- | ------- |
| `POST`   | `/api/auth/login`             | Validate credentials, return the user    | `200`   |
| `GET`    | `/api/auth/me`                | Return the current authenticated user    | `200`   |
| `GET`    | `/api/tasks`                  | List tasks (supports filtering/sorting)  | `200`   |
| `GET`    | `/api/tasks/{id}`             | Get a single task                        | `200`   |
| `POST`   | `/api/tasks`                  | Create a task                            | `201`   |
| `PUT`    | `/api/tasks/{id}`             | Update a task                            | `200`   |
| `PATCH`  | `/api/tasks/{id}/status`      | Toggle completion (`{ "isCompleted" }`)  | `200`   |
| `DELETE` | `/api/tasks/{id}`             | Delete a task                            | `204`   |

**`GET /api/tasks` query parameters**

| Parameter  | Values                                            | Description                |
| ---------- | ------------------------------------------------- | -------------------------- |
| `status`   | `all` · `active` · `completed`                    | Filter by completion       |
| `priority` | `Low` · `Medium` · `High`                         | Filter by priority         |
| `search`   | any text                                          | Search title & description |
| `sortBy`   | `created` · `title` · `priority` · `duedate` · `status` | Sort field           |
| `sortDir`  | `asc` · `desc`                                    | Sort direction             |

Errors are returned as `ProblemDetails`. Validation failures (`400`) include a per-field `errors` object, e.g.:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "Title": ["Title is required."] }
}
```

---

## Authentication

As required, authentication uses a **simple username/password mechanism with HTTP Basic auth — no JWT**.

- The Angular login form sends the credentials; on success they are Base64-encoded and kept in `sessionStorage`.
- An HTTP **interceptor** attaches the `Authorization: Basic …` header to every API request.
- A **route guard** redirects unauthenticated users to the login page; a `401` response signs the user out automatically.
- On the server, a custom `BasicAuthenticationHandler` decodes the header and validates the credentials against the database (PBKDF2-hashed passwords) via a CQRS query.

---

## Configuration

**Backend** — `backend/src/TaskManagement.Api/appsettings.json`:

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Cors": { "AllowedOrigins": [ "http://localhost:4200" ] }
}
```

Common connection-string alternatives:

- **SQL Server Express named instance:** `Server=localhost\\SQLEXPRESS;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True`
- **LocalDB:** `Server=(localdb)\\MSSQLLocalDB;Database=TaskManagementDb;Trusted_Connection=True`
- **SQL authentication:** `Server=localhost;Database=TaskManagementDb;User Id=sa;Password=Your_Password;TrustServerCertificate=True`

**Frontend** — `frontend/src/environments/environment.ts` exposes `apiBaseUrl`.

The API port is defined in `backend/src/TaskManagement.Api/Properties/launchSettings.json` (`http` profile → `5142`).

---

## Notes & assumptions

- The API auto-creates and seeds the database on startup for convenience; in production you would gate migrations behind a deliberate step.
- HTTP Basic auth was chosen deliberately because the brief asked for a *simple username/password mechanism with no JWT*. Basic credentials are kept in `sessionStorage` and re-sent on every request. Because the credentials are reversible, the API enforces **HTTPS redirection + HSTS outside Development** and the login endpoint is **rate-limited**; a production system would still prefer short-lived tokens. A strict Content-Security-Policy is recommended when deploying the SPA.
- Due dates are treated as calendar dates (stored at UTC midnight) and displayed/compared in UTC so the selected day is shown consistently regardless of the viewer's timezone.
- Deleting a task asks for confirmation in the browser before the request is sent.
