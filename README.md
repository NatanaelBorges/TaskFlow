# TaskFlow

A full-stack task management application built with **ASP.NET Core 10** (Clean Architecture + CQRS) on the backend and **Angular 22** (Signals-based state) on the frontend.

---

## Table of Contents

- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Running Locally](#running-locally)
  - [Backend (API)](#backend-api)
  - [Frontend (Web)](#frontend-web)
- [Running the Tests](#running-the-tests)
  - [Backend Tests](#backend-tests)
  - [Frontend Tests](#frontend-tests)
- [API Reference](#api-reference)
- [Assumptions](#assumptions)
- [Key Design Decisions and Trade-offs](#key-design-decisions-and-trade-offs)
- [What I Would Improve or Add Next](#what-i-would-improve-or-add-next)
- [Third-Party Packages and References](#third-party-packages-and-references)

---

## Project Structure

```
TaskFlow/
├── api/
│   └── TaskFlow/
│       ├── TaskFlow.sln
│       ├── src/
│       │   ├── TaskFlow.Api           # Controllers, DI wiring, HTTP pipeline
│       │   ├── TaskFlow.Application   # CQRS handlers, use cases, interfaces
│       │   ├── TaskFlow.Domain        # Entities, value objects, domain logic
│       │   └── TaskFlow.Infrastructure# EF Core, repositories, data access
│       └── test/
│           └── TaskFlow.UnitTests     # xUnit + NSubstitute unit tests
└── web/
    └── task-flow/                     # Angular 22 SPA
        ├── src/
        │   ├── app/
        │   │   ├── core/              # Services, models, interceptors
        │   │   ├── features/tasks/    # Tasks feature module (lazy loaded)
        │   │   └── shared/            # Toast, ConfirmDialog components
        │   ├── environments/          # Dev / prod environment config
        │   └── main.ts
        ├── angular.json
        └── package.json
```

---

## Prerequisites

Before running anything, make sure the following tools are installed.

### Backend

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | **10.0** | https://dotnet.microsoft.com/download/dotnet/10.0 |
| Git | Any recent | https://git-scm.com |

Verify your installation:

```bash
dotnet --version
# Expected: 10.0.x
```

> **HTTPS dev certificate** — .NET requires a trusted local HTTPS certificate. If you have never set one up, run the following once:
> ```bash
> dotnet dev-certs https --trust
> ```
> Accept the security prompt so the browser trusts `https://localhost:7045`.

### Frontend

| Tool | Version | Download |
|------|---------|----------|
| Node.js | **22 or later** | https://nodejs.org/en/download |
| npm | **11 or later** (bundled with Node 22) | — |
| Git | Any recent | https://git-scm.com |

Verify your installation:

```bash
node --version
# Expected: v22.x.x

npm --version
# Expected: 11.x.x
```

> **No global Angular CLI required** — the project uses the version pinned in `devDependencies` via `npx` / `npm run`.

---

## Running Locally

Both services must run at the same time. Open **two separate terminals**.

### Backend (API)

```bash
# 1. Navigate to the solution root
cd api/TaskFlow

# 2. Restore NuGet packages (only needed the first time or after dependency changes)
dotnet restore

# 3. Start the API in Development mode
dotnet run --project src/TaskFlow.Api/TaskFlow.Api.csproj
```

The API will start and listen on:

| Protocol | URL |
|----------|-----|
| HTTPS | `https://localhost:7045` |
| HTTP  | `http://localhost:5121`  |

**Interactive API documentation** (Scalar UI) is available at:

```
https://localhost:7045/scalar/v1
```

**OpenAPI JSON spec** is available at:

```
https://localhost:7045/openapi/v1.json
```

**Health check** endpoint:

```
GET https://localhost:7045/api/healthcheck/ping
```

> The application uses an **in-memory database** — no database engine, connection strings, or migrations are needed. All data resets on each restart; this is by design for the assessment scope.

---

### Frontend (Web)

> The API must be running before starting the frontend; otherwise all HTTP calls will fail with a connection error.

```bash
# 1. Navigate to the Angular workspace
cd web/task-flow

# 2. Install dependencies (only needed the first time or after package.json changes)
npm install

# 3. Start the development server
npm start
```

The dev server will compile and serve the app at:

```
http://localhost:4200
```

Angular CLI will watch for file changes and reload the browser automatically.

**Environment configuration** — the dev build points to the API via `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7045',
};
```

No changes are needed for local development.

---

## Running the Tests

### Backend Tests

The test project lives at `api/TaskFlow/test/TaskFlow.UnitTests/` and uses **xUnit 2.9** with **NSubstitute 5.3** for mocking.

```bash
# From the solution root
cd api/TaskFlow

# Run all tests
dotnet test

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run with code coverage (requires coverlet, already in the project)
dotnet test --collect:"XPlat Code Coverage"
```

**What is tested:**

| Area | Test File |
|------|-----------|
| `TaskTitle` value object validation | `TaskTitleTests.cs` |
| `TaskItem` domain entity logic | `TaskItemTests.cs` |
| `Create` command handler | `CreateHandlerTests.cs` |
| `Update` command handler | `UpdateHandlerTests.cs` |
| `Patch` (toggle status) handler | `PatchHandlerTests.cs` |
| `Delete` command handler | `DeleteHandlerTests.cs` |
| `GetAll` query handler | `GetAllHandlerTests.cs` |
| `GetById` query handler | `GetByIdHandlerTests.cs` |
| `TaskRepository` data access | `TaskRepositoryTests.cs` |
| `TasksController` HTTP layer | `TasksControllerTests.cs` |
| `TaskLinker` HATEOAS links | `TaskLinkerTests.cs` |
| `PaginationLinksHelper` | `PaginationLinksHelperTests.cs` |

---

### Frontend Tests

The Angular project uses **Vitest 4** as the test runner with **jsdom** as the DOM environment.

```bash
# From the Angular workspace
cd web/task-flow

# Install dependencies if you have not already
npm install

# Run tests once (CI mode)
npm test

# Run tests in watch mode (interactive)
npx ng test --watch
```

**What is tested:**

| Area | Test File |
|------|-----------|
| `Task` model mapping functions | `task.model.spec.ts` |
| `TaskService` signal-based state | `task.service.spec.ts` |
| Root `AppComponent` bootstrap | `app.spec.ts` |

---

## API Reference

**Base URL:** `https://localhost:7045/api/v1`

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/tasks` | List tasks (supports `page`, `pageSize`, `status` query params) |
| `GET` | `/tasks/{id}` | Get a single task by ID |
| `POST` | `/tasks` | Create a new task |
| `PUT` | `/tasks/{id}` | Update a task's title and description |
| `PATCH` | `/tasks/{id}/status` | Toggle a task's status (Active ↔ Completed) |
| `DELETE` | `/tasks/{id}` | Soft-delete a task |

**Pagination query parameters for `GET /tasks`:**

| Param | Default | Description |
|-------|---------|-------------|
| `page` | `1` | Page number (1-based) |
| `pageSize` | `10` | Items per page (max 100) |
| `status` | _(all)_ | Filter by `active` or `completed` |

**Example paginated response:**

```json
{
  "data": [ { "id": "...", "title": "...", "description": "...", "status": "Active", "createdAtUtc": "...", "updatedAtUtc": null, "_links": { ... } } ],
  "page": 1,
  "pageSize": 10,
  "totalItems": 3,
  "totalPages": 1,
  "links": {
    "self":  { "href": "/api/v1/tasks?page=1&pageSize=10", "method": "GET" },
    "first": { "href": "/api/v1/tasks?page=1&pageSize=10", "method": "GET" },
    "last":  { "href": "/api/v1/tasks?page=1&pageSize=10", "method": "GET" },
    "next": null,
    "prev": null
  }
}
```

---

## Assumptions

1. **In-memory persistence is sufficient for this assessment.** No external database is configured; all data is held in memory for the lifetime of the API process. The infrastructure layer is designed to make swapping to a real database (e.g. SQL Server, PostgreSQL) straightforward — only the `AddDatabase()` registration and EF Core provider need to change.

2. **No authentication is required.** All endpoints are publicly accessible. The architecture does not preclude adding auth (e.g. JWT Bearer), but it was outside the stated scope.

3. **CORS is locked to `http://localhost:4200` in development.** This covers the standard `ng serve` port. If a different port is needed, `AllowedOrigins` in `appsettings.Development.json` can be updated without code changes.

4. **Soft deletes are the correct deletion strategy.** Tasks are never physically removed from the database; a `DeletedAtUtc` timestamp is set instead. All queries filter deleted records out automatically via the repository layer.

5. **`TaskStatus` is serialised as a string enum in the API (`"Active"`, `"Completed"`) and as a numeric enum in the Angular client (`0`, `1`).** The frontend maps between the two in `task.model.ts`.

6. **Title length is capped at 200 characters; description at 2 000 characters.** These limits are enforced in both the `TaskTitle` domain value object (backend) and the reactive form validators (frontend). The frontend constraint is slightly stricter on the title minimum (3 chars vs. 1 char on the backend) to promote useful entries.

7. **Only `ng serve` (development server) is needed for local evaluation.** SSR (`@angular/ssr` + Express 5) is scaffolded in the project and a production build can be served with `npm run serve:ssr:task-flow`, but it is not required for running the app locally.

---

## Key Design Decisions and Trade-offs

### Backend

**Clean Architecture + CQRS.** The goal was to make each layer independently testable and replaceable. The domain has zero framework dependencies; the application layer only knows about its own abstractions; infrastructure and HTTP wiring live at the edges. CQRS fits naturally here — each handler does one thing, which means tests are small and focused and adding a new operation never touches existing code. The honest cost is four projects and more ceremony than a CRUD app of this size strictly needs. I made that trade consciously because the structure demonstrates how I'd build something meant to grow.

**Result pattern over exceptions.** Handlers return `Result<T>` instead of throwing for expected failures (not found, validation error). Controllers read the `ErrorType` and map it to the right HTTP status. The reason is that exceptions for control flow hide error paths — a handler that throws makes the caller responsible for knowing what might blow up. `Result<T>` makes all outcomes visible at the call site and the compiler will catch an unhandled case. The downside is a bit more boilerplate per handler, which is a fair trade.

**`TaskTitle` value object.** Title validation — not empty, max 200 characters — lives in a dedicated record type with a private constructor and a `Create()` factory. The alternative is a plain `string` with guard clauses duplicated in every handler that touches a title. The value object makes an invalid title unrepresentable, which is the whole point of the pattern. It adds a tiny amount of code and pays for itself the moment a second write operation needs the same rule.

**HATEOAS links.** Every task response carries a `_links` object with the actions currently available on that resource (`complete` vs. `reopen` depending on status, pagination links on list responses). The benefit is that the client doesn't need to reconstruct URLs or conditionally guess which actions are valid — the API tells it. For a single known frontend this coupling is admittedly low-risk, but it keeps the API self-describing and makes it straightforward to consume from a second client without reading the source.

**In-memory database.** A deliberate convenience choice: anyone checking out the repo can run the API with no setup. The repository abstraction and EF Core configuration are already production-shaped — swapping to PostgreSQL or SQL Server is a one-line change in `AddDatabase()`. The obvious limitation is that data resets on every restart.

---

### Frontend

**Angular Signals for state.** `TaskService` holds all mutable state in `signal()` primitives and exposes them as read-only. Components read signals directly in their templates without any subscription boilerplate. I chose this over NgRx because for a single-feature app a full store is overkill, and over RxJS subjects because signals integrate with Angular's change detection without needing `async` pipes or manual `takeUntilDestroyed` wiring. If the app grew to have cross-feature shared state (e.g. a logged-in user, a notification bus) I'd reconsider a dedicated store.

**Smart / Presentational split.** `TasksPageComponent` is the only component that calls the service. Everything below it — `TaskListComponent`, `TaskCardComponent`, `TaskFormComponent`, `TaskFiltersComponent` — receives data via inputs and emits events via outputs. The split makes presentational components easy to test without mocking anything and easy to reuse. The cost is more files and a discipline rule that's easy to accidentally break.

**Two-tier error handling.** The `errorInterceptor` catches network failures (`status === 0`) and server crashes (`5xx`) and shows a generic toast — the user can't act on these beyond retrying. HTTP `4xx` responses are left to propagate to the service, which handles them in context: a `404` on a delete is surfaced differently than a `422` on a create. Collapsing both tiers into one global handler would lose that context; letting `5xx` bubble to the component would scatter the same "server error" toast across every operation.

**Tailwind CSS v4.** Every component is styled with utility classes inline; there are no component-scoped `.css` files. Tailwind's constraint — you only use the design tokens it defines — is what keeps the UI visually consistent without a design system. The trade-off is that complex layouts produce long class lists on a single element, which can be hard to scan at a glance.

---

## What I Would Improve or Add Next

### Backend

1. **Persistent database** — swap the in-memory provider for PostgreSQL or SQL Server with EF Core migrations and a seeded development dataset.
2. **Authentication and authorisation** — add JWT Bearer authentication (ASP.NET Core Identity or a lightweight token service) so tasks are scoped to a logged-in user.
3. **Integration / end-to-end tests** — add an `IntegrationTests` project using `WebApplicationFactory<Program>` and a real (or test-container) database to cover the full HTTP stack.
4. **Validation middleware** — move request DTO validation to `FluentValidation` with a pipeline behaviour in MediatR (if adopted), keeping validation concerns out of handlers.
5. **MediatR** — currently all CQRS handlers are manually registered and called via concrete types. Introducing MediatR would decouple dispatching from the API layer and open the door to cross-cutting pipeline behaviours (logging, validation, caching).
6. **Structured logging and observability** — replace the default logger with Serilog/OpenTelemetry and add correlation IDs to all API responses.
7. **Docker support** — add a `Dockerfile` and `docker-compose.yml` so the API + database can be started with a single command.

### Frontend

1. **Authentication UI** — login/register forms and route guards; store the JWT in an HTTP-only cookie.
2. **Optimistic UI updates** — update the local signal immediately on mutation, then reconcile (or roll back) when the server responds, reducing perceived latency.
3. **Drag-and-drop reordering** — allow tasks to be manually reordered within a list using the `@angular/cdk/drag-drop` module.
4. **Due dates and priority** — add `dueDate` and `priority` fields to the task model and surface them in the card and form.
5. **Accessibility audit** — add ARIA roles, keyboard navigation for the modal and confirmation dialog, and a proper focus trap.
6. **E2E tests** — add Playwright tests covering the critical paths: create, complete, delete, pagination, and filter.
7. **Pagination with URL state** — sync the current page and filter to query params so deep links and browser back/forward work correctly.
8. **Dark mode** — Tailwind v4 supports `@variant dark` out of the box; a toggle could be added with minimal effort.

---

## Third-Party Packages and References

### Backend

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.EntityFrameworkCore.InMemory` | 10.0.0 | In-memory database provider for development |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.0 | EF Core tooling support |
| `Scalar.AspNetCore` | 2.14.14 | OpenAPI interactive documentation UI (alternative to Swagger UI) |
| `xunit` | 2.9.3 | Unit test framework |
| `xunit.runner.visualstudio` | 3.1.4 | Visual Studio / `dotnet test` test runner integration |
| `NSubstitute` | 5.3.0 | Mocking library for unit tests |
| `coverlet.collector` | 6.0.4 | Code coverage data collector |
| `Microsoft.NET.Test.Sdk` | 17.14.1 | MSBuild test infrastructure |

**Documentation and references consulted:**

- [ASP.NET Core Clean Architecture — Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [CQRS pattern — Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- [Result pattern in C# — various community articles](https://enterprisecraftsmanship.com/posts/error-handling-exception-or-result/)
- [EF Core In-Memory Provider — Microsoft Docs](https://learn.microsoft.com/en-us/ef/core/providers/in-memory/)
- [Scalar.AspNetCore — scalar.com/docs](https://scalar.com/docs)
- [HATEOAS — RFC 5988, REST API Design Rulebook (Masse)](https://restfulapi.net/hateoas/)

### Frontend

| Package | Version | Purpose |
|---------|---------|---------|
| `@angular/*` | 22.0.0 | Core framework (components, router, forms, SSR, HTTP client) |
| `rxjs` | ~7.8.0 | Reactive streams (used internally by Angular; HTTP calls) |
| `tailwindcss` | ^4.1.12 | Utility-first CSS framework |
| `@tailwindcss/postcss` | ^4.1.12 | PostCSS plugin for Tailwind v4 |
| `postcss` | ^8.5.3 | CSS transformation pipeline |
| `vitest` | ^4.0.8 | Fast unit test runner (Vite-based) |
| `jsdom` | ^28.0.0 | DOM environment for Vitest tests |
| `prettier` | ^3.8.1 | Code formatter |
| `typescript` | ~6.0.2 | Type system |
| `express` | ^5.1.0 | Node.js server for Angular SSR |

**Documentation and references consulted:**

- [Angular Documentation — angular.dev](https://angular.dev)
- [Angular Signals guide — angular.dev/guide/signals](https://angular.dev/guide/signals)
- [Angular Standalone Components — angular.dev/guide/components](https://angular.dev/guide/components)
- [Angular HTTP Client — angular.dev/guide/http](https://angular.dev/guide/http)
- [Tailwind CSS v4 — tailwindcss.com/docs](https://tailwindcss.com/docs/installation)
- [Vitest — vitest.dev](https://vitest.dev)

---

> By submitting this assessment, I confirm that the design, code, tests, and documentation are my own work.
